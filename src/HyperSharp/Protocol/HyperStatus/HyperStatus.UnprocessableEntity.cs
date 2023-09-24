// <auto-generated/>
// Last modified at 2023-09-24.

#nullable enable
namespace HyperSharp.Protocol
{
    public readonly partial record struct HyperStatus
    {
        #if NET8_0_OR_GREATER

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.UnprocessableEntity" />
        public static HyperStatus UnprocessableEntity() => new(global::System.Net.HttpStatusCode.UnprocessableEntity, new HyperHeaderCollection(), null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.UnprocessableEntity" />
        /// <param name="body">The body of the response.</param>
        public static HyperStatus UnprocessableEntity(object? body) => new(global::System.Net.HttpStatusCode.UnprocessableEntity, new HyperHeaderCollection(), body);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.UnprocessableEntity" />
        /// <param name="headers">The headers of the response.</param>
        public static HyperStatus UnprocessableEntity(HyperHeaderCollection headers) => new(global::System.Net.HttpStatusCode.UnprocessableEntity, headers, null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.UnprocessableEntity" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus UnprocessableEntity(HyperHeaderCollection headers, object? body) => new(global::System.Net.HttpStatusCode.UnprocessableEntity, headers, body);

        #endif
    }
}
