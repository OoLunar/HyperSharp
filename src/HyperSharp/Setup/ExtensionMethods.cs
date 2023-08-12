using System;
using Microsoft.Extensions.DependencyInjection;
using OoLunar.HyperSharp.Responders;

namespace OoLunar.HyperSharp.Setup
{
    public static class ExtensionMethods
    {
        public static IServiceCollection AddHyperSharp(this IServiceCollection services) => AddHyperSharp(services, new HyperConfigurationBuilder());
        public static IServiceCollection AddHyperSharp(this IServiceCollection services, Action<HyperConfigurationBuilder> configurate)
        {
            HyperConfigurationBuilder configurationBuilder = new();
            configurate(configurationBuilder);
            return AddHyperSharp(services, configurationBuilder);
        }

        public static IServiceCollection AddHyperSharp(this IServiceCollection services, Action<IServiceProvider, HyperConfigurationBuilder> configurate)
        {
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            HyperConfigurationBuilder configurationBuilder = new();
            configurate(serviceProvider, configurationBuilder);
            return AddHyperSharp(services, configurationBuilder);
        }

        public static IServiceCollection AddHyperSharp(this IServiceCollection services, HyperConfigurationBuilder builder)
        {
            services.AddSingleton<ResponderCompiler>();
            services.AddSingleton(new HyperConfiguration(services, builder));
            services.AddSingleton<HyperServer>();
            return services;
        }
    }
}
