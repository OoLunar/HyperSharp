using System.Threading;
using System.Threading.Tasks;

namespace HyperSharp.Protocol
{
    /// <summary>
    /// The delegate to be called when serializing the body to the client.
    /// </summary>
    /// <param name="context">The <see cref="HyperContext"/> for the current request.</param>
    /// <param name="status">The returned <see cref="HyperStatus"/> from the responder.</param>
    /// <param name="cancellationToken">A cancellation token to stop the serializer.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation, returning a <see cref="bool"/> indicating whether the serializer was successful.</returns>
    public delegate ValueTask<bool> HyperSerializerDelegate(HyperContext context, HyperStatus status, CancellationToken cancellationToken = default);
}
