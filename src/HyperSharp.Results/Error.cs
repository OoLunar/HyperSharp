using System.Collections.Generic;
using System.Linq;

namespace OoLunar.HyperSharp.Results
{
    public record Error
    {
        public string Message { get; init; }
        public IEnumerable<Error> Errors { get; init; }

        public Error()
        {
            Message = "<No error message provided>";
            Errors = Enumerable.Empty<Error>();
        }

        public Error(string message)
        {
            Message = message;
            Errors = Enumerable.Empty<Error>();
        }

        public Error(string message, Error error)
        {
            Message = message;
            Errors = Enumerable.Repeat(error, 1);
        }

        public Error(string message, IEnumerable<Error> errors)
        {
            Message = message;
            Errors = errors;
        }
    }
}
