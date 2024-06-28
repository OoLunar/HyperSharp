// <auto-generated/>
// Last modified at 2024-06-28.

#nullable enable
namespace HyperSharp.Protocol
{
    public readonly partial record struct HyperStatus
    {
        /// <inheritdoc cref="global::System.Net.HttpStatusCode.Found" />
        public static HyperStatus Found() => new(global::System.Net.HttpStatusCode.Found, new HyperHeaderCollection(), null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.Found" />
        /// <param name="body">The body of the response.</param>
        public static HyperStatus Found(object? body) => new(global::System.Net.HttpStatusCode.Found, new HyperHeaderCollection(), body);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.Found" />
        /// <param name="headers">The headers of the response.</param>
        public static HyperStatus Found(HyperHeaderCollection headers) => new(global::System.Net.HttpStatusCode.Found, headers, null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.Found" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus Found(HyperHeaderCollection headers, object? body) => new(global::System.Net.HttpStatusCode.Found, headers, body);
    }
}
