using System.Collections.Generic;

namespace OoLunar.HyperSharp
{
    public sealed class HyperResponse
    {
        public int StatusCode { get; init; }
        public IReadOnlyDictionary<string, IReadOnlyList<string>> Headers { get; init; }
        public object? Body { get; init; }

        public HyperResponse(int statusCode, IReadOnlyDictionary<string, IReadOnlyList<string>> headers, object? body)
        {
            StatusCode = statusCode;
            Headers = headers;
            Body = body;
        }
    }
}
