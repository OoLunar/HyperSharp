// <auto-generated/>
// Last modified at 2024-06-28.

#nullable enable
namespace HyperSharp.Protocol
{
    public readonly partial record struct HyperStatus
    {
        /// <inheritdoc cref="global::System.Net.HttpStatusCode.PreconditionFailed" />
        public static HyperStatus PreconditionFailed() => new(global::System.Net.HttpStatusCode.PreconditionFailed, new HyperHeaderCollection(), null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.PreconditionFailed" />
        /// <param name="body">The body of the response.</param>
        public static HyperStatus PreconditionFailed(object? body) => new(global::System.Net.HttpStatusCode.PreconditionFailed, new HyperHeaderCollection(), body);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.PreconditionFailed" />
        /// <param name="headers">The headers of the response.</param>
        public static HyperStatus PreconditionFailed(HyperHeaderCollection headers) => new(global::System.Net.HttpStatusCode.PreconditionFailed, headers, null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.PreconditionFailed" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus PreconditionFailed(HyperHeaderCollection headers, object? body) => new(global::System.Net.HttpStatusCode.PreconditionFailed, headers, body);
    }
}
