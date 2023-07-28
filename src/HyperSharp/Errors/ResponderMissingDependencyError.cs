using System;
using OoLunar.HyperSharp.Results;

namespace OoLunar.HyperSharp.Errors
{
    public sealed record ResponderMissingDependencyError : Error
    {
        public Type ResponderType { get; init; }
        public Type MissingDependencyType { get; init; }

        public ResponderMissingDependencyError(Type responderType, Type missingDependencyType)
        {
            ArgumentNullException.ThrowIfNull(responderType, nameof(responderType));
            ArgumentNullException.ThrowIfNull(missingDependencyType, nameof(missingDependencyType));

            Message = $"Missing dependency: Responder {responderType} depends on {missingDependencyType}, which is not registered.";
            ResponderType = responderType;
            MissingDependencyType = missingDependencyType;
        }
    }
}
