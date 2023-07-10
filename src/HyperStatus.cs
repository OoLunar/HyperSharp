using System.Collections.Generic;
using System.Net;

namespace OoLunar.HyperSharp
{
    public readonly record struct HyperStatus(HttpStatusCode Code, IReadOnlyDictionary<string, IReadOnlyList<string>>? Headers = null, object? Body = null);
}
