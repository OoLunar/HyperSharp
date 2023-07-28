using System.Collections.Generic;
using System.Linq;

namespace OoLunar.HyperSharp.Results
{
    public readonly record struct Result
    {
        public readonly object? Value;
        public readonly IEnumerable<IError> Errors;
        public bool IsSuccess => !Errors.Any();

        internal Result(object? value)
        {
            Value = value;
            Errors = Enumerable.Empty<IError>();
        }

        internal Result(IError error)
        {
            Value = null;
            Errors = Enumerable.Repeat(error, 1);
        }

        internal Result(IEnumerable<IError> errors)
        {
            Value = null;
            Errors = errors;
        }

        internal Result(object? value, IEnumerable<IError> errors)
        {
            Value = value;
            Errors = errors;
        }

        public static Result Success() => new(value: null);
        public static Result Success(object? value) => new(value);
        public static Result Failure(string error) => new(new Error(error));
        public static Result Failure(IError error) => new(error);
        public static Result Failure(IEnumerable<IError> errors) => new(errors);
        public static Result Failure(object? value, IEnumerable<IError> errors) => new(value, errors);

        public static Result<T> Success<T>() => new(value: default);
        public static Result<T> Success<T>(T value) => new(value);
        public static Result<T> Failure<T>(string error) => new(new Error(error));
        public static Result<T> Failure<T>(IError error) => new(error);
        public static Result<T> Failure<T>(IEnumerable<IError> errors) => new(errors);
        public static Result<T> Failure<T>(T value, IEnumerable<IError> errors) => new(value, errors);
    }
}
