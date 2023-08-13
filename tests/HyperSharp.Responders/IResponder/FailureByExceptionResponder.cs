using System;
using System.Threading;
using HyperSharp.Responders;
using HyperSharp.Results;

namespace HyperSharp.Tests.Responders.IResponder
{
    public sealed class FailureByExceptionResponder : IResponder<string, string>
    {
        public static Type[] Needs { get; } = Array.Empty<Type>();
        public Result<string> Respond(string context, CancellationToken cancellationToken = default) => throw new Exception("Failure");
    }
}
