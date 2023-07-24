using System;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.Logging;
using OoLunar.HyperSharp.Parsing;

namespace OoLunar.HyperSharp
{
    public sealed class HyperServer
    {
        private readonly HyperConfiguration _configuration;
        private readonly ILogger<HyperServer> _logger;
        private readonly ConcurrentDictionary<Ulid, HyperConnection> _openConnections = new();
        private readonly ConcurrentStack<CancellationTokenSource> _cancellationTokenSources = new();
        private CancellationTokenSource? _mainCancellationTokenSource;

        public HyperServer(HyperConfiguration configuration, ILogger<HyperServer> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Start(CancellationToken cancellationToken = default)
        {
            if (_mainCancellationTokenSource is not null)
            {
                throw new InvalidOperationException("The server is already running.");
            }

            TcpListener listener = new(_configuration.ListeningEndpoint);
            _mainCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _mainCancellationTokenSource.Token.Register(() =>
            {
                _logger.LogInformation("Shutting down server...");
                while (!_openConnections.IsEmpty)
                {
                    _logger.LogInformation("Waiting for {ConnectionCount:N0} connections to close...", _openConnections.Count);
                    foreach (HyperConnection connection in _openConnections.Values)
                    {
                        _openConnections.TryRemove(connection.Id, out _);

                        // Check if the connection has closed during iteration.
                        if (!connection.Client.Connected)
                        {
                            _logger.LogDebug("Connection {ConnectionId} is already closed.", connection.Id);
                            continue;
                        }

                        _logger.LogDebug("Waiting on connection {ConnectionId}...", connection.Id);
                        connection.StreamReader.Complete();
                        connection.StreamWriter.Complete();
                        _logger.LogDebug("Connection {ConnectionId} has closed.", connection.Id);
                    }
                }

                listener.Stop();
                _logger.LogInformation("Server shut down.");
            });

            listener.Start();
            _ = ListenForConnectionsAsync(listener);
            _logger.LogInformation("Listening on {Endpoint}", _configuration.ListeningEndpoint);
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
            HyperConnection connection = new(client);
            _openConnections.TryAdd(connection.Id, connection);
            _logger.LogTrace("Received connection from {RemoteEndPoint} with Id {ConnectionId}", connection.RemoteEndPoint, connection.Id);

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
            cancellationTokenSource.CancelAfter(_configuration.Timeout);

            // Start parsing the HTTP Headers.
            await using NetworkStream networkStream = client.GetStream();
            connection.StreamReader = PipeReader.Create(networkStream, new StreamPipeReaderOptions(leaveOpen: true));
            connection.StreamWriter = PipeWriter.Create(networkStream, new StreamPipeWriterOptions(leaveOpen: true));
            Result<HyperContext> context = await HyperHeaderParser.TryParseHeadersAsync(_configuration.MaxHeaderSize, connection, cancellationTokenSource.Token);
            if (context.IsFailed)
            {
                _logger.LogWarning("Failed to parse headers from {ConnectionId} on '{Route}': {Error}", connection.Id, context.Value.Route, context.Errors);
                return;
            }

            // Execute any registered responders.
            _logger.LogTrace("Received request from {ConnectionId} for '{Route}'", connection.Id, context.Value.Route);
            Result<HyperStatus> status = await _configuration.Responders(context.Value, cancellationTokenSource.Token);
            if (!context.Value.HasResponded)
            {
                _logger.LogDebug("Responding to {ConnectionId} with {Status}", connection.Id, status.Value);
                if (status.IsFailed)
                {
                    await context.Value.RespondAsync(new HyperStatus(HttpStatusCode.InternalServerError), _configuration.JsonSerializerOptions);
                }
                else
                {
                    await context.Value.RespondAsync(context.Value != default ? status.Value : new HyperStatus(HttpStatusCode.NoContent), _configuration.JsonSerializerOptions);
                }
            }

            _logger.LogTrace("Closing connection to {ConnectionId}", connection.Id);
            client.Dispose();
            _openConnections.TryRemove(connection.Id, out _);
            _cancellationTokenSources.Push(cancellationTokenSource);
        }
    }
}
