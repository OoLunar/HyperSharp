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
        private readonly ConcurrentStack<Task> _openConnections = new();

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
            _tcpListener = new(Configuration.ListeningEndpoint);
            _mainCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _mainCancellationTokenSource.Token.Register(() =>
            {
                HyperLogging.ServerStopping(_logger, Configuration.ListeningEndpoint, null);
                if (!_openConnections.IsEmpty)
                {
                    HyperLogging.ConnectionsPending(_logger, _openConnections.Count, null);
                }
            });

            _tcpListener.Start();
            _ = ListenForConnectionsAsync(_tcpListener);
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

            _mainCancellationTokenSource.Cancel();
            _mainCancellationTokenSource.Dispose();
            _mainCancellationTokenSource = null;
            while (!_openConnections.IsEmpty)
            {
                if (_openConnections.TryPop(out Task? task))
                {
                    await task;
                }
            }

            _tcpListener?.Stop();
            HyperLogging.ServerStopped(_logger, Configuration.ListeningEndpoint, null);
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
                _openConnections.Push(HandleConnectionAsync(await listener.AcceptTcpClientAsync(_mainCancellationTokenSource.Token)));
            }
        }

        /// <summary>
        /// Handles an incoming connection asynchronously, parsing the HTTP headers and executing any registered responders.
        /// </summary>
        /// <param name="client">The connected TCP client.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task HandleConnectionAsync(TcpClient client)
        {
            HyperConnection connection = new(client.GetStream(), this);
            HyperLogging.ConnectionOpened(_logger, connection.Id, null);

            // Try to reuse an existing cancellation token source. If none are available, create a new one.
            if (!_cancellationTokenSources.TryPop(out CancellationTokenSource? cancellationTokenSource))
            {
                cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_mainCancellationTokenSource!.Token);
            }
            cancellationTokenSource.CancelAfter(Configuration.Timeout);

            // Start parsing the HTTP Headers.
            Result<HyperContext> context = await HyperHeaderParser.TryParseHeadersAsync(Configuration.MaxHeaderSize, connection, cancellationTokenSource.Token);
            if (!cancellationTokenSource.IsCancellationRequested)
            {
                if (context.IsSuccess)
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

                        await context.Value.RespondAsync(response, Configuration.JsonSerializerOptions, cancellationTokenSource.Token);
                        HyperLogging.HttpResponded(_logger, connection.Id, response, null);
                    }
                }
                else
                {
                    HyperLogging.HttpInvalidHeaders(_logger, connection.Id, context.Errors, null);
                }
            }
            else
            {
                await context.Value!.RespondAsync(HyperStatus.InternalServerError(), Configuration.JsonSerializerOptions);
            }

            HyperLogging.ConnectionClosing(_logger, connection.Id, null);
            await connection.DisposeAsync();

            _openConnections.TryPop(out Task? _);
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
