// <auto-generated/>
// Last modified at 2023-09-24.

#nullable enable
namespace HyperSharp.Protocol
{
    public readonly partial record struct HyperStatus
    {
        #if NET7_0_OR_GREATER

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.HttpVersionNotSupported" />
        public static HyperStatus HttpVersionNotSupported() => new(global::System.Net.HttpStatusCode.HttpVersionNotSupported, new HyperHeaderCollection(), null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.HttpVersionNotSupported" />
        /// <param name="body">The body of the response.</param>
        public static HyperStatus HttpVersionNotSupported(object? body) => new(global::System.Net.HttpStatusCode.HttpVersionNotSupported, new HyperHeaderCollection(), body);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.HttpVersionNotSupported" />
        /// <param name="headers">The headers of the response.</param>
        public static HyperStatus HttpVersionNotSupported(HyperHeaderCollection headers) => new(global::System.Net.HttpStatusCode.HttpVersionNotSupported, headers, null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.HttpVersionNotSupported" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus HttpVersionNotSupported(HyperHeaderCollection headers, object? body) => new(global::System.Net.HttpStatusCode.HttpVersionNotSupported, headers, body);

        #endif
    }
}