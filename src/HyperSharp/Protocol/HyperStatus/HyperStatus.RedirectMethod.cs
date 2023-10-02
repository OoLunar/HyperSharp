// <auto-generated/>
// Last modified at 2023-09-24.

#nullable enable
namespace HyperSharp.Protocol
{
    public readonly partial record struct HyperStatus
    {
        #if NET7_0_OR_GREATER

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.RedirectMethod" />
        public static HyperStatus RedirectMethod() => new(global::System.Net.HttpStatusCode.RedirectMethod, new HyperHeaderCollection(), null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.RedirectMethod" />
        /// <param name="body">The body of the response.</param>
        public static HyperStatus RedirectMethod(object? body) => new(global::System.Net.HttpStatusCode.RedirectMethod, new HyperHeaderCollection(), body);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.RedirectMethod" />
        /// <param name="headers">The headers of the response.</param>
        public static HyperStatus RedirectMethod(HyperHeaderCollection headers) => new(global::System.Net.HttpStatusCode.RedirectMethod, headers, null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.RedirectMethod" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus RedirectMethod(HyperHeaderCollection headers, object? body) => new(global::System.Net.HttpStatusCode.RedirectMethod, headers, body);

        #endif
    }
}