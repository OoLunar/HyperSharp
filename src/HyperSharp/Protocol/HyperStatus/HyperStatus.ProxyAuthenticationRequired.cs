// <auto-generated/>
// Last modified at 2024-06-28.

#nullable enable
namespace HyperSharp.Protocol
{
    public readonly partial record struct HyperStatus
    {
        /// <inheritdoc cref="global::System.Net.HttpStatusCode.ProxyAuthenticationRequired" />
        public static HyperStatus ProxyAuthenticationRequired() => new(global::System.Net.HttpStatusCode.ProxyAuthenticationRequired, new HyperHeaderCollection(), null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.ProxyAuthenticationRequired" />
        /// <param name="body">The body of the response.</param>
        public static HyperStatus ProxyAuthenticationRequired(object? body) => new(global::System.Net.HttpStatusCode.ProxyAuthenticationRequired, new HyperHeaderCollection(), body);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.ProxyAuthenticationRequired" />
        /// <param name="headers">The headers of the response.</param>
        public static HyperStatus ProxyAuthenticationRequired(HyperHeaderCollection headers) => new(global::System.Net.HttpStatusCode.ProxyAuthenticationRequired, headers, null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.ProxyAuthenticationRequired" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus ProxyAuthenticationRequired(HyperHeaderCollection headers, object? body) => new(global::System.Net.HttpStatusCode.ProxyAuthenticationRequired, headers, body);
    }
}
