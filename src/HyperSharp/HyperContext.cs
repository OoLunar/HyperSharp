using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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
        private PipeWriter _responseWriter => Connection.StreamWriter;

        public HyperContext(HttpMethod method, Uri route, Version version, HyperHeaderCollection headers, HyperConnection connection)
        {
            Version = version ?? throw new ArgumentNullException(nameof(version));
            Method = method ?? throw new ArgumentNullException(nameof(method));
            Route = route ?? throw new ArgumentNullException(nameof(route));
            Headers = headers ?? throw new ArgumentNullException(nameof(headers));
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public virtual async Task RespondAsync(HyperStatus status, JsonSerializerOptions? serializerOptions = null)
        {
            // Write request line
            await _responseWriter.WriteAsync(_httpVersions[Version]);
            await _responseWriter.WriteAsync(Encoding.ASCII.GetBytes($"{(int)status.Code} {status.Code}"));
            await _responseWriter.WriteAsync("\r\n"u8.ToArray());

            // Serialize body ahead of time due to headers
            byte[] content = JsonSerializer.SerializeToUtf8Bytes(status.Body, serializerOptions ?? HyperSerializationOptions.Default);

            // Write headers
            status.Headers.SetHeader("Server", _serverName);
            status.Headers.SetHeader("Content-Type", "application/json; charset=utf-8");
            status.Headers.AddHeaderValue("Content-Length", content.Length.ToString());

            foreach (KeyValuePair<string, IReadOnlyList<string>> header in status.Headers)
            {
                await _responseWriter.WriteAsync(Encoding.ASCII.GetBytes(header.Key));
                await _responseWriter.WriteAsync(": "u8.ToArray());
                if (header.Value.Count == 1)
                {
                    await _responseWriter.WriteAsync(Encoding.ASCII.GetBytes(header.Value[0]));
                }
                else
                {
                    foreach (string value in header.Value)
                    {
                        await _responseWriter.WriteAsync(Encoding.ASCII.GetBytes(value));
                        await _responseWriter.WriteAsync(", "u8.ToArray());
                    }
                }

                await _responseWriter.WriteAsync("\r\n"u8.ToArray());
            }
            await _responseWriter.WriteAsync("\r\n"u8.ToArray());

            // Write body
            if (content.Length != 0)
            {
                await _responseWriter.WriteAsync(content);
            }

            await BodyReader.CompleteAsync();
            await _responseWriter.CompleteAsync();
            HasResponded = true;
        }

        public override string ToString() => $"{Method} {Route} HTTP {Version}, {Headers.Count:N0} headers";
    }
}
