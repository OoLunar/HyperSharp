using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using OoLunar.HyperSharp.Results;

namespace OoLunar.HyperSharp.Tests.Benchmarks
{
    [JsonExporterAttribute.Brief, SuppressMessage("Roslyn", "CA1822", Justification = "BenchmarkDotNet does not support static methods.")]
    public class Results
    {
        [WarmupCount(5), Benchmark(Baseline = true)]
        public Result CreateSuccessfulResult() => Result.Success();

        [WarmupCount(5), Benchmark]
        public Result CreateSuccessfulResultWithValue() => Result.Success(new Error("test"));

        [WarmupCount(5), Benchmark]
        public Result CreateFailedResult() => Result.Failure("test");

        [WarmupCount(5), Benchmark]
        public Result CreateFailedResultWithValue() => Result.Failure(1, new Error("test"));

        [WarmupCount(5), Benchmark]
        public Result CreateFailedResultWithMultipleErrors() => Result.Failure(new[] { new Error("test"), new Error("test2") });
    }
}
