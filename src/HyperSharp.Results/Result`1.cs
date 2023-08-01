using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using OoLunar.HyperSharp.Results.Json;

namespace OoLunar.HyperSharp.Results
{
    [JsonConverter(typeof(ResultJsonConverterFactory))]
    public readonly record struct Result<T>
    {
        public readonly T? Value;
        public readonly IEnumerable<Error> Errors;
        public readonly ResultStatus Status;
        public bool IsSuccess => Status.HasFlag(ResultStatus.IsSuccess);
        public bool HasValue => Status.HasFlag(ResultStatus.HasValue);

        public Result()
        {
            Value = default;
            Errors = Enumerable.Empty<Error>();
            Status = ResultStatus.IsSuccess;
        }

        internal Result(T? value)
        {
            Value = value;
            Errors = Enumerable.Empty<Error>();
            Status = ResultStatus.IsSuccess | ResultStatus.HasValue;
        }

        internal Result(Error error)
        {
            Value = default;
            Errors = Enumerable.Repeat(error, 1);
            Status = ResultStatus.None;
        }

        internal Result(IEnumerable<Error> errors)
        {
            Value = default;
            Errors = errors;
            Status = ResultStatus.None;
        }

        internal Result(T? value, IEnumerable<Error> errors)
        {
            Value = value;
            Errors = errors;
            Status = ResultStatus.HasValue;
        }

        public static implicit operator Result<T>(T value) => new(value);
        public static implicit operator Result(Result<T> value) => new(value.Value, value.Errors);
    }
}
