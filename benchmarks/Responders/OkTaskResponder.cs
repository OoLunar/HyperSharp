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

        public async Task<Result<HyperStatus>> RespondAsync(HyperContext context, CancellationToken cancellationToken = default)
        {
            await context.RespondAsync(HyperStatus.OK(), HyperSerializers.PlainTextAsync, cancellationToken);
            return Result.Success(default(HyperStatus));
        }
    }
}
