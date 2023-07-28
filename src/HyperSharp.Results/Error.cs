using System.Collections.Generic;
using System.Linq;

namespace OoLunar.HyperSharp.Results
{
    public class Error : IError
    {
        public string Message { get; init; }
        public IEnumerable<IError> Errors { get; init; }

        public Error(string message)
        {
            Message = message;
            Errors = Enumerable.Empty<IError>();
        }

        public Error(string message, IError error)
        {
            Message = message;
            Errors = Enumerable.Repeat(error, 1);
        }

        public Error(string message, IEnumerable<IError> errors)
        {
            Message = message;
            Errors = errors;
        }
    }
}
