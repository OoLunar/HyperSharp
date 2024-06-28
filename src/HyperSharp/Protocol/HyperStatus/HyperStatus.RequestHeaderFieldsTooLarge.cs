// <auto-generated/>
// Last modified at 2024-06-28.

#nullable enable
namespace HyperSharp.Protocol
{
    public readonly partial record struct HyperStatus
    {
        /// <inheritdoc cref="global::System.Net.HttpStatusCode.RequestHeaderFieldsTooLarge" />
        public static HyperStatus RequestHeaderFieldsTooLarge() => new(global::System.Net.HttpStatusCode.RequestHeaderFieldsTooLarge, new HyperHeaderCollection(), null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.RequestHeaderFieldsTooLarge" />
        /// <param name="body">The body of the response.</param>
        public static HyperStatus RequestHeaderFieldsTooLarge(object? body) => new(global::System.Net.HttpStatusCode.RequestHeaderFieldsTooLarge, new HyperHeaderCollection(), body);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.RequestHeaderFieldsTooLarge" />
        /// <param name="headers">The headers of the response.</param>
        public static HyperStatus RequestHeaderFieldsTooLarge(HyperHeaderCollection headers) => new(global::System.Net.HttpStatusCode.RequestHeaderFieldsTooLarge, headers, null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.RequestHeaderFieldsTooLarge" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus RequestHeaderFieldsTooLarge(HyperHeaderCollection headers, object? body) => new(global::System.Net.HttpStatusCode.RequestHeaderFieldsTooLarge, headers, body);
    }
}
