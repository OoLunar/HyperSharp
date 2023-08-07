using System;
using System.Net;
using System.Threading;
using OoLunar.HyperSharp.Results;
using OoLunar.HyperSharp.Protocol;
using OoLunar.HyperSharp.Responders;

namespace OoLunar.HyperSharp.Tests.Responders
{
    public readonly record struct OkResponder : IResponder<HyperContext, HyperStatus>
    {
        public static Type[] Needs => Type.EmptyTypes;

        public Result<HyperStatus> Respond(HyperContext context, CancellationToken cancellationToken = default) => Result.Success(new HyperStatus(HttpStatusCode.OK));
    }
}
