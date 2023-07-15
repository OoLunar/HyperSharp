using System;
using System.Net;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;

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
            ResponderSearcher responderLocator = new();
            responderLocator.RegisterResponders(builder.Responders);
            Responders = responderLocator.CompileTreeDelegate(serviceDescriptors.BuildServiceProvider());
        }
    }
}