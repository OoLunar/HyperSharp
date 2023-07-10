using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Net;

namespace OoLunar.HyperSharp
{
    public delegate void HyperServerStartedEventArgs(HyperServer server);

    public sealed class HyperServer
    {
        public event HyperServerStartedEventArgs Started;
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
            //Started(this);
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

                // Invoke all header responders in parallel
                // It is the responder's job to respond and close the stream if necessary
                await Task.WhenAll(Configuration.HeaderResponders.Select(responder => responder.RespondAsync(context.Value)));
                if (context.Value.HasResponded)
                {
                    continue;
                }

                // TODO: Route the request
                await context.Value.RespondAsync(new HyperStatus(HttpStatusCode.NotFound));
            }
        }
    }
}
