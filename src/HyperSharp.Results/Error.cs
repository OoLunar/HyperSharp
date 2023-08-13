using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using HyperSharp.Results.Json;

namespace HyperSharp.Results
{
    [JsonConverter(typeof(ErrorJsonConverter))]
    public record Error
    {
        private static readonly Error[] _empty = Array.Empty<Error>();

        public string Message { get; init; }
        public IReadOnlyList<Error> Errors { get; init; }
        public Exception? Exception { get; init; }

        public Error()
        {
            Message = "<No error message provided>";
            Errors = _empty;
        }

        public Error(string message)
        {
            Message = message;
            Errors = _empty;
        }

        public Error(string message, Error error)
        {
            Message = message;
            Errors = new Error[1] { error };
        }

        public Error(string message, IEnumerable<Error> errors)
        {
            Message = message;
            Errors = errors.ToArray();
        }

        public Error(Exception exception)
        {
            Message = exception.Message;
            Exception = exception;
            Errors = _empty;
        }

        public Error(Exception exception, Error error)
        {
            Message = exception.Message;
            Exception = exception;
            Errors = new Error[1] { error };
        }

        public Error(Exception exception, IEnumerable<Error> errors)
        {
            Message = exception.Message;
            Exception = exception;
            Errors = errors.ToArray();
        }

        public Error(string message, Exception exception)
        {
            Message = message;
            Exception = exception;
            Errors = _empty;
        }

        public Error(string message, Exception exception, Error error)
        {
            Message = message;
            Exception = exception;
            Errors = new Error[1] { error };
        }

        public Error(string message, Exception exception, IEnumerable<Error> errors)
        {
            Message = message;
            Exception = exception;
            Errors = errors.ToArray();
        }
    }
}
