using System;
using System.Net;
using System.Threading;
using HyperSharp.Protocol;
using HyperSharp.Responders;
using HyperSharp.Results;

namespace HyperSharp.Benchmarks.Responders
{
    public readonly record struct OkResponder : IResponder<HyperContext, HyperStatus>
    {
        public static Type[] Needs => Type.EmptyTypes;

        public Result<HyperStatus> Respond(HyperContext context, CancellationToken cancellationToken = default) => Result.Success(new HyperStatus(HttpStatusCode.OK));
    }
}
