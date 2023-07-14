using System;
using System.Net;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using OoLunar.HyperSharp.Responders;

namespace OoLunar.HyperSharp
{
    public sealed record HyperConfiguration
    {
        public IPEndPoint ListeningEndpoint { get; init; }
        public int MaxHeaderSize { get; init; }
        public Func<HyperContext, Task<Result<HyperStatus>>> Responders { get; init; }

        internal HyperConfiguration(IServiceCollection serviceDescriptors, HyperConfigurationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(serviceDescriptors, nameof(serviceDescriptors));
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));

            ListeningEndpoint = builder.ListeningEndpoint;
            MaxHeaderSize = builder.MaxHeaderSize;
            ResponderLocator responderLocator = new(serviceDescriptors);
            responderLocator.LocateResponders(builder.Responders);
            Responders = responderLocator.CompileResponderPath();
        }
    }
}
