using System.Net.Http;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace OoLunar.HyperSharp.Tests.Benchmarks
{
    [JsonExporterAttribute.Brief]
    public class HttpBenchmarks
    {
        private readonly HttpClient _client;
        private readonly HyperServer _hyperServer;

        public HttpBenchmarks()
        {
            ServiceProvider serviceProvider = Program.CreateServiceProvider();

            _client = serviceProvider.GetRequiredService<HttpClient>();
            _hyperServer = serviceProvider.GetRequiredService<HyperServer>();
        }

        [GlobalSetup]
        public void Setup() => _hyperServer.Start();

        [GlobalCleanup]
        public Task CleanupAsync() => _hyperServer.StopAsync();

        [WarmupCount(5), Benchmark]
        public Task<HttpResponseMessage> ConcurrentRequestsTestAsync() => _client.GetAsync("/");


    }
}
