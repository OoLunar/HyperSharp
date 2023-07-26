using BenchmarkDotNet.Running;
using OoLunar.HyperSharp.Tests.Benchmarks.Benchmarks;

namespace OoLunar.HyperSharp.Tests.Benchmarks
{
    public sealed class Program
    {
        public static void Main() => BenchmarkRunner.Run<ConcurrentRequests>();
    }
}
