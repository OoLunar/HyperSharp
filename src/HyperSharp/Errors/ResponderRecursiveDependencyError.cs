using System;
using OoLunar.HyperSharp.Results;

namespace OoLunar.HyperSharp.Errors
{
    public sealed record ResponderRecursiveDependencyError : Error
    {
        public Type ResponderType { get; init; }
        public Type RecursiveDependencyType { get; init; }

        public ResponderRecursiveDependencyError(Type responderType, Type recursiveDependencyType)
        {
            ArgumentNullException.ThrowIfNull(responderType, nameof(responderType));
            ArgumentNullException.ThrowIfNull(recursiveDependencyType, nameof(recursiveDependencyType));

            Message = $"Recursive dependency: Responder {responderType} depends on {recursiveDependencyType}, which depends on {responderType}.";
            ResponderType = responderType;
            RecursiveDependencyType = recursiveDependencyType;
        }
    }
}
