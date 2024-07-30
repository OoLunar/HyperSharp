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
        public required HttpStatusCode Code { get; init; }

        /// <summary>
        /// The headers to be included in the response.
        /// </summary>
        public HyperHeaderCollection Headers { get; init; }

        /// <summary>
        /// The body to be serialized into the response.
        /// </summary>
        public object? Body { get; init; }

        /// <summary>
        /// Which serializer to use to serialize the body.
        /// </summary>
        public HyperSerializerDelegate Serializer { get; init; }

        /// <summary>
        /// Creates a new <see cref="HyperStatus"/> with the specified status code.
        /// </summary>
        public HyperStatus()
        {
            Headers = [];
            Serializer = HyperSerializers.JsonAsync;
        }

        /// <inheritdoc />
        public override string ToString() => $"{(int)Code} {Code}, {Headers?.Count ?? 0:N0} header{(Headers?.Count == 1 ? "" : "s")}";
    }
}
