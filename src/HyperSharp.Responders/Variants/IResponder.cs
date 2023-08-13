using System.Threading;
using HyperSharp.Results;

namespace HyperSharp.Responders
{
    public delegate Result<TOutput> ResponderDelegate<TContext, TOutput>(TContext context, CancellationToken cancellationToken = default);

    public interface IResponder : IResponderBase
    {
        Result Respond(object context, CancellationToken cancellationToken = default);
    }

    public interface IResponder<TContext, TOutput> : IResponder
    {
        Result<TOutput> Respond(TContext context, CancellationToken cancellationToken = default);
        Result IResponder.Respond(object context, CancellationToken cancellationToken) => Respond((TContext)context, cancellationToken);
    }
}
