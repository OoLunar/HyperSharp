using OoLunar.HyperSharp.Results;

namespace OoLunar.HyperSharp
{
    public sealed record HyperError(string message) : Error(message);
}
