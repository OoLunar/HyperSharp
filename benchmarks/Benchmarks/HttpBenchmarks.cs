using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using EmbedIO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Engine;
using GenHTTP.Modules.IO.Providers;
using GenHTTP.Modules.IO.Strings;
using GenHTTP.Modules.Practices;
using Microsoft.Extensions.DependencyInjection;
using Swan.Logging;

namespace HyperSharp.Benchmarks.Cases
{
    [JsonExporterAttribute.Brief]
    public class HttpBenchmarks
    {
        private readonly HttpClient _client;

        private readonly HyperServer _hyperServer;
        private readonly HttpListener _httpListener;
        private readonly IServerHost _genHttpServer;
        private readonly WebServer _embedIoServer;
        private readonly HttpCoreServer _httpCoreServer;
        // private readonly IAspnetServer _aspnetServer = new AspnetServer();

        public HttpBenchmarks()
        {
            ServiceProvider serviceProvider = Program.CreateServiceProvider();

            _client = serviceProvider.GetRequiredService<HttpClient>();
            _hyperServer = serviceProvider.GetRequiredService<HyperServer>();

            _httpListener = new();
            _httpListener.Prefixes.Clear();
            _httpListener.Prefixes.Add("http://localhost:8081/");

            _genHttpServer = Host.Create()
                .Defaults()
                .Handler(new ContentProviderBuilder().Resource(new StringResource("Hello World!", "/", FlexibleContentType.Get(ContentType.TextPlain, "UTF-8"), DateTime.UtcNow)))
                .Port(8082);

            Logger.UnregisterLogger<ConsoleLogger>();
            _embedIoServer = new WebServer(config => config
                    .WithUrlPrefix("http://localhost:8083/")
                    .WithMode(HttpListenerMode.EmbedIO))
                .OnAny("/", handler => handler.SendStringAsync("Hello World!", "text/plain", Encoding.UTF8));

            _httpCoreServer = new HttpCoreServer("127.0.0.1", 8084);
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
            _embedIoServer.Start();
            _httpCoreServer.Start();
            // _aspnetServer.Start();
        }

        [WarmupCount(5), Benchmark(Baseline = true)]
        public Task HyperTestAsync() => _client.GetAsync("http://localhost:8080/");

        [WarmupCount(5), Benchmark]
        public Task HttpListenerTestAsync() => _client.GetAsync("http://localhost:8081/");

        [WarmupCount(5), Benchmark]
        public Task GenHttpTestAsync() => _client.GetAsync("http://localhost:8082/");

        [WarmupCount(5), Benchmark]
        public Task EmbedIoTestAsync() => _client.GetAsync("http://localhost:8083/");

        [WarmupCount(5), Benchmark]
        public Task HttpCoreTestAsync() => _client.GetAsync("http://localhost:8084/");

        [GlobalCleanup]
        public async Task CleanupAsync()
        {
            await _hyperServer.StopAsync();
            _httpListener.Stop();
            _genHttpServer.Stop();
            _embedIoServer.Dispose();
            _httpCoreServer.Stop();
            // _aspnetServer.Stop();
        }

        // [WarmupCount(5), Benchmark]
        // public Task AspnetTestAsync() => _client.GetAsync("http://localhost:8083/");

        private class HttpCoreSession : NetCoreServer.HttpSession
        {
            public HttpCoreSession(NetCoreServer.HttpServer server) : base(server) { }
            protected override void OnReceivedRequest(NetCoreServer.HttpRequest request)
            {
                NetCoreServer.HttpResponse response = Response.MakeOkResponse();
                response.SetBody("Hello World!");
                SendResponse(response);
            }
        }

        private class HttpCoreServer : NetCoreServer.HttpServer
        {
            public HttpCoreServer(string address, int port) : base(address, port) { }

            protected override NetCoreServer.TcpSession CreateSession() => new HttpCoreSession(this);
            protected override void OnError(SocketError error) => Console.WriteLine($"Server caught an error with code {error}");
        }
    }
}
