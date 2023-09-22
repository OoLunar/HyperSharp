using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HyperSharp.Protocol;
using HyperSharp.Responders;
using HyperSharp.Results;

namespace HyperSharp.Benchmarks.Responders
{
    public readonly record struct OkTaskResponder : ITaskResponder<HyperContext, HyperStatus>
    {
        public static Type[] Needs => Type.EmptyTypes;

        public Task<Result<HyperStatus>> RespondAsync(HyperContext context, CancellationToken cancellationToken = default) => Task.FromResult(Result.Success(new HyperStatus(HttpStatusCode.OK)));
    }
}
