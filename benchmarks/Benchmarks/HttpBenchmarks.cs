using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Engine;
using GenHTTP.Modules.IO.Providers;
using GenHTTP.Modules.IO.Strings;
using GenHTTP.Modules.Practices;
using Microsoft.Extensions.DependencyInjection;

namespace HyperSharp.Benchmarks.Cases
{
    [JsonExporterAttribute.Brief]
    public class HttpBenchmarks
    {
        private readonly HttpClient _client;

        private readonly HyperServer _hyperServer;
        private readonly HttpListener _httpListener;
        private readonly IServerHost _genHttpServer = Host.Create().Defaults().Handler(new ContentProviderBuilder().Resource(new StringResource("Hello World", "/", FlexibleContentType.Get(ContentType.TextPlain, "UTF-8"), DateTime.UtcNow))).Port(8082);
        // private readonly IAspnetServer _aspnetServer = new AspnetServer();

        public HttpBenchmarks()
        {
            ServiceProvider serviceProvider = Program.CreateServiceProvider();

            _client = serviceProvider.GetRequiredService<HttpClient>();
            _hyperServer = serviceProvider.GetRequiredService<HyperServer>();
            _httpListener = new();
            _httpListener.Prefixes.Clear();
            _httpListener.Prefixes.Add("http://localhost:8081/");
        }

        [GlobalSetup]
        public void Setup()
        {
            _hyperServer.Start();
            _httpListener.Start();
            _ = Task.Run(async () =>
            {
                byte[] helloWorld = "Hello World!"u8.ToArray();
                while (_httpListener.IsListening)
                {
                    HttpListenerContext context = await _httpListener.GetContextAsync();
                    context.Response.StatusCode = 200;
                    context.Response.Close(helloWorld, false);
                }
            });
            _genHttpServer.Start();
            // _aspnetServer.Start();
        }

        [WarmupCount(5), Benchmark(Baseline = true)]
        public Task HyperTestAsync() => _client.GetAsync("http://localhost:8080/");

        [WarmupCount(5), Benchmark]
        public Task HttpListenerTestAsync() => _client.GetAsync("http://localhost:8081/");

        [WarmupCount(5), Benchmark]
        public Task GenHttpTestAsync() => _client.GetAsync("http://localhost:8082/");

        [GlobalCleanup]
        public async Task CleanupAsync()
        {
            await _hyperServer.StopAsync();
            _httpListener.Stop();
            _genHttpServer.Stop();
            // _aspnetServer.Stop();
        }

        // [WarmupCount(5), Benchmark]
        // public Task AspnetTestAsync() => _client.GetAsync("http://localhost:8083/");
    }
}
