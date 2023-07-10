using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace OoLunar.HyperSharp
{
    public sealed record HyperConfiguration
    {
        public IPEndPoint ListeningEndpoint { get; init; }
        public int MaxHeaderSize { get; init; }
        public IReadOnlyList<IHeaderResponder> HeaderResponders { get; init; }

        internal HyperConfiguration(IServiceProvider serviceProvider, HyperConfigurationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));
            ILogger<HyperConfiguration> logger = serviceProvider.GetRequiredService<ILogger<HyperConfiguration>>();
            List<IHeaderResponder> headerResponders = new();
            foreach (Type type in builder.HeaderResponders)
            {
                if (typeof(IHeaderResponder).IsAssignableTo(type))
                {
                    headerResponders.Add(ActivatorUtilities.CreateInstance<IHeaderResponder>(serviceProvider, type));
                    continue;
                }

                logger.LogError("Type '{Type}' does not inherit from {HeaderResponder}.", type, typeof(IHeaderResponder));
            }

            if (headerResponders.Count != builder.HeaderResponders.Count)
            {
                throw new InvalidOperationException("Invalid header responders registered. See logs for more detail.");
            }

            ListeningEndpoint = builder.ListeningEndpoint;
            MaxHeaderSize = builder.MaxHeaderSize;
            HeaderResponders = headerResponders.AsReadOnly();
        }
    }
}
