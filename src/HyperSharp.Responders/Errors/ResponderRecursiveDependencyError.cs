using System;
using HyperSharp.Results;

namespace HyperSharp.Responders.Errors
{
    /// <summary>
    /// Represents an error of a responder that causes a recursive dependency.
    /// </summary>
    public sealed record ResponderRecursiveDependencyError : Error
    {
        /// <summary>
        /// The type of the responder that depends on the recursive dependency.
        /// </summary>
        public Type ResponderType { get; init; }

        /// <summary>
        /// The type which depends on the responder.
        /// </summary>
        public Type RecursiveDependencyType { get; init; }

        /// <summary>
        /// Creates a new <see cref="ResponderRecursiveDependencyError"/>.
        /// </summary>
        /// <param name="responderType">The type of the responder that depends on the recursive dependency.</param>
        /// <param name="recursiveDependencyType">The type which depends on the responder.</param>
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
