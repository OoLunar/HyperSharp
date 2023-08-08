using System;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OoLunar.HyperSharp.Protocol;
using OoLunar.HyperSharp.Responders;
using OoLunar.HyperSharp.Setup;

namespace OoLunar.HyperSharp
{
    public sealed record HyperConfiguration
    {
        public ValueTaskResponderDelegate<HyperContext, HyperStatus> Responders { get; init; }
        public IPEndPoint ListeningEndpoint { get; init; }
        public JsonSerializerOptions JsonSerializerOptions { get; init; }
        public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
        public int MaxHeaderSize { get; init; }
        public Uri Host { get; init; }
        public string ServerName { get; init; }

        internal HyperConfiguration(IServiceCollection serviceDescriptors, HyperConfigurationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(serviceDescriptors, nameof(serviceDescriptors));
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));
            IServiceProvider serviceProvider = serviceDescriptors.BuildServiceProvider();

            if (!Uri.TryCreate($"http://{builder.ListeningEndpoint}/", UriKind.Absolute, out Uri? host))
            {
                throw new ArgumentException("The listening endpoint is invalid.", nameof(builder));
            }

            Host = host;
            ListeningEndpoint = builder.ListeningEndpoint;
            MaxHeaderSize = builder.MaxHeaderSize;
            JsonSerializerOptions = serviceProvider.GetService<IOptionsSnapshot<JsonSerializerOptions>>()?.Get(builder.JsonSerializerOptionsName) ?? HyperJsonSerializationOptions.Default;
            ServerName = builder.ServerName;

            ResponderCompiler responderCompiler = serviceProvider.GetRequiredService<ResponderCompiler>();
            responderCompiler.Search(builder.Responders);

            if (!responderCompiler.IsSynchronous())
            {
                Responders = responderCompiler.CompileAsyncResponders<HyperContext, HyperStatus>(serviceProvider);
                return;
            }

            ResponderDelegate<HyperContext, HyperStatus> synchronousResponders = responderCompiler.CompileResponders<HyperContext, HyperStatus>(serviceProvider);
            Responders = (HyperContext context, CancellationToken cancellationToken) => ValueTask.FromResult(synchronousResponders(context, cancellationToken));
        }
    }
}
