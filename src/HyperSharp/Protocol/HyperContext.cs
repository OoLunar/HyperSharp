using System;
#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#else
using System.Collections.Immutable;
#endif
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.Toolkit.HighPerformance;

namespace HyperSharp.Protocol
{
    /// <summary>
    /// Represents the context of an HTTP request.
    /// </summary>
    [DebuggerDisplay("{ToString(),nq}")]
    public class HyperContext
    {
        /// <summary>
        /// The currently supported HTTP versions.
        /// </summary>
        private static readonly IReadOnlyDictionary<Version, byte[]> _httpVersions = new Dictionary<Version, byte[]>()
        {
            [HttpVersion.Version10] = "HTTP/1.0 "u8.ToArray(),
            [HttpVersion.Version11] = "HTTP/1.1 "u8.ToArray(),
        }
#if NET8_0_OR_GREATER
        .ToFrozenDictionary();
#else
        .ToImmutableDictionary();
#endif
        private static readonly byte[] _newLine = "\r\n"u8.ToArray();
        private static readonly byte[] _emptyBody = Array.Empty<byte>();

        /// <summary>
        /// The HTTP method of the request.
        /// </summary>
        public HttpMethod Method { get; init; }

        /// <summary>
        /// The requested URI of the HTTP request.
        /// </summary>
        public Uri Route { get; init; }

        /// <summary>
        /// The HTTP version of the request.
        /// </summary>
        public Version Version { get; init; }

        /// <summary>
        /// The client headers of the request.
        /// </summary>
        public HyperHeaderCollection Headers { get; init; }

        /// <summary>
        /// The currently opened connection to the client.
        /// </summary>
        public HyperConnection Connection { get; init; }

        /// <summary>
        /// Any metadata associated with the request, explicitly set by the registered responders.
        /// </summary>
        public Dictionary<string, string> Metadata { get; init; } = new();

        /// <summary>
        /// Whether or not the request has been responded to.
        /// </summary>
        public bool HasResponded { get; private set; }

        /// <summary>
        /// The body of the request.
        /// </summary>
        public PipeReader BodyReader => Connection.StreamReader;

        /// <summary>
        /// Creates a new <see cref="HyperContext"/> with the specified parameters.
        /// </summary>
        /// <param name="method">The HTTP method of the request.</param>
        /// <param name="route">The requested URI of the HTTP request.</param>
        /// <param name="version">The HTTP version of the request.</param>
        /// <param name="headers">The client headers of the request.</param>
        /// <param name="connection">The currently opened connection to the client.</param>
        public HyperContext(HttpMethod method, Uri route, Version version, HyperHeaderCollection headers, HyperConnection connection)
        {
            ArgumentNullException.ThrowIfNull(method);
            ArgumentNullException.ThrowIfNull(route);
            ArgumentNullException.ThrowIfNull(version);
            ArgumentNullException.ThrowIfNull(headers);
            ArgumentNullException.ThrowIfNull(connection);

            Version = version;
            Method = method;
            Headers = headers;
            Connection = connection;
            Route = headers.TryGetValue("Host", out string? host)
                ? new Uri($"http://{host}{route.OriginalString}")
                : new Uri(connection.Server.Configuration._host, route);
        }

        /// <summary>
        /// Responds to the request with the specified status, and serializes the body using the specified <see cref="JsonSerializerOptions"/>.
        /// </summary>
        /// <param name="status">The status to respond with.</param>
        /// <param name="serializerOptions">The <see cref="JsonSerializerOptions"/> to use when serializing the body.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use when writing the response.</param>
        public virtual async Task RespondAsync(HyperStatus status, JsonSerializerOptions? serializerOptions = null, CancellationToken cancellationToken = default)
        {
            // Grab the base network stream to write our ASCII headers to.
            // TODO: Find a solution which allows modification of the body (Gzip) and the base stream (SSL).

            // Write request line
            Connection.StreamWriter.Write<byte>(_httpVersions[Version]);
            Connection.StreamWriter.Write<byte>(Encoding.ASCII.GetBytes($"{(int)status.Code} {status.Code}"));
            Connection.StreamWriter.Write<byte>(_newLine);

            // Serialize body ahead of time due to headers
            byte[] content = status.Body is null
                ? _emptyBody
                : JsonSerializer.SerializeToUtf8Bytes(status.Body, serializerOptions ?? Connection.Server.Configuration.JsonSerializerOptions);

            // Write headers
            status.Headers.TryAdd("Date", DateTime.UtcNow.ToString("R"));
            status.Headers.TryAdd("Content-Length", content.Length.ToString(CultureInfo.InvariantCulture));
            status.Headers.UnsafeTryAdd("Content-Type", "application/json; charset=utf-8"u8.ToArray());
            status.Headers.TryAdd("Server", Connection.Server.Configuration.ServerName);

            foreach ((string headerName, byte[] value) in status.Headers)
            {
                Connection.StreamWriter.Write<byte>(Encoding.ASCII.GetBytes(headerName));
                Connection.StreamWriter.Write<byte>(": "u8.ToArray());
                Connection.StreamWriter.Write<byte>(value);
                Connection.StreamWriter.Write<byte>(_newLine);
            }
            Connection.StreamWriter.Write<byte>(_newLine);

            // Write body
            if (content.Length != 0)
            {
                Connection.StreamWriter.Write<byte>(content);
            }

            HasResponded = true;
            await Connection.StreamWriter.FlushAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public override string ToString() => $"{Method} {Route} HTTP {Version}, {Headers.Count:N0} header{(Headers.Count == 1 ? "" : "s")}, {Metadata.Count:N0} metadata item{(Metadata.Count == 1 ? "" : "s")}";
    }
}
