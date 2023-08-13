using System;
using System.Threading;
using System.Threading.Tasks;
using HyperSharp.Responders;
using HyperSharp.Results;

namespace HyperSharp.Tests.Responders.ITaskResponder
{
    public sealed class FailureByExceptionTaskResponder : ITaskResponder<string, string>
    {
        public static Type[] Needs { get; } = Array.Empty<Type>();
        public Task<Result<string>> RespondAsync(string context, CancellationToken cancellationToken = default) => throw new Exception("Failure");
    }
}
