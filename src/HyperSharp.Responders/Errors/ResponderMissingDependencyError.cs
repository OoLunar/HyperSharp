using System;
using HyperSharp.Results;

namespace HyperSharp.Responders.Errors
{
    /// <summary>
    /// Represents an error of a responder that depends on a missing dependency.
    /// </summary>
    public sealed record ResponderMissingDependencyError : Error
    {
        /// <summary>
        /// The type of the responder that depends on the missing dependency.
        /// </summary>
        public Type ResponderType { get; init; }

        /// <summary>
        /// The type of the missing dependency.
        /// </summary>
        public Type MissingDependencyType { get; init; }

        /// <summary>
        /// Creates a new <see cref="ResponderMissingDependencyError"/>.
        /// </summary>
        /// <param name="responderType">The type of the responder that depends on the missing dependency.</param>
        /// <param name="missingDependencyType">The type of the missing dependency.</param>
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
