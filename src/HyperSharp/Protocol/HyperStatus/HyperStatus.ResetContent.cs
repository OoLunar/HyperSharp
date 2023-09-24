// <auto-generated/>
// Last modified at 2023-09-24.

#nullable enable
namespace HyperSharp.Protocol
{
    public readonly partial record struct HyperStatus
    {
        #if NET8_0_OR_GREATER

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.ResetContent" />
        public static HyperStatus ResetContent() => new(global::System.Net.HttpStatusCode.ResetContent, new HyperHeaderCollection(), null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.ResetContent" />
        /// <param name="body">The body of the response.</param>
        public static HyperStatus ResetContent(object? body) => new(global::System.Net.HttpStatusCode.ResetContent, new HyperHeaderCollection(), body);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.ResetContent" />
        /// <param name="headers">The headers of the response.</param>
        public static HyperStatus ResetContent(HyperHeaderCollection headers) => new(global::System.Net.HttpStatusCode.ResetContent, headers, null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.ResetContent" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus ResetContent(HyperHeaderCollection headers, object? body) => new(global::System.Net.HttpStatusCode.ResetContent, headers, body);

        #endif
    }
}
