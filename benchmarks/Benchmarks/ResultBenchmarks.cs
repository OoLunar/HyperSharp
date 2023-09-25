using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using HyperSharp.Results;

namespace HyperSharp.Benchmarks.Cases
{
    [JsonExporterAttribute.Brief, Full, SuppressMessage("Roslyn", "CA1822", Justification = "BenchmarkDotNet does not support static methods.")]
    public class ResultBenchmarks
    {
        private readonly object _value = new();
        private readonly Error _error = new("test");
        private readonly Error[] _errors = new[] { new Error("test"), new Error("test2") };

        [WarmupCount(5), Benchmark(Baseline = true)]
        public Result CreateSuccessfulResult() => Result.Success();

        [Benchmark]
        public Result CreateSuccessfulResultWithValue() => Result.Success(_value);

        [Benchmark]
        public Result CreateFailedResult() => Result.Failure("test");

        [Benchmark]
        public Result CreateFailedResultWithValue() => Result.Failure(_value, _error);

        [Benchmark]
        public Result CreateFailedResultWithMultipleErrors() => Result.Failure(_errors);
    }
}
