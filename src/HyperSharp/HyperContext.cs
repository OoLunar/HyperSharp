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
using OoLunar.HyperSharp.Json;
using OoLunar.HyperSharp.Parsing;

namespace OoLunar.HyperSharp
{
    [DebuggerDisplay("{ToString(),nq}")]
    public class HyperContext
    {
        private static readonly string _serverName;
        private static readonly FrozenDictionary<Version, byte[]> _httpVersions;

        static HyperContext()
        {
            _serverName = "HyperSharp";
            _httpVersions = new Dictionary<Version, byte[]>()
            {
                [HttpVersion.Version10] = "HTTP/1.0 "u8.ToArray(),
                [HttpVersion.Version11] = "HTTP/1.1 "u8.ToArray(),
            }.ToFrozenDictionary();
        }

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
            Version = version ?? throw new ArgumentNullException(nameof(version));
            Method = method ?? throw new ArgumentNullException(nameof(method));
            Route = route ?? throw new ArgumentNullException(nameof(route));
            Headers = headers ?? throw new ArgumentNullException(nameof(headers));
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public virtual async Task RespondAsync(HyperStatus status, JsonSerializerOptions? serializerOptions = null, CancellationToken cancellationToken = default)
        {
            // Grab the base network stream to write our ASCII headers to.
            // TODO: Find a solution which allows modification of the body (Gzip) and the base stream (SSL).
            PipeWriter baseStreamWriter = PipeWriter.Create(Connection.Client.GetStream(), new StreamPipeWriterOptions(leaveOpen: true));

            // Write request line
            await baseStreamWriter.WriteAsync(_httpVersions[Version], cancellationToken);
            await baseStreamWriter.WriteAsync(Encoding.ASCII.GetBytes($"{(int)status.Code} {status.Code}"), cancellationToken);
            await baseStreamWriter.WriteAsync("\r\n"u8.ToArray(), cancellationToken);

            // Serialize body ahead of time due to headers
            byte[] content = JsonSerializer.SerializeToUtf8Bytes(status.Body, serializerOptions ?? HyperSerializationOptions.Default);

            // Write headers
            status.Headers.SetHeader("Server", _serverName);
            status.Headers.SetHeader("Content-Length", content.Length.ToString());
            status.Headers.SetHeader("Content-Type", "application/json; charset=utf-8");

            foreach (KeyValuePair<string, IReadOnlyList<string>> header in status.Headers)
            {
                await baseStreamWriter.WriteAsync(Encoding.ASCII.GetBytes(header.Key), cancellationToken);
                await baseStreamWriter.WriteAsync(": "u8.ToArray(), cancellationToken);
                if (header.Value.Count == 1)
                {
                    await baseStreamWriter.WriteAsync(Encoding.ASCII.GetBytes(header.Value[0]), cancellationToken);
                }
                else
                {
                    foreach (string value in header.Value)
                    {
                        await baseStreamWriter.WriteAsync(Encoding.ASCII.GetBytes(value), cancellationToken);
                        await baseStreamWriter.WriteAsync(", "u8.ToArray(), cancellationToken);
                    }
                }

                await baseStreamWriter.WriteAsync("\r\n"u8.ToArray(), cancellationToken);
            }
            await baseStreamWriter.WriteAsync("\r\n"u8.ToArray(), cancellationToken);
            await baseStreamWriter.CompleteAsync();

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
