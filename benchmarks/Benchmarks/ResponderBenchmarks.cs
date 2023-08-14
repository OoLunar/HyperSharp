using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using BenchmarkDotNet.Attributes;
using HyperSharp.Benchmarks.Responders;
using HyperSharp.Protocol;
using HyperSharp.Responders;
using Microsoft.Extensions.DependencyInjection;

namespace HyperSharp.Benchmarks.Cases
{
    [JsonExporterAttribute.Brief]
    public class ResponderBenchmarks
    {
        private readonly HyperContext _context;

        public ResponderBenchmarks()
        {
            ServiceProvider serviceProvider = Program.CreateServiceProvider();
            HyperConnection connection = new(new MemoryStream(), serviceProvider.GetRequiredService<HyperServer>());
            _context = new HyperContext(HttpMethod.Get, new Uri("http://localhost/"), HttpVersion.Version11, new(), connection);
        }

        [Benchmark]
        [ArgumentsSource(nameof(Data))]
        public void DelegateExecutionTime(ResponderDelegate<HyperContext, HyperStatus> responder) => responder(_context, default);

        public static IEnumerable<ResponderDelegate<HyperContext, HyperStatus>> Data()
        {
            yield return new OkResponder().Respond;
            yield return new HelloWorldResponder().Respond;

            ResponderCompiler compiler = new();
            compiler.Search(new[] { typeof(OkResponder) });
            yield return compiler.CompileResponders<HyperContext, HyperStatus>(Program.CreateServiceProvider());

            compiler = new();
            compiler.Search(new[] { typeof(HelloWorldResponder) });
            yield return compiler.CompileResponders<HyperContext, HyperStatus>(Program.CreateServiceProvider());

            compiler = new();
            compiler.Search(new[] { typeof(OkResponder), typeof(HelloWorldResponder) });
            yield return compiler.CompileResponders<HyperContext, HyperStatus>(Program.CreateServiceProvider());
        }
    }
}
