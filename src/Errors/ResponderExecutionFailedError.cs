using System;
using FluentResults;
using OoLunar.HyperSharp.Responders;

namespace OoLunar.HyperSharp.Errors
{
    public sealed class ResponderExecutionFailedError : Error
    {
        public Twig? ResponderBranch => Metadata.TryGetValue(nameof(ResponderBranch), out object? branch) ? branch as Twig : null;
        public Exception? Exception => Metadata.TryGetValue(nameof(Exception), out object? exception) ? exception as Exception : null;

        public ResponderExecutionFailedError(Twig? responderBranch = null, Exception? exception = null)
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

            WithMetadata(new()
            {
                [nameof(ResponderBranch)] = responderBranch,
                [nameof(Exception)] = exception
            });
        }
    }
}
