using System;
using FluentResults;

namespace OoLunar.HyperSharp.Errors
{
    public sealed class ResponderInvalidTypeError : Error
    {
        public Type ResponderType => (Type)Metadata[nameof(ResponderType)];
        public Type InvalidResponderType => (Type)Metadata[nameof(InvalidResponderType)];

        public ResponderInvalidTypeError(Type responderType, Type invalidResponderType)
        {
            Message = $"Invalid dependency: Responder {responderType} depends on {invalidResponderType}, which is not a responder.";
            WithMetadata(new()
            {
                [nameof(ResponderType)] = responderType,
                [nameof(InvalidResponderType)] = invalidResponderType
            });
        }
    }
}
