// <auto-generated/>
// Last modified at 2024-07-30.

#nullable enable
namespace HyperSharp.Protocol
{
    public readonly partial record struct HyperStatus
    {
        /// <inheritdoc cref="global::System.Net.HttpStatusCode.MethodNotAllowed" />
        public static HyperStatus MethodNotAllowed() => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.MethodNotAllowed
        };

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.MethodNotAllowed" />
        /// <param name="body">The body of the response.</param>
        public static HyperStatus MethodNotAllowed(object? body) => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.MethodNotAllowed,
            Body = body
        };

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.MethodNotAllowed" />
        /// <param name="body">The body of the response.</param>
        /// <param name="serializer">Which serializer to use to serialize the body.</param>
        public static HyperStatus MethodNotAllowed(object? body, HyperSerializerDelegate serializer) => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.MethodNotAllowed,
            Body = body,
            Serializer = serializer
        };

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.MethodNotAllowed" />
        /// <param name="headers">The headers of the response.</param>
        public static HyperStatus MethodNotAllowed(HyperHeaderCollection headers) => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.MethodNotAllowed,
            Headers = headers
        };

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.MethodNotAllowed" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus MethodNotAllowed(HyperHeaderCollection headers, object? body) => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.MethodNotAllowed,
            Headers = headers,
            Body = body
        };

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.MethodNotAllowed" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus MethodNotAllowed(HyperHeaderCollection headers, object? body, HyperSerializerDelegate serializer) => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.MethodNotAllowed,
            Headers = headers,
            Body = body,
            Serializer = serializer
        };
    }
}
