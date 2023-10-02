// <auto-generated/>
// Last modified at 2023-09-24.

#nullable enable
namespace HyperSharp.Protocol
{
    public readonly partial record struct HyperStatus
    {
        #if NET7_0_OR_GREATER

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.SwitchingProtocols" />
        public static HyperStatus SwitchingProtocols() => new(global::System.Net.HttpStatusCode.SwitchingProtocols, new HyperHeaderCollection(), null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.SwitchingProtocols" />
        /// <param name="body">The body of the response.</param>
        public static HyperStatus SwitchingProtocols(object? body) => new(global::System.Net.HttpStatusCode.SwitchingProtocols, new HyperHeaderCollection(), body);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.SwitchingProtocols" />
        /// <param name="headers">The headers of the response.</param>
        public static HyperStatus SwitchingProtocols(HyperHeaderCollection headers) => new(global::System.Net.HttpStatusCode.SwitchingProtocols, headers, null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.SwitchingProtocols" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus SwitchingProtocols(HyperHeaderCollection headers, object? body) => new(global::System.Net.HttpStatusCode.SwitchingProtocols, headers, body);

        #endif
    }
}