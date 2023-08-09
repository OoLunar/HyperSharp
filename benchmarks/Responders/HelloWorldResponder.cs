using System;
using System.Net;
using System.Threading;
using OoLunar.HyperSharp.Protocol;
using OoLunar.HyperSharp.Responders;
using OoLunar.HyperSharp.Results;

namespace OoLunar.HyperSharp.Benchmarks.Responders
{
    public readonly record struct HelloWorldResponder : IResponder<HyperContext, HyperStatus>
    {
        public static Type[] Needs => Type.EmptyTypes;

        public Result<HyperStatus> Respond(HyperContext context, CancellationToken cancellationToken = default) => Result.Success(new HyperStatus(
            HttpStatusCode.OK,
            new Error("Hello World!")
        ));
    }
}
