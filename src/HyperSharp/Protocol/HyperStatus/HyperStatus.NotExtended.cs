// <auto-generated/>
// Last modified at 2024-07-30.

#nullable enable
namespace HyperSharp.Protocol
{
    public readonly partial record struct HyperStatus
    {
        /// <inheritdoc cref="global::System.Net.HttpStatusCode.NotExtended" />
        public static HyperStatus NotExtended() => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.NotExtended
        };

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.NotExtended" />
        /// <param name="body">The body of the response.</param>
        public static HyperStatus NotExtended(object? body) => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.NotExtended,
            Body = body
        };

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.NotExtended" />
        /// <param name="body">The body of the response.</param>
        /// <param name="serializer">Which serializer to use to serialize the body.</param>
        public static HyperStatus NotExtended(object? body, HyperSerializerDelegate serializer) => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.NotExtended,
            Body = body,
            Serializer = serializer
        };

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.NotExtended" />
        /// <param name="headers">The headers of the response.</param>
        public static HyperStatus NotExtended(HyperHeaderCollection headers) => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.NotExtended,
            Headers = headers
        };

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.NotExtended" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus NotExtended(HyperHeaderCollection headers, object? body) => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.NotExtended,
            Headers = headers,
            Body = body
        };

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.NotExtended" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus NotExtended(HyperHeaderCollection headers, object? body, HyperSerializerDelegate serializer) => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.NotExtended,
            Headers = headers,
            Body = body,
            Serializer = serializer
        };
    }
}
