// <auto-generated/>
// Last modified at 2024-06-28.

#nullable enable
namespace HyperSharp.Protocol
{
    public readonly partial record struct HyperStatus
    {
        /// <inheritdoc cref="global::System.Net.HttpStatusCode.Forbidden" />
        public static HyperStatus Forbidden() => new(global::System.Net.HttpStatusCode.Forbidden, new HyperHeaderCollection(), null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.Forbidden" />
        /// <param name="body">The body of the response.</param>
        public static HyperStatus Forbidden(object? body) => new(global::System.Net.HttpStatusCode.Forbidden, new HyperHeaderCollection(), body);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.Forbidden" />
        /// <param name="headers">The headers of the response.</param>
        public static HyperStatus Forbidden(HyperHeaderCollection headers) => new(global::System.Net.HttpStatusCode.Forbidden, headers, null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.Forbidden" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus Forbidden(HyperHeaderCollection headers, object? body) => new(global::System.Net.HttpStatusCode.Forbidden, headers, body);
    }
}
