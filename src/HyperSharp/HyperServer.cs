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

        public HyperServer(HyperConfiguration configuration, ILogger<HyperServer> logger, IOptionsSnapshot<JsonSerializerOptions>? jsonSerializerOptions = null)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            JsonSerializerOptions = jsonSerializerOptions?.Get("HyperSharp") ?? HyperSerializationOptions.Default;
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
                using NetworkStream clientStream = client.GetStream();
                Result<HyperContext> context = await HyperHeaderParser.TryParseHeadersAsync(Configuration.MaxHeaderSize, clientStream);
                if (context.IsFailed)
                {
                    Logger.LogError("Failed to parse headers: {Errors}", context.Errors);
                    continue;
                }

                Logger.LogTrace("Received request: {Request}", context.Value);
                Result<HyperStatus> status = await Configuration.Responders(context.Value);
                if (status.IsFailed)
                {
                    Logger.LogWarning("Failed to respond: {Errors}", status.Errors);
                    if (!context.Value.HasResponded)
                    {
                        await context.Value.RespondAsync(new HyperStatus(HttpStatusCode.NotFound), JsonSerializerOptions);
                    }
                }
                else if (context.IsSuccess && !context.Value.HasResponded)
                {
                    await context.Value.RespondAsync(context.Value != default ? status.Value : new HyperStatus(HttpStatusCode.OK), JsonSerializerOptions);
                }
            }
        }
    }
}
