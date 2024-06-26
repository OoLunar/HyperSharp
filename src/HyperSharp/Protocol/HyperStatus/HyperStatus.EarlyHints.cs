// <auto-generated/>
// Last modified at 2024-06-28.

#nullable enable
namespace HyperSharp.Protocol
{
    public readonly partial record struct HyperStatus
    {
        /// <inheritdoc cref="global::System.Net.HttpStatusCode.EarlyHints" />
        public static HyperStatus EarlyHints() => new(global::System.Net.HttpStatusCode.EarlyHints, new HyperHeaderCollection(), null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.EarlyHints" />
        /// <param name="body">The body of the response.</param>
        public static HyperStatus EarlyHints(object? body) => new(global::System.Net.HttpStatusCode.EarlyHints, new HyperHeaderCollection(), body);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.EarlyHints" />
        /// <param name="headers">The headers of the response.</param>
        public static HyperStatus EarlyHints(HyperHeaderCollection headers) => new(global::System.Net.HttpStatusCode.EarlyHints, headers, null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.EarlyHints" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus EarlyHints(HyperHeaderCollection headers, object? body) => new(global::System.Net.HttpStatusCode.EarlyHints, headers, body);
    }
}
