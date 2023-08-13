using System;
using System.Threading;
using System.Threading.Tasks;
using HyperSharp.Responders;
using HyperSharp.Results;

namespace HyperSharp.Tests.Responders.IValueTaskResponder
{
    public sealed class FailureWithValueValueTaskResponder : IValueTaskResponder<string, string>
    {
        public static Type[] Needs { get; } = Array.Empty<Type>();
        public ValueTask<Result<string>> RespondAsync(string context, CancellationToken cancellationToken = default) => ValueTask.FromResult(Result.Failure(context, new Error("Failure Error")));
    }
}
