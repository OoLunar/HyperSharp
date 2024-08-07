// <auto-generated/>
// Last modified at 2024-07-30.

#nullable enable
namespace HyperSharp.Protocol
{
    public readonly partial record struct HyperStatus
    {
        /// <inheritdoc cref="global::System.Net.HttpStatusCode.LengthRequired" />
        public static HyperStatus LengthRequired() => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.LengthRequired
        };

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.LengthRequired" />
        /// <param name="body">The body of the response.</param>
        public static HyperStatus LengthRequired(object? body) => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.LengthRequired,
            Body = body
        };

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.LengthRequired" />
        /// <param name="body">The body of the response.</param>
        /// <param name="serializer">Which serializer to use to serialize the body.</param>
        public static HyperStatus LengthRequired(object? body, HyperSerializerDelegate serializer) => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.LengthRequired,
            Body = body,
            Serializer = serializer
        };

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.LengthRequired" />
        /// <param name="headers">The headers of the response.</param>
        public static HyperStatus LengthRequired(HyperHeaderCollection headers) => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.LengthRequired,
            Headers = headers
        };

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.LengthRequired" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus LengthRequired(HyperHeaderCollection headers, object? body) => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.LengthRequired,
            Headers = headers,
            Body = body
        };

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.LengthRequired" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus LengthRequired(HyperHeaderCollection headers, object? body, HyperSerializerDelegate serializer) => new HyperStatus()
        {
            Code = global::System.Net.HttpStatusCode.LengthRequired,
            Headers = headers,
            Body = body,
            Serializer = serializer
        };
    }
}
