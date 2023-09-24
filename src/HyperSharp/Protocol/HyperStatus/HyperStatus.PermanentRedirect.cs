// <auto-generated/>
// Last modified at 2023-09-24.

#nullable enable
namespace HyperSharp.Protocol
{
    public readonly partial record struct HyperStatus
    {
        #if NET8_0_OR_GREATER

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.PermanentRedirect" />
        public static HyperStatus PermanentRedirect() => new(global::System.Net.HttpStatusCode.PermanentRedirect, new HyperHeaderCollection(), null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.PermanentRedirect" />
        /// <param name="body">The body of the response.</param>
        public static HyperStatus PermanentRedirect(object? body) => new(global::System.Net.HttpStatusCode.PermanentRedirect, new HyperHeaderCollection(), body);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.PermanentRedirect" />
        /// <param name="headers">The headers of the response.</param>
        public static HyperStatus PermanentRedirect(HyperHeaderCollection headers) => new(global::System.Net.HttpStatusCode.PermanentRedirect, headers, null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.PermanentRedirect" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus PermanentRedirect(HyperHeaderCollection headers, object? body) => new(global::System.Net.HttpStatusCode.PermanentRedirect, headers, body);

        #endif
    }
}
