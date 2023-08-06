using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using OoLunar.HyperSharp.Results;

namespace OoLunar.HyperSharp.Tests.Benchmarks
{
    [JsonExporterAttribute.Brief, SuppressMessage("Roslyn", "CA1822", Justification = "BenchmarkDotNet does not support static methods.")]
    public class GenericResults
    {
        [WarmupCount(5), Benchmark(Baseline = true)]
        public Result<int> CreateGenericSuccessfulResult() => Result.Success<int>();

        [WarmupCount(5), Benchmark]
        public Result<int> CreateGenericSuccessfulResultWithValue() => Result.Success(1);

        [WarmupCount(5), Benchmark]
        public Result<int> CreateGenericFailedResult() => Result.Failure<int>("test");

        [WarmupCount(5), Benchmark]
        public Result<int> CreateGenericFailedResultWithValue() => Result.Failure(1, new Error("test"));

        [WarmupCount(5), Benchmark]
        public Result<int> CreateGenericFailedResultWithMultipleErrors() => Result.Failure<int>(new[] { new Error("test"), new Error("test2") });
    }
}
