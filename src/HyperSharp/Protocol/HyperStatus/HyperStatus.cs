using System;
using System.Diagnostics;
using System.Net;

namespace HyperSharp.Protocol
{
    /// <summary>
    /// Represents the status of an HTTP response.
    /// </summary>
    [DebuggerDisplay("{ToString(),nq}")]
    public readonly partial record struct HyperStatus
    {
        /// <summary>
        /// The HTTP status code to respond with.
        /// </summary>
        public HttpStatusCode Code { get; init; }

        /// <summary>
        /// The headers to be included in the response.
        /// </summary>
        public HyperHeaderCollection Headers { get; init; }

        /// <summary>
        /// The body to be serialized into the response.
        /// </summary>
        public object? Body { get; init; }

        /// <summary>
        /// Creates a new <see cref="HyperStatus"/>. The status code will be <see cref="HttpStatusCode.NoContent"/>, the headers will be empty, and the body will be null.
        /// </summary>
        public HyperStatus() : this(HttpStatusCode.NoContent, new HyperHeaderCollection(), null) { }

        /// <summary>
        /// Creates a new <see cref="HyperStatus"/> with the specified status code. The headers will be empty and the body will be null.
        /// </summary>
        /// <param name="code">The HTTP status code to respond with.</param>
        public HyperStatus(HttpStatusCode code) : this(code, new HyperHeaderCollection(), null) { }

        /// <summary>
        /// Creates a new <see cref="HyperStatus"/> with the specified status code and body. The headers will be empty.
        /// </summary>
        /// <param name="code">The HTTP status code to respond with.</param>
        /// <param name="body">The body to be serialized into the response.</param>
        public HyperStatus(HttpStatusCode code, object? body) : this(code, new HyperHeaderCollection(), body) { }

        /// <summary>
        /// Creates a new <see cref="HyperStatus"/> with the specified status code and headers. The body will be null.
        /// </summary>
        /// <param name="code">The HTTP status code to respond with.</param>
        /// <param name="headers">The headers to be included in the response.</param>
        public HyperStatus(HttpStatusCode code, HyperHeaderCollection headers) : this(code, headers, null) { }

        /// <summary>
        /// Creates a new <see cref="HyperStatus"/> with the specified status code, headers, and body.
        /// </summary>
        /// <param name="code">The HTTP status code to respond with.</param>
        /// <param name="headers">The headers to be included in the response.</param>
        /// <param name="body">The body to be serialized into the response.</param>
        public HyperStatus(HttpStatusCode code, HyperHeaderCollection headers, object? body)
        {
            ArgumentNullException.ThrowIfNull(headers, nameof(headers));

            Code = code;
            Headers = headers;
            Body = body;
        }

        /// <inheritdoc />
        public override string ToString() => $"{(int)Code} {Code}, {Headers.Count:N0} header{(Headers.Count == 1 ? "" : "s")}";
    }
}
