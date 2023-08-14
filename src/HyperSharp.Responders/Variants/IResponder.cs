using System.Threading;
using HyperSharp.Results;

namespace HyperSharp.Responders
{
    /// <summary>
    /// Represents a synchronous responder.
    /// </summary>
    /// <param name="context">The context that the responder will respond to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public delegate Result<TOutput> ResponderDelegate<TContext, TOutput>(TContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Represents a synchronous responder.
    /// </summary>
    public interface IResponder : IResponderBase
    {
        /// <summary>
        /// Responds to the given context.
        /// </summary>
        /// <param name="context">The context that the responder will respond to.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the response.</returns>
        Result Respond(object context, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Represents a synchronous responder.
    /// </summary>
    public interface IResponder<TContext, TOutput> : IResponder
    {
        /// <summary>
        /// Responds to the given context.
        /// </summary>
        /// <param name="context">The context that the responder will respond to.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the response.</returns>
        Result<TOutput> Respond(TContext context, CancellationToken cancellationToken = default);

        /// <inheritdoc cref="IResponder.Respond(object, CancellationToken)"/>
        Result IResponder.Respond(object context, CancellationToken cancellationToken) => Respond((TContext)context, cancellationToken);
    }
}
