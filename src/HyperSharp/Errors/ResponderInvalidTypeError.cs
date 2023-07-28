using System;
using OoLunar.HyperSharp.Results;

namespace OoLunar.HyperSharp.Errors
{
    public sealed record ResponderInvalidTypeError : Error
    {
        public Type ResponderType { get; init; }
        public Type InvalidResponderType { get; init; }

        public ResponderInvalidTypeError(Type responderType, Type invalidResponderType)
        {
            ArgumentNullException.ThrowIfNull(responderType, nameof(responderType));
            ArgumentNullException.ThrowIfNull(invalidResponderType, nameof(invalidResponderType));

            Message = $"Invalid dependency: Responder {responderType} depends on {invalidResponderType}, which is not a responder.";
            ResponderType = responderType;
            InvalidResponderType = invalidResponderType;
        }
    }
}
