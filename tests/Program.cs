using System.Threading.Tasks;
using OoLunar.HyperSharp.Tests.Benchmarks.Benchmarks;
#if !DEBUG
using BenchmarkDotNet.Running;
#endif

namespace OoLunar.HyperSharp.Tests.Benchmarks
{
    public sealed class Program
    {
#if DEBUG
        public static async Task Main()
        {
            ConcurrentRequests concurrentRequestsTest = new();
            (await concurrentRequestsTest.ConcurrentRequestsTestAsync()).EnsureSuccessStatusCode();
            (await concurrentRequestsTest.ConcurrentRequestsTestAsync()).EnsureSuccessStatusCode();
            (await concurrentRequestsTest.ConcurrentRequestsTestAsync()).EnsureSuccessStatusCode();
        }
#else
        public static void Main() => BenchmarkRunner.Run<ConcurrentRequests>();
#endif
    }
}
