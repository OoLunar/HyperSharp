using System;
using HyperSharp.Results;

namespace HyperSharp.Responders.Errors
{
    /// <summary>
    /// Represents an error of a responder that depends on an invalid responder.
    /// </summary>
    public sealed record ResponderInvalidTypeError : Error
    {
        /// <summary>
        /// The type of the responder that depends on the invalid responder.
        /// </summary>
        public Type ResponderType { get; init; }

        /// <summary>
        /// The type of the invalid responder.
        /// </summary>
        public Type InvalidResponderDependency { get; init; }

        /// <summary>
        /// Creates a new <see cref="ResponderInvalidTypeError"/>.
        /// </summary>
        /// <param name="responderType">The type of the responder that depends on the invalid responder.</param>
        /// <param name="invalidResponderDependency">The type of the invalid responder.</param>
        public ResponderInvalidTypeError(Type responderType, Type invalidResponderDependency)
        {
            ArgumentNullException.ThrowIfNull(responderType, nameof(responderType));
            ArgumentNullException.ThrowIfNull(invalidResponderDependency, nameof(invalidResponderDependency));

            Message = $"Invalid dependency: Responder {responderType} depends on {invalidResponderDependency}, which is not a responder.";
            ResponderType = responderType;
            InvalidResponderDependency = invalidResponderDependency;
        }
    }
}
