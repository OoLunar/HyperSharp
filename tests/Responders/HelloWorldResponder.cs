using System;
using System.Net;
using System.Threading.Tasks;
using FluentResults;
using OoLunar.HyperSharp.Responders;

namespace OoLunar.HyperSharp.Tests.Responders
{
    public sealed class HelloWorldResponder : IResponder<HyperContext, HyperStatus>
    {
        public string[] Implements { get; init; } = Array.Empty<string>();
        public ResponderPriority Priority { get; init; } = ResponderPriority.Medium;

        public HelloWorldResponder() { }

        public Task<Result<HyperStatus>> RespondAsync(HyperContext context) => Task.FromResult(Result.Ok(new HyperStatus(HttpStatusCode.OK, new()
        {
            ["X-Test"] = new[] { "Hello, World!" }
        }, "Hello world!")));
    }
}
