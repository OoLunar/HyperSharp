// <auto-generated/>
// Last modified at 2024-07-30.

#nullable enable
namespace HyperSharp.Protocol
{
    public readonly partial record struct HyperStatus
    {
        /// <inheritdoc cref="global::System.Net.HttpStatusCode.Conflict" />
        public static HyperStatus Conflict() => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.Conflict
        };

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.Conflict" />
        /// <param name="body">The body of the response.</param>
        public static HyperStatus Conflict(object? body) => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.Conflict,
            Body = body
        };

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.Conflict" />
        /// <param name="body">The body of the response.</param>
        /// <param name="serializer">Which serializer to use to serialize the body.</param>
        public static HyperStatus Conflict(object? body, HyperSerializerDelegate serializer) => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.Conflict,
            Body = body,
            Serializer = serializer
        };

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.Conflict" />
        /// <param name="headers">The headers of the response.</param>
        public static HyperStatus Conflict(HyperHeaderCollection headers) => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.Conflict,
            Headers = headers
        };

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.Conflict" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus Conflict(HyperHeaderCollection headers, object? body) => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.Conflict,
            Headers = headers,
            Body = body
        };

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.Conflict" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus Conflict(HyperHeaderCollection headers, object? body, HyperSerializerDelegate serializer) => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.Conflict,
            Headers = headers,
            Body = body,
            Serializer = serializer
        };
    }
}
