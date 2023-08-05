using System.Threading;
using System.Threading.Tasks;
using OoLunar.HyperSharp.Results;

namespace OoLunar.HyperSharp.Responders
{
    public interface ITaskResponder : IResponderBase
    {
        Task<Result> RespondAsync(object context, CancellationToken cancellationToken = default);
    }

    public interface ITaskResponder<TContext, TOutput> : ITaskResponder
    {
        Task<Result<TOutput>> RespondAsync(TContext context, CancellationToken cancellationToken = default);
        async Task<Result> ITaskResponder.RespondAsync(object context, CancellationToken cancellationToken) => await RespondAsync((TContext)context, cancellationToken);
    }
}
