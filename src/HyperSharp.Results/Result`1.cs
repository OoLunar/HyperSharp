using System.Collections.Generic;
using System.Linq;

namespace OoLunar.HyperSharp.Results
{
    public readonly record struct Result<T>
    {
        public readonly T? Value;
        public readonly IEnumerable<IError> Errors;
        public bool IsSuccess => !Errors.Any();

        public Result(T? value)
        {
            Value = value;
            Errors = Enumerable.Empty<IError>();
        }

        public Result(IError error)
        {
            Value = default;
            Errors = Enumerable.Repeat(error, 1);
        }

        public Result(IEnumerable<IError> errors)
        {
            Value = default;
            Errors = errors;
        }

        public Result(T? value, IEnumerable<IError> errors)
        {
            Value = value;
            Errors = errors;
        }

        public static Result<T> Success() => new(value: default);
        public static Result<T> Success(T value) => new(value);
        public static Result<T> Failure(IError error) => new(error);
        public static Result<T> Failure(IEnumerable<IError> errors) => new(errors);
        public static Result<T> Failure(T value, IEnumerable<IError> errors) => new(value, errors);

        public static implicit operator Result<T>(T value) => Success(value);
        public static implicit operator Result(Result<T> value) => new(value.Value, value.Errors);
    }
}
