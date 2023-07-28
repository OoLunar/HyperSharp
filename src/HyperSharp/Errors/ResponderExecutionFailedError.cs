using System;
using System.Collections.Generic;
using OoLunar.HyperSharp.Responders;
using OoLunar.HyperSharp.Results;

namespace OoLunar.HyperSharp.Errors
{
    public sealed record ResponderExecutionFailedError<TInput, TOutput> : Error where TOutput : new()
    {
        public Responder<TInput, TOutput>? ResponderBranch { get; init; }
        public Exception? Exception { get; init; }

        public ResponderExecutionFailedError(IEnumerable<Error> errors) : base() => Errors = errors;
        public ResponderExecutionFailedError(Responder<TInput, TOutput>? responderBranch = null, Exception? exception = null)
        {
            // Sometimes I wish there was a cleaner way to do this.
            // A bunch of if else statements looks ugly.
            Message = exception switch
            {
                // exception is null && responderBranch is null
                null when responderBranch is null => "All responders returned unsuccessful results.",
                // exception is null && responderBranch is not null
                null => $"Responder {responderBranch.Type} returned a failed result.",
                // exception is not null && responderBranch is null
                _ when responderBranch is null => throw new ArgumentException("Responder branch cannot be null when exception is not null."),
                // exception is not null && responderBranch is not null
                _ => $"Responder {responderBranch.Type} failed to execute: {exception.Message}"
            };

            ResponderBranch = responderBranch;
            Exception = exception;
        }
    }
}
