// <auto-generated/>
// Last modified at 2024-06-28.

#nullable enable
namespace HyperSharp.Protocol
{
    public readonly partial record struct HyperStatus
    {
        /// <inheritdoc cref="global::System.Net.HttpStatusCode.MisdirectedRequest" />
        public static HyperStatus MisdirectedRequest() => new(global::System.Net.HttpStatusCode.MisdirectedRequest, new HyperHeaderCollection(), null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.MisdirectedRequest" />
        /// <param name="body">The body of the response.</param>
        public static HyperStatus MisdirectedRequest(object? body) => new(global::System.Net.HttpStatusCode.MisdirectedRequest, new HyperHeaderCollection(), body);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.MisdirectedRequest" />
        /// <param name="headers">The headers of the response.</param>
        public static HyperStatus MisdirectedRequest(HyperHeaderCollection headers) => new(global::System.Net.HttpStatusCode.MisdirectedRequest, headers, null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.MisdirectedRequest" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus MisdirectedRequest(HyperHeaderCollection headers, object? body) => new(global::System.Net.HttpStatusCode.MisdirectedRequest, headers, body);
    }
}
