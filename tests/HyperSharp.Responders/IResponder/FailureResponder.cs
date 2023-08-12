using System;
using System.Threading;
using OoLunar.HyperSharp.Responders;
using OoLunar.HyperSharp.Results;

namespace OoLunar.HyperSharp.Tests.Responders.IResponder
{
    public sealed class FailureResponder : IResponder<string, string>
    {
        public static Type[] Needs { get; } = Array.Empty<Type>();
        public Result<string> Respond(string context, CancellationToken cancellationToken = default) => Result.Failure<string>("Failure");
    }
}
