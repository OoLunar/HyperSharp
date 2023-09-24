// <auto-generated/>
// Last modified at 2023-09-24.

#nullable enable
namespace HyperSharp.Protocol
{
    public readonly partial record struct HyperStatus
    {
        #if NET7_0_OR_GREATER

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.MovedPermanently" />
        public static HyperStatus MovedPermanently() => new(global::System.Net.HttpStatusCode.MovedPermanently, new HyperHeaderCollection(), null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.MovedPermanently" />
        /// <param name="body">The body of the response.</param>
        public static HyperStatus MovedPermanently(object? body) => new(global::System.Net.HttpStatusCode.MovedPermanently, new HyperHeaderCollection(), body);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.MovedPermanently" />
        /// <param name="headers">The headers of the response.</param>
        public static HyperStatus MovedPermanently(HyperHeaderCollection headers) => new(global::System.Net.HttpStatusCode.MovedPermanently, headers, null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.MovedPermanently" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus MovedPermanently(HyperHeaderCollection headers, object? body) => new(global::System.Net.HttpStatusCode.MovedPermanently, headers, body);

        #endif
    }
}
