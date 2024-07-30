// <auto-generated/>
// Last modified at 2024-07-30.

#nullable enable
namespace HyperSharp.Protocol
{
    public readonly partial record struct HyperStatus
    {
        /// <inheritdoc cref="global::System.Net.HttpStatusCode.RequestHeaderFieldsTooLarge" />
        public static HyperStatus RequestHeaderFieldsTooLarge() => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.RequestHeaderFieldsTooLarge
        };

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.RequestHeaderFieldsTooLarge" />
        /// <param name="body">The body of the response.</param>
        public static HyperStatus RequestHeaderFieldsTooLarge(object? body) => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.RequestHeaderFieldsTooLarge,
            Body = body
        };

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.RequestHeaderFieldsTooLarge" />
        /// <param name="body">The body of the response.</param>
        /// <param name="serializer">Which serializer to use to serialize the body.</param>
        public static HyperStatus RequestHeaderFieldsTooLarge(object? body, HyperSerializerDelegate serializer) => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.RequestHeaderFieldsTooLarge,
            Body = body,
            Serializer = serializer
        };

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.RequestHeaderFieldsTooLarge" />
        /// <param name="headers">The headers of the response.</param>
        public static HyperStatus RequestHeaderFieldsTooLarge(HyperHeaderCollection headers) => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.RequestHeaderFieldsTooLarge,
            Headers = headers
        };

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.RequestHeaderFieldsTooLarge" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus RequestHeaderFieldsTooLarge(HyperHeaderCollection headers, object? body) => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.RequestHeaderFieldsTooLarge,
            Headers = headers,
            Body = body
        };

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.RequestHeaderFieldsTooLarge" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus RequestHeaderFieldsTooLarge(HyperHeaderCollection headers, object? body, HyperSerializerDelegate serializer) => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.RequestHeaderFieldsTooLarge,
            Headers = headers,
            Body = body,
            Serializer = serializer
        };
    }
}
