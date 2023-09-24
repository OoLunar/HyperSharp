// <auto-generated/>
// Last modified at 2023-09-24.

#nullable enable
namespace HyperSharp.Protocol
{
    public readonly partial record struct HyperStatus
    {
        #if NET7_0_OR_GREATER

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.NotModified" />
        public static HyperStatus NotModified() => new(global::System.Net.HttpStatusCode.NotModified, new HyperHeaderCollection(), null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.NotModified" />
        /// <param name="body">The body of the response.</param>
        public static HyperStatus NotModified(object? body) => new(global::System.Net.HttpStatusCode.NotModified, new HyperHeaderCollection(), body);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.NotModified" />
        /// <param name="headers">The headers of the response.</param>
        public static HyperStatus NotModified(HyperHeaderCollection headers) => new(global::System.Net.HttpStatusCode.NotModified, headers, null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.NotModified" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus NotModified(HyperHeaderCollection headers, object? body) => new(global::System.Net.HttpStatusCode.NotModified, headers, body);

        #endif
    }
}
