using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using OoLunar.HyperSharp.Protocol;

namespace OoLunar.HyperSharp.Tests.Benchmarks
{
    [JsonExporterAttribute.Brief]
    public class HttpBenchmarks
    {
        private readonly HttpClient _client;
        private readonly HyperServer _hyperServer;
        private HyperConnection _connection;

        public HttpBenchmarks()
        {
            ServiceProvider serviceProvider = Program.CreateServiceProvider();

            _client = serviceProvider.GetRequiredService<HttpClient>();
            _hyperServer = serviceProvider.GetRequiredService<HyperServer>();
            IterationSetup();
        }

        [GlobalSetup]
        public void Setup() => _hyperServer.Start();

        [GlobalCleanup]
        public Task CleanupAsync() => _hyperServer.StopAsync();

        [IterationSetup(Target = nameof(ParseHeadersTestAsync)), MemberNotNull(nameof(_connection))]
        public void IterationSetup()
        {
            MemoryStream stream = new();
            stream.Write(@"""
GET / HTTP/1.1
Host: localhost:8080
User-Agent: Mozilla/5.0 (X11; Linux x86_64; rv:109.0) Gecko/20100101 Firefox/116.0
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8
Accept-Language: en-US,en;q=0.5
Accept-Encoding: gzip, deflate, br
DNT: 1
Alt-Used: localhost:8080
Connection: keep-alive
Upgrade-Insecure-Requests: 1
Sec-Fetch-Dest: document
Sec-Fetch-Mode: navigate
Sec-Fetch-Site: cross-site
Pragma: no-cache
Cache-Control: no-cache
TE: trailers
"""u8);
            _connection = new(stream, _hyperServer);
        }


        [WarmupCount(5), Benchmark]
        public Task<HttpResponseMessage> ConcurrentRequestsTestAsync() => _client.GetAsync("/");

        [Benchmark]
        public async ValueTask ParseHeadersTestAsync() => await HyperHeaderParser.TryParseHeadersAsync(_hyperServer.Configuration.MaxHeaderSize, _connection, default);
    }
}
