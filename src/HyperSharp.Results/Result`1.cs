using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using HyperSharp.Results.Json;

namespace HyperSharp.Results
{
    /// <summary>
    /// Represents a result that may or may not have a value.
    /// </summary>
    [JsonConverter(typeof(ResultJsonConverterFactory))]
    public readonly record struct Result<T>
    {
        /// <summary>
        /// The value of the result. Check <see cref="HasValue"/> before accessing.
        /// </summary>
        public readonly T? Value;

        /// <summary>
        /// The errors of the result.
        /// </summary>
        public readonly IReadOnlyList<Error> Errors;

        /// <summary>
        /// The status of the result.
        /// </summary>
        public readonly ResultStatus Status;

        /// <summary>
        /// Calculates whether the result is a success.
        /// </summary>
        public bool IsSuccess => Status.HasFlag(ResultStatus.IsSuccess);

        /// <summary>
        /// Calculates whether the result has a value.
        /// </summary>
        public bool HasValue => Status.HasFlag(ResultStatus.HasValue);

        /// <summary>
        /// Creates a new successful result without a value.
        /// </summary>
        public Result()
        {
            Value = default;
            Errors = Result._emptyErrors;
            Status = ResultStatus.IsSuccess;
        }

        internal Result(ResultStatus status)
        {
            Value = default;
            Errors = Result._emptyErrors;
            Status = status;
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

        /// <summary>
        /// Converts a value to a result.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        public static implicit operator Result<T>(T value) => new(value);

        /// <summary>
        /// Converts a result to a value.
        /// </summary>
        /// <param name="value">The result to convert.</param>
        public static implicit operator Result(Result<T> value) => new(value.Value, value.Errors);
    }
}
