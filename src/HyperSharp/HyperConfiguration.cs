using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OoLunar.HyperSharp.Json;
using OoLunar.HyperSharp.Responders;

namespace OoLunar.HyperSharp
{
    public sealed record HyperConfiguration
    {
        public Func<HyperContext, Task<Result<HyperStatus>>> Responders { get; init; }
        public IPEndPoint ListeningEndpoint { get; init; }
        public JsonSerializerOptions JsonSerializerOptions { get; init; }
        public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
        public int MaxHeaderSize { get; init; }

        internal HyperConfiguration(IServiceCollection serviceDescriptors, HyperConfigurationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(serviceDescriptors, nameof(serviceDescriptors));
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));
            IServiceProvider serviceProvider = serviceDescriptors.BuildServiceProvider();

            ListeningEndpoint = builder.ListeningEndpoint;
            MaxHeaderSize = builder.MaxHeaderSize;
            JsonSerializerOptions = serviceProvider.GetService<IOptionsSnapshot<JsonSerializerOptions>>()?.Get(builder.JsonSerializerOptionsName) ?? HyperSerializationOptions.Default;

            ResponderSearcher<HyperContext, HyperStatus> responderLocator = new();
            responderLocator.RegisterResponders(builder.Responders);
            Responders = responderLocator.CompileTreeDelegate(serviceProvider);
        }
    }
}
