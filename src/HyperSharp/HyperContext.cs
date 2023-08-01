using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OoLunar.HyperSharp.Parsing;

namespace OoLunar.HyperSharp
{
    [DebuggerDisplay("{ToString(),nq}")]
    public class HyperContext
    {
        private static readonly FrozenDictionary<Version, byte[]> _httpVersions = new Dictionary<Version, byte[]>()
        {
            [HttpVersion.Version10] = "HTTP/1.0 "u8.ToArray(),
            [HttpVersion.Version11] = "HTTP/1.1 "u8.ToArray(),
        }.ToFrozenDictionary();

        public HttpMethod Method { get; init; }
        public Uri Route { get; init; }
        public Version Version { get; init; }
        public HyperHeaderCollection Headers { get; init; }
        public HyperConnection Connection { get; init; }
        public Dictionary<string, string> Metadata { get; init; } = new();

        public bool HasResponded { get; private set; }
        public PipeReader BodyReader => Connection.StreamReader;

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
            Route = headers.TryGetValue("Host", out IReadOnlyList<string>? host)
                ? new Uri($"http://{host[0]}/{route.OriginalString}")
                : new Uri(connection.Server.Configuration.Host, route);
        }

        public virtual async Task RespondAsync(HyperStatus status, JsonSerializerOptions? serializerOptions = null, CancellationToken cancellationToken = default)
        {
            // Grab the base network stream to write our ASCII headers to.
            // TODO: Find a solution which allows modification of the body (Gzip) and the base stream (SSL).

            // Write request line
            await Connection.StreamWriter.WriteAsync(_httpVersions[Version], cancellationToken);
            await Connection.StreamWriter.WriteAsync(Encoding.ASCII.GetBytes($"{(int)status.Code} {status.Code}"), cancellationToken);
            await Connection.StreamWriter.WriteAsync("\r\n"u8.ToArray(), cancellationToken);

            // Serialize body ahead of time due to headers
            byte[] content = JsonSerializer.SerializeToUtf8Bytes(status.Body, serializerOptions ?? HyperJsonSerializationOptions.Default);

            // Write headers
            status.Headers.TryAdd("Date", DateTime.UtcNow.ToString("R"));
            status.Headers.TryAdd("Content-Length", content.Length.ToString());
            status.Headers.TryAdd("Content-Type", "application/json; charset=utf-8");
            status.Headers.TryAdd("Server", Connection.Server.Configuration.ServerName);

            foreach (string headerName in status.Headers.Keys)
            {
                await Connection.StreamWriter.WriteAsync(Encoding.ASCII.GetBytes(headerName), cancellationToken);
                await Connection.StreamWriter.WriteAsync(": "u8.ToArray(), cancellationToken);

                if (!status.Headers.TryGetValue(headerName, out IReadOnlyList<byte[]>? headerValues))
                {
                    // This shouldn't be able to happen, but just in case.
                    await Connection.StreamWriter.WriteAsync("\r\n"u8.ToArray(), cancellationToken);
                    continue;
                }

                if (headerValues.Count == 1)
                {
                    await Connection.StreamWriter.WriteAsync(headerValues[0], cancellationToken);
                }
                else
                {
                    foreach (byte[] value in headerValues)
                    {
                        await Connection.StreamWriter.WriteAsync(value, cancellationToken);
                        await Connection.StreamWriter.WriteAsync(", "u8.ToArray(), cancellationToken);
                    }
                }

                await Connection.StreamWriter.WriteAsync("\r\n"u8.ToArray(), cancellationToken);
            }
            await Connection.StreamWriter.WriteAsync("\r\n"u8.ToArray(), cancellationToken);

            // Write body
            if (content.Length != 0)
            {
                await Connection.StreamWriter.WriteAsync(content, cancellationToken);
            }

            await BodyReader.CompleteAsync();
            await Connection.StreamWriter.CompleteAsync();
            HasResponded = true;
        }

        public override string ToString() => $"{Method} {Route} HTTP {Version}, {Headers.Count:N0} header{(Headers.Count == 1 ? "" : "s")}, {Metadata.Count:N0} metadata item{(Metadata.Count == 1 ? "" : "s")}";
    }
}
