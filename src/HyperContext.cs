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
using OoLunar.HyperSharp.Parsing;

namespace OoLunar.HyperSharp
{
    public sealed class HyperContext
    {
        [DebuggerDisplay("{ToString(),nq}")]
        private static readonly FrozenDictionary<Version, byte[]> HttpVersions = new Dictionary<Version, byte[]>()
        {
            [HttpVersion.Version10] = "HTTP/1.0 "u8.ToArray(),
            [HttpVersion.Version11] = "HTTP/1.1 "u8.ToArray(),
        }.ToFrozenDictionary();

        public HttpMethod Method { get; init; }
        public Uri Route { get; init; }
        public Version Version { get; init; }
        public HyperHeaderCollection Headers { get; init; }
        public PipeReader BodyReader { get; init; }

        public Dictionary<string, string> Metadata { get; init; } = new();
        public bool HasResponded { get; private set; }

        private PipeWriter ResponseWriter { get; init; }

        public HyperContext(HttpMethod method, Uri route, Version version, HyperHeaderCollection headers, PipeReader bodyReader, PipeWriter responseWriter)
        {
            Version = version ?? throw new ArgumentNullException(nameof(version));
            Method = method ?? throw new ArgumentNullException(nameof(method));
            Route = route ?? throw new ArgumentNullException(nameof(route));
            Headers = headers ?? throw new ArgumentNullException(nameof(headers));
            BodyReader = bodyReader ?? throw new ArgumentNullException(nameof(bodyReader));
            ResponseWriter = responseWriter ?? throw new ArgumentNullException(nameof(responseWriter));
        }

        public async Task RespondAsync(HyperStatus status, JsonSerializerOptions? serializerOptions = null)
        {
            // Write request line
            await ResponseWriter.WriteAsync(HttpVersions[Version]);
            await ResponseWriter.WriteAsync(Encoding.ASCII.GetBytes($"{(int)status.Code} {status.Code}"));
            await ResponseWriter.WriteAsync("\r\n"u8.ToArray());

            // Write headers
            if (status.Headers is not null)
            {
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
            }
            await ResponseWriter.WriteAsync("\r\n"u8.ToArray());

            // Write body
            if (status.Body is not null)
            {
                await JsonSerializer.SerializeAsync(ResponseWriter.AsStream(true), status.Body, serializerOptions ?? new JsonSerializerOptions(JsonSerializerDefaults.Web));
            }

            await ResponseWriter.CompleteAsync();
            HasResponded = true;
        }

        public override string ToString() => $"{Method} {Route} HTTP {Version}, {Headers.Count:N0} headers";
    }
}
