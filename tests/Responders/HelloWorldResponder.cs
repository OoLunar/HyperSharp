using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using OoLunar.HyperSharp.Responders;
using OoLunar.HyperSharp.Results;

namespace OoLunar.HyperSharp.Tests.Benchmarks.Responders
{
    public sealed class HelloWorldResponder : IResponder<HyperContext, HyperStatus>
    {
        public string[] Implements { get; init; } = Array.Empty<string>();
        public HelloWorldResponder() { }

        public Task<Result<HyperStatus>> RespondAsync(HyperContext context, CancellationToken cancellationToken = default) => Task.FromResult(Result.Success(new HyperStatus(
            HttpStatusCode.OK,
            new Error("Hello World!")
        )));
    }
}
