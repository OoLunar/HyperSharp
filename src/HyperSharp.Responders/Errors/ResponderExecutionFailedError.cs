using System;
using OoLunar.HyperSharp.Results;

namespace OoLunar.HyperSharp.Responders.Errors
{
    public sealed record ResponderExecutionFailedError : Error
    {
        public Exception? Exception { get; init; }
        public ResponderExecutionFailedError(Exception? exception = null) => Exception = exception;
    }
}
