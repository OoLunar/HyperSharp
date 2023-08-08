using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OoLunar.HyperSharp.Protocol;
using OoLunar.HyperSharp.Results;
using OoLunar.HyperSharp.Setup;

namespace OoLunar.HyperSharp
{
    public sealed class HyperServer
    {
        public HyperConfiguration Configuration { get; init; }
        private readonly ILogger<HyperServer> _logger;
        private readonly ConcurrentDictionary<Ulid, HyperConnection> _openConnections = new();
        private readonly ConcurrentStack<CancellationTokenSource> _cancellationTokenSources = new();
        private CancellationTokenSource? _mainCancellationTokenSource;

        public HyperServer(HyperConfiguration configuration, ILogger<HyperServer> logger)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

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
                while (!_openConnections.IsEmpty)
                {
                    HyperLogging.ConnectionsPending(_logger, _openConnections.Count, null);
                    foreach (HyperConnection connection in _openConnections.Values)
                    {
                        _openConnections.TryRemove(connection.Id, out _);

                        // Check if the connection has closed during iteration.
                        if (!connection.Client.Connected)
                        {
                            HyperLogging.ConnectionAlreadyClosed(_logger, connection.Id, null);
                            continue;
                        }

                        HyperLogging.ConnectionClosing(_logger, connection.Id, null);
                        connection.StreamReader.Complete();
                        connection.StreamWriter.Complete();
                        connection.Client.Dispose();
                        HyperLogging.ConnectionClosed(_logger, connection.Id, null);
                    }
                }

                listener.Stop();
                HyperLogging.ServerStopped(_logger, Configuration.ListeningEndpoint, null);
            });

            listener.Start();
            _ = ListenForConnectionsAsync(listener);
            HyperLogging.ServerStarted(_logger, Configuration.ListeningEndpoint, null);
        }

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

        private async Task ListenForConnectionsAsync(TcpListener listener)
        {
            while (!_mainCancellationTokenSource!.IsCancellationRequested)
            {
                // Throw the connection onto the async thread pool and wait for the next connection.
                _ = HandleConnectionAsync(await listener.AcceptTcpClientAsync());
            }
        }

        private async Task HandleConnectionAsync(TcpClient client)
        {
            HyperConnection connection = new(client, this);
            _openConnections.TryAdd(connection.Id, connection);
            HyperLogging.ConnectionOpened(_logger, connection.RemoteEndPoint, connection.Id, null);

            // Try to reuse an existing cancellation token source. If none are available, create a new one.
            CancellationTokenSource? cancellationTokenSource = null;
            while (!_cancellationTokenSources.IsEmpty)
            {
                if (_cancellationTokenSources.TryPop(out cancellationTokenSource) && cancellationTokenSource.TryReset())
                {
                    break;
                }
            }

            cancellationTokenSource ??= CancellationTokenSource.CreateLinkedTokenSource(_mainCancellationTokenSource!.Token);
            cancellationTokenSource.CancelAfter(Configuration.Timeout);

            // Start parsing the HTTP Headers.
            await using NetworkStream networkStream = client.GetStream();
            Result<HyperContext> context = await HyperHeaderParser.TryParseHeadersAsync(Configuration.MaxHeaderSize, connection, cancellationTokenSource.Token);
            if (!context.IsSuccess)
            {
                HyperLogging.HttpInvalidHeaders(_logger, connection.Id, context.Errors, null);
                return;
            }

            // Execute any registered responders.
            HyperLogging.HttpReceivedRequest(_logger, connection.Id, context.Value!.Route, null);
            Result<HyperStatus> status = await Configuration.Responders(context.Value, cancellationTokenSource.Token);
            if (!context.Value.HasResponded)
            {
                HyperLogging.HttpResponding(_logger, connection.Id, status.Value, null);
                if (status.IsSuccess)
                {
                    await context.Value.RespondAsync(status.HasValue ? status.Value : new HyperStatus(HttpStatusCode.NoContent), Configuration.JsonSerializerOptions);
                }
                else
                {
                    await context.Value.RespondAsync(new HyperStatus(HttpStatusCode.InternalServerError), Configuration.JsonSerializerOptions);
                }
            }

            HyperLogging.HttpResponded(_logger, connection.Id, status.Value, null);
            HyperLogging.ConnectionClosing(_logger, connection.Id, null);
            client.Dispose();
            _openConnections.TryRemove(connection.Id, out _);
            _cancellationTokenSources.Push(cancellationTokenSource);
            HyperLogging.ConnectionClosed(_logger, connection.Id, null);
        }
    }
}
