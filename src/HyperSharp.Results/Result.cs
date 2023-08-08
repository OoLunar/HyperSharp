using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using OoLunar.HyperSharp.Results.Json;

namespace OoLunar.HyperSharp.Results
{
    [JsonConverter(typeof(ResultJsonConverter))]
    public readonly record struct Result
    {
        internal static readonly Error[] _emptyErrors = Array.Empty<Error>();

        public readonly object? Value;
        public readonly IEnumerable<Error> Errors;
        public readonly ResultStatus Status;
        public bool IsSuccess => Status.HasFlag(ResultStatus.IsSuccess);
        public bool HasValue => Status.HasFlag(ResultStatus.HasValue);

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
            Errors = errors;
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
            Errors = errors;
            Status = ResultStatus.HasValue;
        }

        public static Result Success() => new();
        public static Result Success(object? value) => new(value);
        public static Result Failure(string error) => new(new Error(error));
        public static Result Failure(Error error) => new(error);
        public static Result Failure(IEnumerable<Error> errors) => new(errors);
        public static Result Failure(object? value, Error error) => new(value, error);
        public static Result Failure(object? value, IEnumerable<Error> errors) => new(value, errors);

        public static Result<T> Success<T>() => new();
        public static Result<T> Success<T>(T value) => new(value);
        public static Result<T> Failure<T>(string error) => new(new Error(error));
        public static Result<T> Failure<T>(Error error) => new(error);
        public static Result<T> Failure<T>(IEnumerable<Error> errors) => new(errors);
        public static Result<T> Failure<T>(T value, Error error) => new(value, error);
        public static Result<T> Failure<T>(T value, IEnumerable<Error> errors) => new(value, errors);
    }
}
