using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using HyperSharp.Results.Json;

namespace HyperSharp.Results
{
    /// <summary>
    /// Represents an expected error that occurred.
    /// </summary>
    [JsonConverter(typeof(ErrorJsonConverter))]
    public record Error
    {
        private static readonly Error[] _empty = Array.Empty<Error>();

        /// <summary>
        /// A summary of the error.
        /// </summary>
        public string Message { get; init; }

        /// <summary>
        /// Other subsequent errors that are related to this error.
        /// </summary>
        public IReadOnlyList<Error> Errors { get; init; }

        /// <summary>
        /// Any possible exception that may have occurred.
        /// </summary>
        public Exception? Exception { get; init; }

        /// <summary>
        /// Creates a new error without a message.
        /// </summary>
        public Error()
        {
            Message = "<No error message provided>";
            Errors = _empty;
        }

        /// <summary>
        /// Creates a new error with a message.
        /// </summary>
        /// <param name="message">A summary of the error.</param>
        public Error(string message)
        {
            Message = message;
            Errors = _empty;
        }

        /// <summary>
        /// Creates a new error with a message and a subsequent error.
        /// </summary>
        /// <param name="message">A summary of the error.</param>
        /// <param name="error">A subsequent error that is related to this error.</param>
        public Error(string message, Error error)
        {
            Message = message;
            Errors = new Error[1] { error };
        }

        /// <summary>
        /// Creates a new error with a message and subsequent errors.
        /// </summary>
        /// <param name="message">A summary of the error.</param>
        /// <param name="errors">Subsequent errors that are related to this error.</param>
        public Error(string message, IEnumerable<Error> errors)
        {
            Message = message;
            Errors = errors.ToArray();
        }

        /// <summary>
        /// Creates a new error from an exception.
        /// </summary>
        /// <param name="exception">The exception that occurred.</param>
        public Error(Exception exception)
        {
            Message = exception.Message;
            Exception = exception;
            Errors = _empty;
        }

        /// <summary>
        /// Creates a new error from an exception and a subsequent error.
        /// </summary>
        /// <param name="exception">The exception that occurred.</param>
        /// <param name="error">A subsequent error that is related to this error.</param>
        public Error(Exception exception, Error error)
        {
            Message = exception.Message;
            Exception = exception;
            Errors = new Error[1] { error };
        }

        /// <summary>
        /// Creates a new error from an exception and subsequent errors.
        /// </summary>
        /// <param name="exception">The exception that occurred.</param>
        /// <param name="errors">Subsequent errors that are related to this error.</param>
        public Error(Exception exception, IEnumerable<Error> errors)
        {
            Message = exception.Message;
            Exception = exception;
            Errors = errors.ToArray();
        }

        /// <summary>
        /// Creates a new error with a message and an exception.
        /// </summary>
        /// <param name="message">A summary of the error.</param>
        /// <param name="exception">The exception that occurred.</param>
        public Error(string message, Exception exception)
        {
            Message = message;
            Exception = exception;
            Errors = _empty;
        }

        /// <summary>
        /// Creates a new error with a message, an exception, and a subsequent error.
        /// </summary>
        /// <param name="message">A summary of the error.</param>
        /// <param name="exception">The exception that occurred.</param>
        /// <param name="error">A subsequent error that is related to this error.</param>
        public Error(string message, Exception exception, Error error)
        {
            Message = message;
            Exception = exception;
            Errors = new Error[1] { error };
        }

        /// <summary>
        /// Creates a new error with a message, an exception, and subsequent errors.
        /// </summary>
        /// <param name="message">A summary of the error.</param>
        /// <param name="exception">The exception that occurred.</param>
        /// <param name="errors">Subsequent errors that are related to this error.</param>
        public Error(string message, Exception exception, IEnumerable<Error> errors)
        {
            Message = message;
            Exception = exception;
            Errors = errors.ToArray();
        }
    }
}
