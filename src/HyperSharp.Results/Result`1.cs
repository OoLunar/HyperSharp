using System.Collections.Generic;
using System.Linq;

namespace OoLunar.HyperSharp.Results
{
    public readonly record struct Result<T>
    {
        public readonly T? Value;
        public readonly IEnumerable<IError> Errors;
        public bool IsSuccess => !Errors.Any();

        internal Result(T? value)
        {
            Value = value;
            Errors = Enumerable.Empty<IError>();
        }

        internal Result(IError error)
        {
            Value = default;
            Errors = Enumerable.Repeat(error, 1);
        }

        internal Result(IEnumerable<IError> errors)
        {
            Value = default;
            Errors = errors;
        }

        internal Result(T? value, IEnumerable<IError> errors)
        {
            Value = value;
            Errors = errors;
        }

        public static implicit operator Result<T>(T value) => new(value);
        public static implicit operator Result(Result<T> value) => new(value.Value, value.Errors);
    }
}
