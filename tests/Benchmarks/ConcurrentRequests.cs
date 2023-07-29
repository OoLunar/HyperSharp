using System.Net.Http;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace OoLunar.HyperSharp.Tests.Benchmarks
{
    [JsonExporterAttribute.Brief]
    public class ConcurrentRequests
    {
        private readonly HttpClient _client;
        private readonly HyperServer _hyperServer;

        public ConcurrentRequests()
        {
            ServiceProvider serviceProvider = Program.CreateServiceProvider();

            _client = serviceProvider.GetRequiredService<HttpClient>();
            _hyperServer = serviceProvider.GetRequiredService<HyperServer>();
        }

        [GlobalSetup]
        public void Setup() => _hyperServer.Start();

        [GlobalCleanup]
        public Task CleanupAsync() => _hyperServer.StopAsync();

        [WarmupCount(25), Benchmark]
        public Task<HttpResponseMessage> ConcurrentRequestsTestAsync() => _client.GetAsync("/");
    }
}
