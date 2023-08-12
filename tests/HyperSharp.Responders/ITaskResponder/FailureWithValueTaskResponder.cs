using System;
using System.Threading;
using System.Threading.Tasks;
using OoLunar.HyperSharp.Responders;
using OoLunar.HyperSharp.Results;

namespace OoLunar.HyperSharp.Tests.Responders.ITaskResponder
{
    public sealed class FailureWithValueTaskResponder : ITaskResponder<string, string>
    {
        public static Type[] Needs { get; } = Array.Empty<Type>();
        public Task<Result<string>> RespondAsync(string context, CancellationToken cancellationToken = default) => Task.FromResult(Result.Failure(context, new Error("Failure Error")));
    }
}
