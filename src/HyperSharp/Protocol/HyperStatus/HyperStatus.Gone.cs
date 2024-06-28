// <auto-generated/>
// Last modified at 2024-06-28.

#nullable enable
namespace HyperSharp.Protocol
{
    public readonly partial record struct HyperStatus
    {
        /// <inheritdoc cref="global::System.Net.HttpStatusCode.Gone" />
        public static HyperStatus Gone() => new(global::System.Net.HttpStatusCode.Gone, new HyperHeaderCollection(), null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.Gone" />
        /// <param name="body">The body of the response.</param>
        public static HyperStatus Gone(object? body) => new(global::System.Net.HttpStatusCode.Gone, new HyperHeaderCollection(), body);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.Gone" />
        /// <param name="headers">The headers of the response.</param>
        public static HyperStatus Gone(HyperHeaderCollection headers) => new(global::System.Net.HttpStatusCode.Gone, headers, null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.Gone" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus Gone(HyperHeaderCollection headers, object? body) => new(global::System.Net.HttpStatusCode.Gone, headers, body);
    }
}
