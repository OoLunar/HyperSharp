using System;
using FluentResults;

namespace OoLunar.HyperSharp.Errors
{
    public sealed class ResponderMissingDependencyError : Error
    {
        public Type ResponderType => (Type)Metadata[nameof(ResponderType)];
        public Type MissingDependencyType => (Type)Metadata[nameof(MissingDependencyType)];

        public ResponderMissingDependencyError(Type responderType, Type missingDependencyType)
        {
            Message = $"Missing dependency: Responder {responderType} depends on {missingDependencyType}, which is not registered.";
            WithMetadata(new()
            {
                [nameof(ResponderType)] = responderType,
                [nameof(MissingDependencyType)] = missingDependencyType
            });
        }
    }
}
