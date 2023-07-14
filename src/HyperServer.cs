using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.Logging;
using OoLunar.HyperSharp.Parsing;

namespace OoLunar.HyperSharp
{
    public delegate void HyperServerStartedEventArgs(HyperServer server);

    public sealed class HyperServer
    {
        private readonly HyperConfiguration Configuration;
        private readonly ILogger<HyperServer> Logger;

        public HyperServer(HyperConfiguration configuration, ILogger<HyperServer> logger)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Run(CancellationToken cancellationToken = default)
        {
            _ = ReceiveLoopAsync(cancellationToken);
            Logger.LogInformation("HyperServer started.");
        }

        public async ValueTask StopAsync()
        {
            Logger.LogInformation("Stopping HyperServer...");
            await Task.Delay(1000);
            Logger.LogInformation("HyperServer stopped.");
        }

        private async Task ReceiveLoopAsync(CancellationToken cancellationToken = default)
        {
            TcpListener listener = new(Configuration.ListeningEndpoint);
            listener.Start();
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient client = await listener.AcceptTcpClientAsync(cancellationToken);
                NetworkStream clientStream = client.GetStream();
                Result<HyperContext> context = await HyperHeaderParser.TryParseHeadersAsync(Configuration.MaxHeaderSize, clientStream);
                if (context.IsFailed)
                {
                    Logger.LogError("Failed to parse headers: {Errors}", context.Errors);
                    continue;
                }

                Logger.LogTrace("Received request: {Method} {Route} {Version}", context.Value.Method, context.Value.Route, context.Value.Version);
                Result<HyperStatus> status = await Configuration.Responders(context.Value);
                if (status.IsFailed)
                {
                    Logger.LogDebug("Failed to respond: {Errors}", status.Errors);
                    await context.Value.RespondAsync(status.ValueOrDefault == default ? new HyperStatus(HttpStatusCode.NotFound) : status.Value);
                }
                else
                {
                    await context.Value.RespondAsync(status.Value == default ? new HyperStatus(HttpStatusCode.OK) : status.Value);
                }
            }
        }
    }
}
