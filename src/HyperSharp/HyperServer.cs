using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using HyperSharp.Protocol;
using HyperSharp.Results;
using HyperSharp.Setup;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace HyperSharp
{
    /// <summary>
    /// Represents a high-level HTTP server for handling incoming connections and requests.
    /// </summary>
    public sealed class HyperServer
    {
        /// <summary>
        /// Gets the configuration settings for the <see cref="HyperServer"/>.
        /// </summary>
        public HyperConfiguration Configuration { get; init; }

        /// <summary>
        /// If no logger was specified in the constructor, this will be a <see cref="NullLogger{T}"/> instance.
        /// </summary>
        private readonly ILogger<HyperServer> _logger;

        /// <summary>
        /// Dictionary to store currently open connections by their unique IDs.
        /// </summary>
        private readonly ConcurrentDictionary<Ulid, Task> _openConnections = new();

        /// <summary>
        /// Stack of cancellation token sources for reuse in handling connections.
        /// </summary>
        private readonly ConcurrentStack<CancellationTokenSource> _cancellationTokenSources = new();

        /// <summary>
        /// The TCP listener instance for accepting incoming connections.
        /// </summary>
        private TcpListener? _tcpListener;

        /// <summary>
        /// The main cancellation token source for controlling the server's lifecycle.
        /// </summary>
        private CancellationTokenSource? _mainCancellationTokenSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="HyperServer"/> class with the specified configuration and optional logger.
        /// </summary>
        /// <param name="configuration">The configuration settings for the server.</param>
        /// <param name="logger">Optional logger to log server activities.</param>
        public HyperServer(HyperConfiguration configuration, ILogger<HyperServer>? logger = null)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? NullLogger<HyperServer>.Instance;
        }

        /// <summary>
        /// Starts the server, listening for incoming connections.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to stop the server.</param>
        /// <exception cref="InvalidOperationException">Thrown if the server is already running.</exception>
        public void Start(CancellationToken cancellationToken = default)
        {
            if (_mainCancellationTokenSource is not null)
            {
                throw new InvalidOperationException("The server is already running.");
            }

            HyperLogging.ServerStarting(_logger, Configuration.ListeningEndpoint, null);
            _mainCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _tcpListener = new(Configuration.ListeningEndpoint);
            Task.Factory.StartNew(ListenForConnectionsAsync, _mainCancellationTokenSource.Token, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach | TaskCreationOptions.PreferFairness, TaskScheduler.Default);
            HyperLogging.ServerStarted(_logger, Configuration.ListeningEndpoint, null);
        }

        /// <summary>
        /// Stops the server, blocking until all connections have been closed.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the server is not running.</exception>
        public async Task StopAsync()
        {
            if (_mainCancellationTokenSource is null)
            {
                throw new InvalidOperationException("The server is not running.");
            }

            HyperLogging.ServerStopping(_logger, Configuration.ListeningEndpoint, null);
            _mainCancellationTokenSource.Cancel();
            _mainCancellationTokenSource.Dispose();
            _mainCancellationTokenSource = null;
            while (!_openConnections.IsEmpty)
            {
                HyperLogging.ConnectionsPending(_logger, _openConnections.Count, null);
                await Task.WhenAll(_openConnections.Values);
            }

            // We don't call this earlier since ListenForConnectionsAsync will break out of it's while loop.
            // Since the cancellation token is cancelled, no new connections will be accepted.
            _tcpListener?.Stop();
            HyperLogging.ServerStopped(_logger, Configuration.ListeningEndpoint, null);
        }

        /// <summary>
        /// Listens for incoming connections asynchronously, accepting them and throwing them onto the async thread pool.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task ListenForConnectionsAsync()
        {
            _tcpListener!.Start();
            while (!_mainCancellationTokenSource!.IsCancellationRequested)
            {
                // Wait for a new connection.
                TcpClient client = await _tcpListener.AcceptTcpClientAsync(_mainCancellationTokenSource.Token);

                // Regenerate the ID until we get a unique one.
                Ulid id;
                Task task;
                do
                {
                    id = Ulid.NewUlid();
                    task = new Task<Task>(() => HandleConnectionAsync(id, client.GetStream()), _mainCancellationTokenSource.Token);
                } while (!_openConnections.TryAdd(id, task));
                task.Start();
            }
        }

        /// <summary>
        /// Handles an incoming connection asynchronously, parsing the HTTP headers and executing any registered responders.
        /// </summary>
        /// <param name="id">The unique ID of the incoming connection, different from <see cref="HyperConnection.Id"/>.</param>
        /// <param name="networkStream">The network stream for the incoming connection.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task HandleConnectionAsync(Ulid id, NetworkStream networkStream)
        {
            HyperConnection connection = new(networkStream, this);
            HyperLogging.ConnectionOpened(_logger, connection.Id, null);

            // Try to reuse an existing cancellation token source. If none are available, create a new one.
            if (!_cancellationTokenSources.TryPop(out CancellationTokenSource? cancellationTokenSource))
            {
                cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_mainCancellationTokenSource!.Token);
            }

            // Ensure all connections will close after the configured timeout.
            cancellationTokenSource.CancelAfter(Configuration.Timeout);

            // Start parsing the HTTP Headers.
            Result<HyperContext> context = await HyperHeaderParser.TryParseHeadersAsync(Configuration.MaxHeaderSize, connection, cancellationTokenSource.Token);
            if (cancellationTokenSource.IsCancellationRequested)
            {
                // Intentionally don't pass the cancellation token to RespondAsync, since we want to respond with a 500.
                await context.Value!.RespondAsync(HyperStatus.InternalServerError(), HyperSerializers.JsonAsync, CancellationToken.None);
            }
            else if (!context.IsSuccess)
            {
                // If the headers are invalid, log the errors and close the connection without responding.
                HyperLogging.HttpInvalidHeaders(_logger, connection.Id, context.Errors, null);
            }
            else
            {
                // Execute any registered responders.
                HyperLogging.HttpReceivedRequest(_logger, connection.Id, context.Value!.Route, null);
                Result<HyperStatus> status = await Configuration.RespondersDelegate(context.Value, cancellationTokenSource.Token);
                if (!context.Value.HasResponded)
                {
                    HyperLogging.HttpResponding(_logger, connection.Id, status.Value, null);
                    HyperStatus response = status.Status switch
                    {
                        _ when cancellationTokenSource.IsCancellationRequested => HyperStatus.InternalServerError(),
                        ResultStatus.IsSuccess | ResultStatus.HasValue => status.Value,
                        ResultStatus.IsSuccess => HyperStatus.OK(),
                        ResultStatus.HasValue => HyperStatus.InternalServerError(status.Value.Headers, status.Value.Body),
                        ResultStatus.None => HyperStatus.InternalServerError(),
                        _ => throw new NotImplementedException("Unimplemented result status, please open a GitHub issue as this is a bug.")
                    };

                    await context.Value.RespondAsync(response, cancellationTokenSource.Token);
                    HyperLogging.HttpResponded(_logger, connection.Id, response, null);
                }
            }

            HyperLogging.ConnectionClosing(_logger, connection.Id, null);
            await connection.DisposeAsync();

            _openConnections.TryRemove(id, out Task? thisTask);
            if (cancellationTokenSource.TryReset())
            {
                _cancellationTokenSources.Push(cancellationTokenSource);
            }
            else
            {
                // Dispose of the token source if it's not reusable.
                cancellationTokenSource.Dispose();
            }

            HyperLogging.ConnectionClosed(_logger, connection.Id, null);
        }
    }
}
