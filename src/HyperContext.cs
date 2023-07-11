using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
        private PipeWriter Response { get; init; }
        public bool HasResponded { get; private set; }

        public HyperContext(HttpMethod method, Uri route, Version version, HyperHeaderCollection headers, PipeReader body, PipeWriter response)
        {
            Version = version ?? throw new ArgumentNullException(nameof(version));
            Method = method ?? throw new ArgumentNullException(nameof(method));
            Route = route ?? throw new ArgumentNullException(nameof(route));
            Headers = headers ?? throw new ArgumentNullException(nameof(headers));
            Body = body ?? throw new ArgumentNullException(nameof(body));
            Response = response ?? throw new ArgumentNullException(nameof(response));
        }

        public async Task RespondAsync(HyperStatus status)
        {
            // Write request line
            await Response.WriteAsync(HttpVersions[Version]);
            await Response.WriteAsync(Encoding.ASCII.GetBytes($"{(int)status.Code} {status.Code}"));
            await Response.WriteAsync("\r\n"u8.ToArray());

            // Write headers
            if (status.Headers is not null)
            {
                foreach (KeyValuePair<string, IReadOnlyList<string>> header in status.Headers)
                {
                    await Response.WriteAsync(Encoding.ASCII.GetBytes(header.Key));
                    await Response.WriteAsync(": "u8.ToArray());
                    if (header.Value.Count == 1)
                    {
                        await Response.WriteAsync(Encoding.ASCII.GetBytes(header.Value[0]));
                    }
                    else
                    {
                        foreach (string value in header.Value)
                        {
                            await Response.WriteAsync(Encoding.ASCII.GetBytes(value));
                            await Response.WriteAsync(", "u8.ToArray());
                        }
                    }

                    await Response.WriteAsync("\r\n"u8.ToArray());
                }
            }
            await Response.WriteAsync("\r\n"u8.ToArray());

            // Write body
            if (status.Body is not null)
            {
                await JsonSerializer.SerializeAsync(Response.AsStream(), status.Body, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            }

            await Response.CompleteAsync();
            HasResponded = true;
        }
    }
}
