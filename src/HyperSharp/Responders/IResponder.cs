using System.Threading;
using System.Threading.Tasks;
using FluentResults;

namespace OoLunar.HyperSharp.Responders
{
    public delegate Task<Result<TOutput>> ResponderDelegate<TInput, TOutput>(TInput context, CancellationToken cancellationToken) where TOutput : new();

    public interface IResponder
    {
        string[] Implements { get; init; }

        Task<Result> RespondAsync(object context, CancellationToken cancellationToken = default);
    }

    public interface IResponder<TInput, TOutput> : IResponder
    {
        Task<Result<TOutput>> RespondAsync(TInput context, CancellationToken cancellationToken = default);
        async Task<Result> IResponder.RespondAsync(object context, CancellationToken cancellationToken) => (Result)(ResultBase)await RespondAsync((TInput)context, cancellationToken);
    }
}
