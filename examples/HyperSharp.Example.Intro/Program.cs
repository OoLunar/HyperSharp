using System;
using System.Threading.Tasks;
using HyperSharp.Setup;
using Microsoft.Extensions.DependencyInjection;

namespace HyperSharp.Examples.Intro
{
    public static class Program
    {
        public static async Task Main()
        {
            // Create a new service collection and add HyperSharp to it.
            IServiceCollection services = new ServiceCollection();
            services.AddHyperSharp();

            // Build the service provider and get the HyperServer instance.
            IServiceProvider provider = services.BuildServiceProvider();
            HyperServer server = provider.GetRequiredService<HyperServer>();

            // Start the server and wait for new connections.
            server.Start();
            await Task.Delay(-1);
        }
    }
}
