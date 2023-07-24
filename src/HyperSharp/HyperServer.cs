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
        private readonly HyperConfiguration Configuration;
        private readonly ILogger<HyperServer> Logger;
        private readonly ConcurrentDictionary<Ulid, HyperConnection> OpenConnections = new();
        private readonly ConcurrentStack<CancellationTokenSource> CancellationTokenSources = new();
        private CancellationTokenSource? MainCancellationTokenSource;

        public HyperServer(HyperConfiguration configuration, ILogger<HyperServer> logger)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Start(CancellationToken cancellationToken = default)
        {
            if (MainCancellationTokenSource is not null)
            {
                throw new InvalidOperationException("The server is already running.");
            }

            TcpListener listener = new(Configuration.ListeningEndpoint);
            MainCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            MainCancellationTokenSource.Token.Register(() =>
            {
                Logger.LogInformation("Shutting down server...");
                while (!OpenConnections.IsEmpty)
                {
                    Logger.LogInformation("Waiting for {ConnectionCount:N0} connections to close...", OpenConnections.Count);
                    foreach (HyperConnection connection in OpenConnections.Values)
                    {
                        OpenConnections.TryRemove(connection.Id, out _);

                        // Check if the connection has closed during iteration.
                        if (!connection.Client.Connected)
                        {
                            Logger.LogDebug("Connection {ConnectionId} is already closed.", connection.Id);
                            continue;
                        }

                        Logger.LogDebug("Waiting on connection {ConnectionId}...", connection.Id);
                        connection.StreamReader.Complete();
                        connection.StreamWriter.Complete();
                        Logger.LogDebug("Connection {ConnectionId} has closed.", connection.Id);
                    }
                }

                listener.Stop();
                Logger.LogInformation("Server shut down.");
            });

            listener.Start();
            _ = ListenForConnectionsAsync(listener);
            Logger.LogInformation("Listening on {Endpoint}", Configuration.ListeningEndpoint);
        }

        public async Task StopAsync()
        {
            if (MainCancellationTokenSource is null)
            {
                throw new InvalidOperationException("The server is not running.");
            }

            await MainCancellationTokenSource.CancelAsync();
            MainCancellationTokenSource.Dispose();
            MainCancellationTokenSource = null;
        }

        private async Task ListenForConnectionsAsync(TcpListener listener)
        {
            while (!MainCancellationTokenSource!.IsCancellationRequested)
            {
                // Throw the connection onto the async thread pool and wait for the next connection.
                _ = HandleConnectionAsync(await listener.AcceptTcpClientAsync());
            }
        }

        private async Task HandleConnectionAsync(TcpClient client)
        {
            HyperConnection connection = new(client);
            OpenConnections.TryAdd(connection.Id, connection);
            Logger.LogTrace("Received connection from {RemoteEndPoint} with Id {ConnectionId}", connection.RemoteEndPoint, connection.Id);

            // Try to reuse an existing cancellation token source. If none are available, create a new one.
            if (!CancellationTokenSources.TryPop(out CancellationTokenSource? cancellationTokenSource) || !cancellationTokenSource.TryReset())
            {
                cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(MainCancellationTokenSource!.Token);
            }
            cancellationTokenSource.CancelAfter(Configuration.Timeout);

            // Start parsing the HTTP Headers.
            await using NetworkStream networkStream = client.GetStream();
            connection.StreamReader = PipeReader.Create(networkStream, new StreamPipeReaderOptions(leaveOpen: true));
            connection.StreamWriter = PipeWriter.Create(networkStream, new StreamPipeWriterOptions(leaveOpen: true));
            Result<HyperContext> context = await HyperHeaderParser.TryParseHeadersAsync(Configuration.MaxHeaderSize, connection, cancellationTokenSource.Token);
            if (context.IsFailed)
            {
                Logger.LogWarning("Failed to parse headers from {ConnectionId} on '{Route}': {Error}", connection.Id, context.Value.Route, context.Errors);
                return;
            }

            // Execute any registered responders.
            Logger.LogTrace("Received request from {ConnectionId} for '{Route}'", connection.Id, context.Value.Route);
            Result<HyperStatus> status = await Configuration.Responders(context.Value);
            if (!context.Value.HasResponded)
            {
                Logger.LogDebug("Responding to {ConnectionId} with {Status}", connection.Id, status.Value);
                if (status.IsFailed)
                {
                    await context.Value.RespondAsync(new HyperStatus(HttpStatusCode.InternalServerError), Configuration.JsonSerializerOptions);
                }
                else
                {
                    await context.Value.RespondAsync(context.Value != default ? status.Value : new HyperStatus(HttpStatusCode.NoContent), Configuration.JsonSerializerOptions);
                }
            }

            Logger.LogTrace("Closing connection to {ConnectionId}", connection.Id);
            client.Dispose();
        }
    }
}
