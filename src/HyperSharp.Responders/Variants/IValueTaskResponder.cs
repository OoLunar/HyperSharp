using System.Threading;
using System.Threading.Tasks;
using OoLunar.HyperSharp.Results;

namespace OoLunar.HyperSharp.Responders
{
    public delegate ValueTask<Result<TOutput>> ValueTaskResponderDelegate<TContext, TOutput>(TContext context, CancellationToken cancellationToken = default);

    public interface IValueTaskResponder : IResponderBase
    {
        ValueTask<Result> RespondAsync(object context, CancellationToken cancellationToken = default);
    }

    public interface IValueTaskResponder<TContext, TOutput> : IValueTaskResponder
    {
        ValueTask<Result<TOutput>> RespondAsync(TContext context, CancellationToken cancellationToken = default);
        async ValueTask<Result> IValueTaskResponder.RespondAsync(object context, CancellationToken cancellationToken) => await RespondAsync((TContext)context, cancellationToken);
    }
}
