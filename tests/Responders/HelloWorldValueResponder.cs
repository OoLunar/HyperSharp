using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using OoLunar.HyperSharp.Protocol;
using OoLunar.HyperSharp.Responders;
using OoLunar.HyperSharp.Results;

namespace OoLunar.HyperSharp.Tests.Responders
{
    public readonly record struct HelloWorldValueResponder : IValueResponder<HyperContext, HyperStatus>
    {
        public static Type[] Needs => Type.EmptyTypes;

        public ValueTask<Result<HyperStatus>> RespondAsync(HyperContext context, CancellationToken cancellationToken = default) => ValueTask.FromResult(Result.Success(new HyperStatus(
            HttpStatusCode.OK,
            new Error("Hello World!")
        )));
    }
}
