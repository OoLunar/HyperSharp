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
        private static readonly string ServerName;
        private static readonly FrozenDictionary<Version, byte[]> HttpVersions;

        static HyperContext()
        {
            ServerName = "HyperSharp";
            HttpVersions = new Dictionary<Version, byte[]>()
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

        public PipeReader BodyReader => Connection.StreamReader;
        private PipeWriter ResponseWriter => Connection.StreamWriter;
        public bool HasResponded { get; private set; }

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
            await ResponseWriter.WriteAsync(HttpVersions[Version]);
            await ResponseWriter.WriteAsync(Encoding.ASCII.GetBytes($"{(int)status.Code} {status.Code}"));
            await ResponseWriter.WriteAsync("\r\n"u8.ToArray());

            // Serialize body ahead of time due to headers
            byte[] content = JsonSerializer.SerializeToUtf8Bytes(status.Body, serializerOptions ?? HyperSerializationOptions.Default);

            // Write headers
            status.Headers.SetHeader("Server", ServerName);
            status.Headers.SetHeader("Content-Type", "application/json; charset=utf-8");
            status.Headers.AddHeaderValue("Content-Length", content.Length.ToString());

            foreach (KeyValuePair<string, IReadOnlyList<string>> header in status.Headers)
            {
                await ResponseWriter.WriteAsync(Encoding.ASCII.GetBytes(header.Key));
                await ResponseWriter.WriteAsync(": "u8.ToArray());
                if (header.Value.Count == 1)
                {
                    await ResponseWriter.WriteAsync(Encoding.ASCII.GetBytes(header.Value[0]));
                }
                else
                {
                    foreach (string value in header.Value)
                    {
                        await ResponseWriter.WriteAsync(Encoding.ASCII.GetBytes(value));
                        await ResponseWriter.WriteAsync(", "u8.ToArray());
                    }
                }

                await ResponseWriter.WriteAsync("\r\n"u8.ToArray());
            }
            await ResponseWriter.WriteAsync("\r\n"u8.ToArray());

            // Write body
            if (content.Length != 0)
            {
                await ResponseWriter.WriteAsync(content);
            }

            await BodyReader.CompleteAsync();
            await ResponseWriter.CompleteAsync();
            HasResponded = true;
        }

        public override string ToString() => $"{Method} {Route} HTTP {Version}, {Headers.Count:N0} headers";
    }
}
