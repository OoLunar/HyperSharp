using System;
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

        public async ValueTask<Result<HyperStatus>> RespondAsync(HyperContext context, CancellationToken cancellationToken = default)
        {
            await context.RespondAsync(HyperStatus.OK("Hello World!"), HyperSerializers.PlainTextAsync, cancellationToken);
            return Result.Success(default(HyperStatus));
        }
    }
}
