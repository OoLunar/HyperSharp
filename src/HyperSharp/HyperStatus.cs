using System;
using System.Diagnostics;
using System.Net;
using OoLunar.HyperSharp.Parsing;

namespace OoLunar.HyperSharp
{
    [DebuggerDisplay("{ToString(),nq}")]
    public record HyperStatus
    {
        public HttpStatusCode Code { get; init; }
        public HyperHeaderCollection Headers { get; init; }
        public object? Body { get; init; }

        public HyperStatus(HttpStatusCode code) : this(code, new HyperHeaderCollection(), null) { }
        public HyperStatus(HttpStatusCode code, HyperHeaderCollection headers) : this(code, headers, null) { }
        public HyperStatus(HttpStatusCode code, object? body) : this(code, new HyperHeaderCollection(), body) { }
        public HyperStatus(HttpStatusCode code, HyperHeaderCollection headers, object? body)
        {
            ArgumentNullException.ThrowIfNull(headers, nameof(headers));

            Code = code;
            Headers = headers;
            Body = body;
        }

        public override string ToString() => $"{(int)Code} {Code}, {Headers.Count:N0} headers";
    }
}
