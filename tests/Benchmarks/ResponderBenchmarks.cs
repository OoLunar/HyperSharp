using System;
using System.Net.Http;
using System.IO;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Net;
using OoLunar.HyperSharp.Protocol;
using OoLunar.HyperSharp.Responders;
using OoLunar.HyperSharp.Tests.Responders;

namespace OoLunar.HyperSharp.Tests.Benchmarks
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
            yield return compiler.Compile<HyperContext, HyperStatus>(Program.CreateServiceProvider());

            compiler = new();
            compiler.Search(new[] { typeof(HelloWorldResponder) });
            yield return compiler.Compile<HyperContext, HyperStatus>(Program.CreateServiceProvider());

            compiler = new();
            compiler.Search(new[] { typeof(OkResponder), typeof(HelloWorldResponder) });
            yield return compiler.Compile<HyperContext, HyperStatus>(Program.CreateServiceProvider());
        }
    }
}
