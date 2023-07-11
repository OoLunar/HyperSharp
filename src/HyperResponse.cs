namespace OoLunar.HyperSharp
{
    public sealed class HyperResponse
    {
        public int StatusCode { get; init; }
        public HyperHeaderCollection Headers { get; init; }
        public object? Body { get; init; }

        public HyperResponse(int statusCode, HyperHeaderCollection headers, object? body)
        {
            StatusCode = statusCode;
            Headers = headers;
            Body = body;
        }
    }
}
