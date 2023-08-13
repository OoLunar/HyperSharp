using System;
using System.Threading;
using System.Threading.Tasks;
using HyperSharp.Responders;
using HyperSharp.Results;

namespace HyperSharp.Tests.Responders.IValueTaskResponder
{
    public sealed class AsyncSuccessValueTaskResponder : IValueTaskResponder<string, string>
    {
        public static Type[] Needs { get; } = Array.Empty<Type>();
        public async ValueTask<Result<string>> RespondAsync(string context, CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken);
            return Result.Success<string>();
        }
    }
}
