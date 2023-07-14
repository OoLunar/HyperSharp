using System.Net;
using OoLunar.HyperSharp.Parsing;

namespace OoLunar.HyperSharp
{
    public readonly record struct HyperStatus(HttpStatusCode Code, HyperHeaderCollection? Headers = null, object? Body = null);
}
