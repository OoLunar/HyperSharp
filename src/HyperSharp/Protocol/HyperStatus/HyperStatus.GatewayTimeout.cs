// <auto-generated/>
// Last modified at 2023-09-24.

#nullable enable
namespace HyperSharp.Protocol
{
    public readonly partial record struct HyperStatus
    {
        #if NET7_0_OR_GREATER

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.GatewayTimeout" />
        public static HyperStatus GatewayTimeout() => new(global::System.Net.HttpStatusCode.GatewayTimeout, new HyperHeaderCollection(), null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.GatewayTimeout" />
        /// <param name="body">The body of the response.</param>
        public static HyperStatus GatewayTimeout(object? body) => new(global::System.Net.HttpStatusCode.GatewayTimeout, new HyperHeaderCollection(), body);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.GatewayTimeout" />
        /// <param name="headers">The headers of the response.</param>
        public static HyperStatus GatewayTimeout(HyperHeaderCollection headers) => new(global::System.Net.HttpStatusCode.GatewayTimeout, headers, null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.GatewayTimeout" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus GatewayTimeout(HyperHeaderCollection headers, object? body) => new(global::System.Net.HttpStatusCode.GatewayTimeout, headers, body);

        #endif
    }
}
