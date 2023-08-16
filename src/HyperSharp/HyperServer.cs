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
        private readonly ConcurrentDictionary<Ulid, HyperConnection> _openConnections = new();

        /// <summary>
        /// Stack of cancellation token sources for reuse in handling connections.
        /// </summary>
        private readonly ConcurrentStack<CancellationTokenSource> _cancellationTokenSources = new();

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
            TcpListener listener = new(Configuration.ListeningEndpoint);
            _mainCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _mainCancellationTokenSource.Token.Register(() =>
            {
                HyperLogging.ServerStopping(_logger, Configuration.ListeningEndpoint, null);

                if (!_openConnections.IsEmpty)
                {
                    HyperLogging.ConnectionsPending(_logger, _openConnections.Count, null);
                    while (!_openConnections.IsEmpty)
                    {
                        // I'm dying inside.
                        Thread.Sleep(100);
                    }
                }

                listener.Stop();
                HyperLogging.ServerStopped(_logger, Configuration.ListeningEndpoint, null);
            });

            listener.Start();
            _ = ListenForConnectionsAsync(listener);
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

            await _mainCancellationTokenSource.CancelAsync();
            _mainCancellationTokenSource.Dispose();
            _mainCancellationTokenSource = null;
        }

        /// <summary>
        /// Listens for incoming connections asynchronously, accepting them and throwing them onto the async thread pool.
        /// </summary>
        /// <param name="listener">The TCP listener instance to accept connections from.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task ListenForConnectionsAsync(TcpListener listener)
        {
            while (!_mainCancellationTokenSource!.IsCancellationRequested)
            {
                // Throw the connection onto the async thread pool and wait for the next connection.
                _ = HandleConnectionAsync(await listener.AcceptTcpClientAsync());
            }
        }

        /// <summary>
        /// Handles an incoming connection asynchronously, parsing the HTTP headers and executing any registered responders.
        /// </summary>
        /// <param name="client">The connected TCP client.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task HandleConnectionAsync(TcpClient client)
        {
            HyperConnection connection = new(client, this);
            _openConnections.TryAdd(connection.Id, connection);
            HyperLogging.ConnectionOpened(_logger, connection.RemoteEndPoint, connection.Id, null);

            // Try to reuse an existing cancellation token source. If none are available, create a new one.
            if (!_cancellationTokenSources.TryPop(out CancellationTokenSource? cancellationTokenSource))
            {
                cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_mainCancellationTokenSource!.Token);
            }
            cancellationTokenSource.CancelAfter(Configuration.Timeout);

            // Start parsing the HTTP Headers.
            await using NetworkStream networkStream = client.GetStream();
            Result<HyperContext> context = await HyperHeaderParser.TryParseHeadersAsync(Configuration.MaxHeaderSize, connection, cancellationTokenSource.Token);
            if (context.IsSuccess)
            {
                // Execute any registered responders.
                HyperLogging.HttpReceivedRequest(_logger, connection.Id, context.Value!.Route, null);
                Result<HyperStatus> status = await Configuration.RespondersDelegate(context.Value, cancellationTokenSource.Token);
                if (!context.Value.HasResponded)
                {
                    HyperLogging.HttpResponding(_logger, connection.Id, status.Value, null);

                    await context.Value.RespondAsync(status.Status switch
                    {
                        ResultStatus.IsSuccess | ResultStatus.HasValue => status.Value,
                        ResultStatus.IsSuccess => HyperStatus.OK(),
                        _ => HyperStatus.InternalServerError()
                    }, Configuration.JsonSerializerOptions);

                    HyperLogging.HttpResponded(_logger, connection.Id, status.Value, null);
                }
            }
            else
            {
                HyperLogging.HttpInvalidHeaders(_logger, connection.Id, context.Errors, null);
            }

            HyperLogging.ConnectionClosing(_logger, connection.Id, null);
            await connection.StreamReader.CompleteAsync();
            await connection.StreamWriter.CompleteAsync();
            connection.Dispose();
            _openConnections.TryRemove(connection.Id, out _);

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
