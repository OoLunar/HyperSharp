using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using OoLunar.HyperSharp.Results;
using OoLunar.HyperSharp.Protocol;
using OoLunar.HyperSharp.Responders;

namespace OoLunar.HyperSharp.Tests.Responders
{
    public readonly record struct OkTaskResponder : ITaskResponder<HyperContext, HyperStatus>
    {
        public static Type[] Needs => Type.EmptyTypes;

        public Task<Result<HyperStatus>> RespondAsync(HyperContext context, CancellationToken cancellationToken = default) => Task.FromResult(Result.Success(new HyperStatus(HttpStatusCode.OK)));
    }
}
