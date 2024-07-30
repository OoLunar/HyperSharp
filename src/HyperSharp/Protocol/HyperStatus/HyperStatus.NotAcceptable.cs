// <auto-generated/>
// Last modified at 2024-07-30.

#nullable enable
namespace HyperSharp.Protocol
{
    public readonly partial record struct HyperStatus
    {
        /// <inheritdoc cref="global::System.Net.HttpStatusCode.NotAcceptable" />
        public static HyperStatus NotAcceptable() => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.NotAcceptable
        };

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.NotAcceptable" />
        /// <param name="body">The body of the response.</param>
        public static HyperStatus NotAcceptable(object? body) => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.NotAcceptable,
            Body = body
        };

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.NotAcceptable" />
        /// <param name="body">The body of the response.</param>
        /// <param name="serializer">Which serializer to use to serialize the body.</param>
        public static HyperStatus NotAcceptable(object? body, HyperSerializerDelegate serializer) => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.NotAcceptable,
            Body = body,
            Serializer = serializer
        };

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.NotAcceptable" />
        /// <param name="headers">The headers of the response.</param>
        public static HyperStatus NotAcceptable(HyperHeaderCollection headers) => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.NotAcceptable,
            Headers = headers
        };

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.NotAcceptable" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus NotAcceptable(HyperHeaderCollection headers, object? body) => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.NotAcceptable,
            Headers = headers,
            Body = body
        };

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.NotAcceptable" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus NotAcceptable(HyperHeaderCollection headers, object? body, HyperSerializerDelegate serializer) => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.NotAcceptable,
            Headers = headers,
            Body = body,
            Serializer = serializer
        };
    }
}
