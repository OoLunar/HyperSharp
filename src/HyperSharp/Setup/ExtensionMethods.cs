using System;
using Microsoft.Extensions.DependencyInjection;
using OoLunar.HyperSharp.Responders;

namespace OoLunar.HyperSharp.Setup
{
    public static class ExtensionMethods
    {
        public static IServiceCollection AddHyperSharp(this IServiceCollection services, Action<IServiceProvider, HyperConfigurationBuilder> configurate)
        {
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            HyperConfigurationBuilder configurationBuilder = new();
            configurate(serviceProvider, configurationBuilder);
            services.AddSingleton<ResponderCompiler>();
            services.AddSingleton(new HyperConfiguration(services, configurationBuilder));
            services.AddSingleton<HyperServer>();
            return services;
        }

        public static IServiceCollection AddHyperSharp(this IServiceCollection services, Action<HyperConfigurationBuilder> configurate)
        {
            HyperConfigurationBuilder configurationBuilder = new();
            configurate(configurationBuilder);
            services.AddSingleton<ResponderCompiler>();
            services.AddSingleton(new HyperConfiguration(services, configurationBuilder));
            services.AddSingleton<HyperServer>();
            return services;
        }
    }
}
