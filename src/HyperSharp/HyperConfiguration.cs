using System;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HyperSharp.Protocol;
using HyperSharp.Responders;
using HyperSharp.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HyperSharp
{
    public sealed record HyperConfiguration
    {
        public string ServerName { get; init; }
        public int MaxHeaderSize { get; init; }
        public Uri Host { get; init; }
        public TimeSpan Timeout { get; init; }
        public IPEndPoint ListeningEndpoint { get; init; }
        public JsonSerializerOptions JsonSerializerOptions { get; init; }
        public ValueTaskResponderDelegate<HyperContext, HyperStatus> RespondersDelegate { get; init; }

        public HyperConfiguration() : this(new ServiceCollection().AddHyperSharp(), new HyperConfigurationBuilder()) { }
        public HyperConfiguration(HyperConfigurationBuilder builder) : this(new ServiceCollection().AddHyperSharp(), builder) { }
        public HyperConfiguration(IServiceCollection serviceDescriptors) : this(serviceDescriptors, new HyperConfigurationBuilder()) { }
        public HyperConfiguration(IServiceCollection serviceDescriptors, HyperConfigurationBuilder builder)
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
                RespondersDelegate = responderCompiler.CompileAsyncResponders<HyperContext, HyperStatus>(serviceProvider);
                return;
            }

            ResponderDelegate<HyperContext, HyperStatus> synchronousResponders = responderCompiler.CompileResponders<HyperContext, HyperStatus>(serviceProvider);
            RespondersDelegate = (HyperContext context, CancellationToken cancellationToken) => ValueTask.FromResult(synchronousResponders(context, cancellationToken));
        }
    }
}
