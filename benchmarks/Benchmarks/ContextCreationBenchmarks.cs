using System;
using System.IO;
using System.Net;
using System.Net.Http;
using BenchmarkDotNet.Attributes;
using HyperSharp.Protocol;
using Microsoft.Extensions.DependencyInjection;

namespace HyperSharp.Benchmarks.Cases
{
    [JsonExporterAttribute.Brief]
    public class ContextCreationBenchmarks
    {
        private readonly Uri _route;
        private readonly HyperHeaderCollection _headers;
        private readonly HyperConnection _connection;

        public ContextCreationBenchmarks()
        {
            _route = new Uri("http://localhost/");
            _headers = new HyperHeaderCollection();
            _connection = new(new MemoryStream(), Program.CreateServiceProvider().GetRequiredService<HyperServer>());
        }

        [WarmupCount(5), Benchmark]
        public HyperContext CreateContext() => new(HttpMethod.Get, _route, HttpVersion.Version11, _headers, _connection);
    }
}
