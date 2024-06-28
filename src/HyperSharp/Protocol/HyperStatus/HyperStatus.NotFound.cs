// <auto-generated/>
// Last modified at 2024-06-28.

#nullable enable
namespace HyperSharp.Protocol
{
    public readonly partial record struct HyperStatus
    {
        /// <inheritdoc cref="global::System.Net.HttpStatusCode.NotFound" />
        public static HyperStatus NotFound() => new(global::System.Net.HttpStatusCode.NotFound, new HyperHeaderCollection(), null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.NotFound" />
        /// <param name="body">The body of the response.</param>
        public static HyperStatus NotFound(object? body) => new(global::System.Net.HttpStatusCode.NotFound, new HyperHeaderCollection(), body);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.NotFound" />
        /// <param name="headers">The headers of the response.</param>
        public static HyperStatus NotFound(HyperHeaderCollection headers) => new(global::System.Net.HttpStatusCode.NotFound, headers, null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.NotFound" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus NotFound(HyperHeaderCollection headers, object? body) => new(global::System.Net.HttpStatusCode.NotFound, headers, body);
    }
}
