// <auto-generated/>
// Last modified at 2024-06-28.

#nullable enable
namespace HyperSharp.Protocol
{
    public readonly partial record struct HyperStatus
    {
        /// <inheritdoc cref="global::System.Net.HttpStatusCode.PaymentRequired" />
        public static HyperStatus PaymentRequired() => new(global::System.Net.HttpStatusCode.PaymentRequired, new HyperHeaderCollection(), null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.PaymentRequired" />
        /// <param name="body">The body of the response.</param>
        public static HyperStatus PaymentRequired(object? body) => new(global::System.Net.HttpStatusCode.PaymentRequired, new HyperHeaderCollection(), body);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.PaymentRequired" />
        /// <param name="headers">The headers of the response.</param>
        public static HyperStatus PaymentRequired(HyperHeaderCollection headers) => new(global::System.Net.HttpStatusCode.PaymentRequired, headers, null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.PaymentRequired" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus PaymentRequired(HyperHeaderCollection headers, object? body) => new(global::System.Net.HttpStatusCode.PaymentRequired, headers, body);
    }
}
