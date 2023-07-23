using System;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OoLunar.HyperSharp.Json;
using OoLunar.HyperSharp.Parsing;

namespace OoLunar.HyperSharp
{
    public delegate void HyperServerStartedEventArgs(HyperServer server);

    public sealed class HyperServer
    {
        private readonly HyperConfiguration Configuration;
        private readonly ILogger<HyperServer> Logger;
        private readonly JsonSerializerOptions JsonSerializerOptions;
        private readonly TcpListener Listener;
        private CancellationTokenSource? CancellationTokenSource;

        public HyperServer(HyperConfiguration configuration, ILogger<HyperServer> logger, IOptionsSnapshot<JsonSerializerOptions>? jsonSerializerOptions = null)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            JsonSerializerOptions = jsonSerializerOptions?.Get("HyperSharp") ?? HyperSerializationOptions.Default;
            Listener = new(Configuration.ListeningEndpoint);
        }

        public void Run(CancellationToken cancellationToken = default)
        {
            if (CancellationTokenSource is not null)
            {
                throw new InvalidOperationException("HyperServer is already running.");
            }

            Logger.LogDebug("Starting HyperServer...");
            CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            Listener.Start();
            CancellationTokenSource.Token.Register(Listener.Stop);
            _ = ReceiveLoopAsync();
            Logger.LogInformation("HyperServer started.");
        }

        public async ValueTask StopAsync()
        {
            if (CancellationTokenSource is null)
            {
                throw new InvalidOperationException("HyperServer is not running.");
            }

            Logger.LogDebug("Stopping HyperServer...");
            await CancellationTokenSource!.CancelAsync();
            Logger.LogInformation("HyperServer stopped.");
        }

        private async Task ReceiveLoopAsync()
        {
            while (!CancellationTokenSource!.Token.IsCancellationRequested)
            {
                _ = HandleStreamAsync(await Listener.AcceptTcpClientAsync(CancellationTokenSource.Token));
            }
        }

        private async Task HandleStreamAsync(TcpClient client)
        {
            Logger.LogTrace("Received connection from {IP}", client.Client.RemoteEndPoint?.ToString() ?? "<Unknown Ip Address>");
            NetworkStream networkStream = client.GetStream();
            Result<HyperContext> context = await HyperHeaderParser.TryParseHeadersAsync(Configuration.MaxHeaderSize, networkStream);
            if (context.IsFailed)
            {
                Logger.LogWarning("Failed to parse headers from {IP} on {Route}: {Error}", networkStream.Socket.RemoteEndPoint?.ToString() ?? "<Unknown Ip Address>", context.Value.Route, context.Errors);
                await networkStream.DisposeAsync();
                return;
            }

            Logger.LogTrace("Received request from {IP} for {Path}", networkStream.Socket.RemoteEndPoint?.ToString() ?? "<Unknown Ip Address>", context.Value.Route);
            Result<HyperStatus> status = await Configuration.Responders(context.Value);
            if (!context.Value.HasResponded)
            {
                Logger.LogDebug("Responding to {IP} with {Status}", networkStream.Socket.RemoteEndPoint?.ToString() ?? "<Unknown Ip Address>", status.Value);
                if (status.IsFailed)
                {
                    await context.Value.RespondAsync(new HyperStatus(HttpStatusCode.NotFound), JsonSerializerOptions);
                }
                else
                {
                    await context.Value.RespondAsync(context.Value != default ? status.Value : new HyperStatus(HttpStatusCode.NoContent), JsonSerializerOptions);
                }
            }

            Logger.LogTrace("Closing connection to {IP}", networkStream.Socket.RemoteEndPoint?.ToString() ?? "<Unknown Ip Address>");
            await networkStream.DisposeAsync();
            client.Dispose();
        }
    }
}
