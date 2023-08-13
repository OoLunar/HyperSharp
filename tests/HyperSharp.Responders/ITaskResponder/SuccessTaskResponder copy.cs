using System;
using System.Threading;
using System.Threading.Tasks;
using HyperSharp.Responders;
using HyperSharp.Results;

namespace HyperSharp.Tests.Responders.ITaskResponder
{
    public sealed class AsyncSuccessTaskResponder : ITaskResponder<string, string>
    {
        public static Type[] Needs { get; } = Array.Empty<Type>();
        public async Task<Result<string>> RespondAsync(string context, CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken);
            return Result.Success<string>();
        }
    }
}
