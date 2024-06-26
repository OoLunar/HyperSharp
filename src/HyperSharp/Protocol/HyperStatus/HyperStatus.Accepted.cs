// <auto-generated/>
// Last modified at 2024-06-28.

#nullable enable
namespace HyperSharp.Protocol
{
    public readonly partial record struct HyperStatus
    {
        /// <inheritdoc cref="global::System.Net.HttpStatusCode.Accepted" />
        public static HyperStatus Accepted() => new(global::System.Net.HttpStatusCode.Accepted, new HyperHeaderCollection(), null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.Accepted" />
        /// <param name="body">The body of the response.</param>
        public static HyperStatus Accepted(object? body) => new(global::System.Net.HttpStatusCode.Accepted, new HyperHeaderCollection(), body);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.Accepted" />
        /// <param name="headers">The headers of the response.</param>
        public static HyperStatus Accepted(HyperHeaderCollection headers) => new(global::System.Net.HttpStatusCode.Accepted, headers, null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.Accepted" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus Accepted(HyperHeaderCollection headers, object? body) => new(global::System.Net.HttpStatusCode.Accepted, headers, body);
    }
}
