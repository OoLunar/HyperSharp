using System.Collections.Generic;
using System.Linq;

namespace OoLunar.HyperSharp.Results
{
    public readonly record struct Result
    {
        public readonly object? Value;
        public readonly IEnumerable<IError> Errors;
        public bool IsSuccess => !Errors.Any();

        public Result(object? value)
        {
            Value = value;
            Errors = Enumerable.Empty<IError>();
        }

        public Result(IError error)
        {
            Value = null;
            Errors = Enumerable.Repeat(error, 1);
        }

        public Result(IEnumerable<IError> errors)
        {
            Value = null;
            Errors = errors;
        }

        public Result(object? value, IEnumerable<IError> errors)
        {
            Value = value;
            Errors = errors;
        }

        public static Result Success() => new(value: null);
        public static Result Success(object? value) => new(value);
        public static Result Failure(IError error) => new(error);
        public static Result Failure(IEnumerable<IError> errors) => new(errors);
        public static Result Failure(object? value, IEnumerable<IError> errors) => new(value, errors);
    }
}
