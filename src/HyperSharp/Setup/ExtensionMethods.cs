using System;
using HyperSharp.Responders;
using Microsoft.Extensions.DependencyInjection;

namespace HyperSharp.Setup
{
    /// <summary>
    /// Provides extension methods for setting up HyperSharp.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Adds HyperSharp to the service collection.
        /// </summary>
        /// <param name="services">The service collection to add HyperSharp to.</param>
        /// <returns>The service collection with HyperSharp added.</returns>
        public static IServiceCollection AddHyperSharp(this IServiceCollection services) => AddHyperSharp(services, new HyperConfigurationBuilder());

        /// <summary>
        /// Adds HyperSharp to the service collection.
        /// </summary>
        /// <param name="services">The service collection to add HyperSharp to.</param>
        /// <param name="configurate">An Action which configures the HyperSharp configuration.</param>
        /// <returns>The service collection with HyperSharp added.</returns>
        public static IServiceCollection AddHyperSharp(this IServiceCollection services, Action<HyperConfigurationBuilder> configurate)
        {
            HyperConfigurationBuilder configurationBuilder = new();
            configurate(configurationBuilder);
            return AddHyperSharp(services, configurationBuilder);
        }

        /// <summary>
        /// Adds HyperSharp to the service collection.
        /// </summary>
        /// <param name="services">The service collection to add HyperSharp to.</param>
        /// <param name="configurate">An Action which configures the HyperSharp configuration.</param>
        /// <returns>The service collection with HyperSharp added.</returns>
        public static IServiceCollection AddHyperSharp(this IServiceCollection services, Action<IServiceProvider, HyperConfigurationBuilder> configurate)
        {
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            HyperConfigurationBuilder configurationBuilder = new();
            configurate(serviceProvider, configurationBuilder);
            return AddHyperSharp(services, configurationBuilder);
        }

        /// <summary>
        /// Adds HyperSharp to the service collection.
        /// </summary>
        /// <param name="services">The service collection to add HyperSharp to.</param>
        /// <param name="builder">The HyperSharp configuration builder.</param>
        /// <returns>The service collection with HyperSharp added.</returns>
        public static IServiceCollection AddHyperSharp(this IServiceCollection services, HyperConfigurationBuilder builder)
        {
            services.AddSingleton<ResponderCompiler>();
            services.AddSingleton(new HyperConfiguration(services, builder));
            services.AddSingleton<HyperServer>();
            return services;
        }
    }
}
