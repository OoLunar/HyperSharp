using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using HyperSharp.Results.Json;

namespace HyperSharp.Results
{
    /// <summary>
    /// Represents a result that may or may not have a value.
    /// </summary>
    [JsonConverter(typeof(ResultJsonConverter))]
    public readonly record struct Result
    {
        internal static readonly Error[] _emptyErrors = [];

        /// <summary>
        /// The value of the result. Check <see cref="HasValue"/> before accessing.
        /// </summary>
        public readonly object? Value;

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
            Value = null;
            Errors = _emptyErrors;
            Status = ResultStatus.IsSuccess;
        }

        internal Result(object? value)
        {
            Value = value;
            Errors = _emptyErrors;
            Status = ResultStatus.IsSuccess | ResultStatus.HasValue;
        }

        internal Result(Error error)
        {
            Value = null;
            Errors = new[] { error };
            Status = ResultStatus.None;
        }

        internal Result(IEnumerable<Error> errors)
        {
            Value = null;
            Errors = errors.ToArray();
            Status = ResultStatus.None;
        }

        internal Result(object? value, Error error)
        {
            Value = value;
            Errors = new[] { error };
            Status = ResultStatus.HasValue;
        }

        internal Result(object? value, IEnumerable<Error> errors)
        {
            Value = value;
            Errors = errors.ToArray();
            Status = ResultStatus.HasValue;
        }

        /// <summary>
        /// Creates an empty and successful result.
        /// </summary>
        public static Result Success() => new();

        /// <summary>
        /// Creates a successful result with a value.
        /// </summary>
        /// <param name="value">The value of the result.</param>
        public static Result Success(object? value) => new(value);

        /// <summary>
        /// Creates a failed result with an error.
        /// </summary>
        /// <param name="error">The error of the result.</param>
        public static Result Failure(string error) => new(new Error(error));

        /// <summary>
        /// Creates a failed result with an error.
        /// </summary>
        /// <param name="error">The error of the result.</param>
        public static Result Failure(Error error) => new(error);

        /// <summary>
        /// Creates a failed result with multiple errors.
        /// </summary>
        /// <param name="errors">The errors of the result.</param>
        public static Result Failure(IEnumerable<Error> errors) => new(errors);

        /// <inheritdoc cref="Failure(object?, Error)"/>
        public static Result Failure(object? value, Error error) => new(value, error);

        /// <summary>
        /// Creates a failed result with a value and multiple errors.
        /// </summary>
        /// <param name="value">The value of the result.</param>
        /// <param name="errors">The errors of the result.</param>
        public static Result Failure(object? value, IEnumerable<Error> errors) => new(value, errors);

        /// <inheritdoc cref="Success()"/>
        public static Result<T> Success<T>() => new();

        /// <inheritdoc cref="Success(object?)"/>
        public static Result<T> Success<T>(T value) => new(value);

        /// <inheritdoc cref="Failure(string)"/>
        public static Result<T> Failure<T>(string error) => new(new Error(error));

        /// <inheritdoc cref="Failure(Error)"/>
        public static Result<T> Failure<T>(Error error) => new(error);

        /// <inheritdoc cref="Failure(IEnumerable{Error})"/>
        public static Result<T> Failure<T>(IEnumerable<Error> errors) => new(errors);

        /// <inheritdoc cref="Failure(object?, Error)"/>
        public static Result<T> Failure<T>(T value, Error error) => new(value, error);

        /// <inheritdoc cref="Failure(object?, IEnumerable{Error})"/>
        public static Result<T> Failure<T>(T value, IEnumerable<Error> errors) => new(value, errors);
    }
}
