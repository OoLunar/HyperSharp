using HyperSharp.Results;

namespace HyperSharp
{
    /// <summary>
    /// Represents an error that occurred during the handling of a request.
    /// </summary>
    /// <param name="message">The error message.</param>
    public sealed record HyperError(string message) : Error(message);
}
