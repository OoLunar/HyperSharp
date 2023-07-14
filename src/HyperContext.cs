using System;
using System.Collections.Frozen;
using System.Collections.Generic;
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
        private static readonly FrozenDictionary<Version, byte[]> HttpVersions = new Dictionary<Version, byte[]>()
        {
            [HttpVersion.Version10] = "HTTP/1.0 "u8.ToArray(),
            [HttpVersion.Version11] = "HTTP/1.1 "u8.ToArray(),
        }.ToFrozenDictionary();

        public HttpMethod Method { get; init; }
        public Uri Route { get; init; }
        public Version Version { get; init; }
        public HyperHeaderCollection Headers { get; init; }
        public PipeReader Body { get; init; }

        public Dictionary<string, string> Metadata { get; init; } = new();
        public bool HasResponded { get; private set; }

        private PipeWriter Responder { get; init; }

        public HyperContext(HttpMethod method, Uri route, Version version, HyperHeaderCollection headers, PipeReader body, PipeWriter responder)
        {
            Version = version ?? throw new ArgumentNullException(nameof(version));
            Method = method ?? throw new ArgumentNullException(nameof(method));
            Route = route ?? throw new ArgumentNullException(nameof(route));
            Headers = headers ?? throw new ArgumentNullException(nameof(headers));
            Body = body ?? throw new ArgumentNullException(nameof(body));
            Responder = responder ?? throw new ArgumentNullException(nameof(responder));
        }

        public async Task RespondAsync(HyperStatus status, JsonSerializerOptions? serializerOptions = null)
        {
            // Write request line
            await Responder.WriteAsync(HttpVersions[Version]);
            await Responder.WriteAsync(Encoding.ASCII.GetBytes($"{(int)status.Code} {status.Code}"));
            await Responder.WriteAsync("\r\n"u8.ToArray());

            // Write headers
            if (status.Headers is not null)
            {
                foreach (KeyValuePair<string, IReadOnlyList<string>> header in status.Headers)
                {
                    await Responder.WriteAsync(Encoding.ASCII.GetBytes(header.Key));
                    await Responder.WriteAsync(": "u8.ToArray());
                    if (header.Value.Count == 1)
                    {
                        await Responder.WriteAsync(Encoding.ASCII.GetBytes(header.Value[0]));
                    }
                    else
                    {
                        foreach (string value in header.Value)
                        {
                            await Responder.WriteAsync(Encoding.ASCII.GetBytes(value));
                            await Responder.WriteAsync(", "u8.ToArray());
                        }
                    }

                    await Responder.WriteAsync("\r\n"u8.ToArray());
                }
            }
            await Responder.WriteAsync("\r\n"u8.ToArray());

            // Write body
            if (status.Body is not null)
            {
                await JsonSerializer.SerializeAsync(Responder.AsStream(), status.Body, serializerOptions ?? new JsonSerializerOptions(JsonSerializerDefaults.Web));
            }

            await Responder.CompleteAsync();
            HasResponded = true;
        }
    }
}
