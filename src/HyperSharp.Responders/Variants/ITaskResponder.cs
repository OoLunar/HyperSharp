using System.Threading;
using System.Threading.Tasks;
using HyperSharp.Results;

namespace HyperSharp.Responders
{
    /// <summary>
    /// Represents an asynchronous responder.
    /// </summary>
    public interface ITaskResponder : IResponderBase
    {
        /// <summary>
        /// Responds to the given context.
        /// </summary>
        /// <param name="context">The context that the responder will respond to.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous response.</returns>
        Task<Result> RespondAsync(object context, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Represents an asynchronous responder.
    /// </summary>
    public interface ITaskResponder<TContext, TOutput> : ITaskResponder
    {
        /// <summary>
        /// Responds to the given context.
        /// </summary>
        /// <param name="context">The context that the responder will respond to.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous response.</returns>
        Task<Result<TOutput>> RespondAsync(TContext context, CancellationToken cancellationToken = default);

        /// <inheritdoc cref="ITaskResponder.RespondAsync(object, CancellationToken)"/>
        async Task<Result> ITaskResponder.RespondAsync(object context, CancellationToken cancellationToken) => await RespondAsync((TContext)context, cancellationToken);
    }
}
