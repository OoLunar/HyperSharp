using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using OoLunar.HyperSharp.Results;

namespace OoLunar.HyperSharp.Benchmarks.Cases
{
    [JsonExporterAttribute.Brief, SuppressMessage("Roslyn", "CA1822", Justification = "BenchmarkDotNet does not support static methods.")]
    public class GenericResultBenchmarks
    {
        private readonly Error _error = new("test");
        private readonly Error[] _errors = new[] { new Error("test"), new Error("test2") };

        [WarmupCount(5), Benchmark(Baseline = true)]
        public Result<int> CreateGenericSuccessfulResult() => Result.Success<int>();

        [Benchmark]
        public Result<int> CreateGenericSuccessfulResultWithValue() => Result.Success(1);

        [Benchmark]
        public Result<int> CreateGenericFailedResult() => Result.Failure<int>("test");

        [Benchmark]
        public Result<int> CreateGenericFailedResultWithValue() => Result.Failure(1, _error);

        [Benchmark]
        public Result<int> CreateGenericFailedResultWithMultipleErrors() => Result.Failure<int>(_errors);
    }
}
