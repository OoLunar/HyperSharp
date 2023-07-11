using System.Net;

namespace OoLunar.HyperSharp
{
    public readonly record struct HyperStatus(HttpStatusCode Code, HyperHeaderCollection? Headers = null, object? Body = null);
}
