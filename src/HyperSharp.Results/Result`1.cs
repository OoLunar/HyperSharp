using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using HyperSharp.Results.Json;

namespace HyperSharp.Results
{
    [JsonConverter(typeof(ResultJsonConverterFactory))]
    public readonly record struct Result<T>
    {
        public readonly T? Value;
        public readonly IReadOnlyList<Error> Errors;
        public readonly ResultStatus Status;
        public bool IsSuccess => Status.HasFlag(ResultStatus.IsSuccess);
        public bool HasValue => Status.HasFlag(ResultStatus.HasValue);

        public Result()
        {
            Value = default;
            Errors = Result._emptyErrors;
            Status = ResultStatus.IsSuccess;
        }

        internal Result(T? value)
        {
            Value = value;
            Errors = Result._emptyErrors;
            Status = ResultStatus.IsSuccess | ResultStatus.HasValue;
        }

        internal Result(Error error)
        {
            Value = default;
            Errors = new[] { error };
            Status = ResultStatus.None;
        }

        internal Result(IEnumerable<Error> errors)
        {
            Value = default;
            Errors = errors.ToArray();
            Status = ResultStatus.None;
        }

        internal Result(T? value, Error error)
        {
            Value = value;
            Errors = new[] { error };
            Status = ResultStatus.HasValue;
        }

        internal Result(T? value, IEnumerable<Error> errors)
        {
            Value = value;
            Errors = errors.ToArray();
            Status = ResultStatus.HasValue;
        }

        public static implicit operator Result<T>(T value) => new(value);
        public static implicit operator Result(Result<T> value) => new(value.Value, value.Errors);
    }
}
