using System.Threading;
using System.Threading.Tasks;
using HyperSharp.Results;

namespace HyperSharp.Responders
{
    /// <summary>
    /// Represents an asynchronous responder.
    /// </summary>
    /// <param name="context">The context that the responder will respond to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous response.</returns>
    public delegate ValueTask<Result<TOutput>> ValueTaskResponderDelegate<TContext, TOutput>(TContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Represents an asynchronous responder.
    /// </summary>
    public interface IValueTaskResponder : IResponderBase
    {
        /// <summary>
        /// Responds to the given context.
        /// </summary>
        /// <param name="context">The context that the responder will respond to.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous response.</returns>
        ValueTask<Result> RespondAsync(object context, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Represents an asynchronous responder.
    /// </summary>
    public interface IValueTaskResponder<TContext, TOutput> : IValueTaskResponder
    {
        /// <summary>
        /// Responds to the given context.
        /// </summary>
        /// <param name="context">The context that the responder will respond to.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous response.</returns>
        ValueTask<Result<TOutput>> RespondAsync(TContext context, CancellationToken cancellationToken = default);

        /// <inheritdoc cref="IValueTaskResponder.RespondAsync(object, CancellationToken)"/>
        async ValueTask<Result> IValueTaskResponder.RespondAsync(object context, CancellationToken cancellationToken) => await RespondAsync((TContext)context, cancellationToken);
    }
}
