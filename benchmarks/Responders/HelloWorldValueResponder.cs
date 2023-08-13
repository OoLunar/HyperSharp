using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HyperSharp.Protocol;
using HyperSharp.Responders;
using HyperSharp.Results;

namespace HyperSharp.Benchmarks.Responders
{
    public readonly record struct HelloWorldValueTaskResponder : IValueTaskResponder<HyperContext, HyperStatus>
    {
        public static Type[] Needs => Type.EmptyTypes;

        public ValueTask<Result<HyperStatus>> RespondAsync(HyperContext context, CancellationToken cancellationToken = default) => ValueTask.FromResult(Result.Success(new HyperStatus(
            HttpStatusCode.OK,
            new Error("Hello World!")
        )));
    }
}
