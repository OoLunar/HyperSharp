// <auto-generated/>
// Last modified at 2024-06-28.

#nullable enable
namespace HyperSharp.Protocol
{
    public readonly partial record struct HyperStatus
    {
        /// <inheritdoc cref="global::System.Net.HttpStatusCode.BadRequest" />
        public static HyperStatus BadRequest() => new(global::System.Net.HttpStatusCode.BadRequest, new HyperHeaderCollection(), null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.BadRequest" />
        /// <param name="body">The body of the response.</param>
        public static HyperStatus BadRequest(object? body) => new(global::System.Net.HttpStatusCode.BadRequest, new HyperHeaderCollection(), body);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.BadRequest" />
        /// <param name="headers">The headers of the response.</param>
        public static HyperStatus BadRequest(HyperHeaderCollection headers) => new(global::System.Net.HttpStatusCode.BadRequest, headers, null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.BadRequest" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus BadRequest(HyperHeaderCollection headers, object? body) => new(global::System.Net.HttpStatusCode.BadRequest, headers, body);
    }
}
