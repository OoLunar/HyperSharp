using System.Net.Http;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using OoLunar.HyperSharp.Protocol;

namespace OoLunar.HyperSharp.Tests.Benchmarks
{
    [JsonExporterAttribute.Brief]
    public class HttpBenchmarks
    {
        private static readonly byte[] _headers = "GET / HTTP/1.1\r\nHost: localhost:8080\r\nUser-Agent: Mozilla/5.0 (X11; Linux x86_64; rv:109.0) Gecko/20100101 Firefox/116.0\r\nAccept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8\r\nAccept-Language: en-US,en;q=0.5\r\nAccept-Encoding: gzip, deflate, br\r\nDNT: 1\r\nAlt-Used: localhost:8080\r\nConnection: keep-alive\r\nUpgrade-Insecure-Requests: 1\r\nSec-Fetch-Dest: document\r\nSec-Fetch-Mode: navigate\r\nSec-Fetch-Site: cross-site\r\nPragma: no-cache\r\nCache-Control: no-cache\r\nTE: trailers\r\n\r\n"u8.ToArray();
        private readonly HttpClient _client;
        private readonly HyperServer _hyperServer;
        private HyperConnection _connection = null!;

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
        public async Task HttpClientTestAsync() => await _client.GetAsync("http://localhost:8080/");

        [IterationSetup(Target = nameof(ParseHeadersTestAsync))]
        public void HeaderIterationSetup()
        {
            MemoryStream stream = new();
            stream.Write(_headers);
            _connection = new(stream, _hyperServer);
        }

        [Benchmark]
        public async ValueTask ParseHeadersTestAsync() => await HyperHeaderParser.TryParseHeadersAsync(_hyperServer.Configuration.MaxHeaderSize, _connection, default);
    }
}
