using System.Threading;
using System.Threading.Tasks;
using OoLunar.HyperSharp.Results;

namespace OoLunar.HyperSharp.Responders
{
    public delegate ValueTask<Result<TOutput>> ValueResponderDelegate<TContext, TOutput>(TContext context, CancellationToken cancellationToken = default);

    public interface IValueResponder : IResponderBase
    {
        ValueTask<Result> RespondAsync(object context, CancellationToken cancellationToken = default);
    }

    public interface IValueResponder<TContext, TOutput> : IValueResponder
    {
        ValueTask<Result<TOutput>> RespondAsync(TContext context, CancellationToken cancellationToken = default);
        async ValueTask<Result> IValueResponder.RespondAsync(object context, CancellationToken cancellationToken) => await RespondAsync((TContext)context, cancellationToken);
    }
}
