using HyperSharp.Results;

namespace HyperSharp
{
    public sealed record HyperError(string message) : Error(message);
}
