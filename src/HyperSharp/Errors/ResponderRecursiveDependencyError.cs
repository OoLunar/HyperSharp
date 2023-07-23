using System;
using FluentResults;

namespace OoLunar.HyperSharp.Errors
{
    public sealed class ResponderRecursiveDependencyError : Error
    {
        public Type ResponderType => (Type)Metadata[nameof(ResponderType)];
        public Type RecursiveDependencyType => (Type)Metadata[nameof(RecursiveDependencyType)];

        public ResponderRecursiveDependencyError(Type responderType, Type recursiveDependencyType)
        {
            Message = $"Recursive dependency: Responder {responderType} depends on {recursiveDependencyType}, which depends on {responderType}.";
            WithMetadata(new()
            {
                [nameof(ResponderType)] = responderType,
                [nameof(RecursiveDependencyType)] = recursiveDependencyType
            });
        }
    }
}
