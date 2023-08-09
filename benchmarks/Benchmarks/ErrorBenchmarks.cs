using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using OoLunar.HyperSharp.Results;

namespace OoLunar.HyperSharp.Benchmarks.Cases
{
    [JsonExporterAttribute.Brief, SuppressMessage("Roslyn", "CA1822", Justification = "BenchmarkDotNet does not support static methods.")]
    public class ErrorBenchmarks
    {
        private readonly Error _subError = new("This is a test sub-error.");
        private readonly Error[] _subErrors = new Error[]
        {
            new("This is a test sub-error 1."),
            new("This is a test sub-error 2.")
        };

        [WarmupCount(5), Benchmark(Baseline = true)]
        public Error CreateError() => new();

        [Benchmark]
        public Error CreateErrorWithMessage() => new("This is a test error.");

        [Benchmark]
        public Error CreateErrorWithSubError() => new("This is a test error.", _subError);

        [Benchmark]
        public Error CreateErrorWithSubErrors() => new("This is a test error.", _subErrors);
    }
}
