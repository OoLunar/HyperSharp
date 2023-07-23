using System;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using OoLunar.HyperSharp.Json;

namespace OoLunar.HyperSharp
{
    public static class ExtensionMethods
    {
        public static IServiceCollection AddHyperSharp(this IServiceCollection services, Action<IServiceProvider, HyperConfigurationBuilder> configurate)
        {
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            HyperConfigurationBuilder configurationBuilder = new();
            configurate(serviceProvider, configurationBuilder);
            services.AddSingleton(new HyperConfiguration(services, configurationBuilder));
            services.AddSingleton<HyperServer>();
            return services;
        }

        public static IServiceCollection AddHyperSharp(this IServiceCollection services, Action<HyperConfigurationBuilder> configurate)
        {
            HyperConfigurationBuilder configurationBuilder = new();
            configurate(configurationBuilder);
            services.AddSingleton(new HyperConfiguration(services, configurationBuilder));
            services.AddSingleton<HyperServer>();
            return services;
        }

        public static IServiceCollection ConfigureHyperJsonConverters(this IServiceCollection serviceCollection, string? optionsName = "HyperSharp") => serviceCollection.Configure<JsonSerializerOptions>(optionsName, options => options.Converters.Add(new IErrorJsonConverterFactory()));
    }
}
