using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using BenchmarkDotNet.Portability.Cpu;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using HyperSharp.Benchmarks.Responders;
using HyperSharp.Setup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
#if DEBUG
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
#endif

namespace HyperSharp.Benchmarks
{
    public sealed class Program
    {
        public static void Main(string[] args)
        {
#if DEBUG
            Summary[] summaries = BenchmarkRunner.Run(typeof(Program).Assembly, ManualConfig.CreateMinimumViable().WithOptions(ConfigOptions.DisableOptimizationsValidator).AddJob(Job.Dry), args);
#else
            Summary[] summaries = BenchmarkRunner.Run(typeof(Program).Assembly, args: args);
#endif
            Summary firstSummary = summaries[0];
            File.WriteAllText("benchmark-results.md", string.Empty);

            using FileStream fileStream = File.OpenWrite("benchmark-results.md");
            fileStream.Write("### Machine Information:\nBenchmarkDotNet v"u8);
            fileStream.Write(Encoding.UTF8.GetBytes(firstSummary.HostEnvironmentInfo.BenchmarkDotNetVersion));
            fileStream.Write(", "u8);
            fileStream.Write(Encoding.UTF8.GetBytes(firstSummary.HostEnvironmentInfo.OsVersion.Value));
            fileStream.Write("\n- "u8);
            fileStream.Write(Encoding.UTF8.GetBytes(CpuInfoFormatter.Format(firstSummary.HostEnvironmentInfo.CpuInfo.Value)));
            fileStream.Write("\n- Hardware Intrinsics: "u8);
            fileStream.Write(Encoding.UTF8.GetBytes(firstSummary.Reports[0].GetHardwareIntrinsicsInfo()?.Replace(",", ", ") ?? "Not supported."));
            fileStream.Write("\n- "u8);
            fileStream.Write(Encoding.UTF8.GetBytes(firstSummary.HostEnvironmentInfo.RuntimeVersion));
            fileStream.Write(", "u8);
            fileStream.Write(Encoding.UTF8.GetBytes(firstSummary.HostEnvironmentInfo.Architecture.ToLowerInvariant()));
            fileStream.Write(", "u8);
            fileStream.Write(Encoding.UTF8.GetBytes(firstSummary.HostEnvironmentInfo.JitInfo));
            fileStream.Write("\n- Total Execution Time: "u8);
            fileStream.Write(Encoding.UTF8.GetBytes(GetHumanizedNanoSeconds(summaries.Sum(summary => summary.TotalTime.TotalNanoseconds))));

            foreach (Summary summary in summaries.OrderBy(summary => summary.BenchmarksCases.Length))
            {
                fileStream.Write("\n\n## "u8);
                fileStream.Write(Encoding.UTF8.GetBytes(summary.BenchmarksCases[0].Descriptor.Type.Name));
                fileStream.Write("\nExecution Time: "u8);
                fileStream.Write(Encoding.UTF8.GetBytes(GetHumanizedNanoSeconds(summary.TotalTime.TotalNanoseconds)));

                // baseline first, then order by success, then by name
                BenchmarkReport[] array = summary.Reports.OrderBy(report => !summary.IsBaseline(report.BenchmarkCase)).ThenBy(report => !report.Success).ThenBy(report => report.BenchmarkCase.Descriptor.WorkloadMethodDisplayInfo).ToArray();
                for (int i = 0; i < array.Length; i++)
                {
                    BenchmarkReport report = array[i];
                    fileStream.Write("\n### "u8);
                    fileStream.Write(Encoding.UTF8.GetBytes(report.BenchmarkCase.Descriptor.WorkloadMethodDisplayInfo));
                    if (summary.IsBaseline(report.BenchmarkCase))
                    {
                        fileStream.Write(", Baseline"u8);
                    }

                    if (!report.Success)
                    {
                        fileStream.Write(", Failed"u8);
                    }

                    if (report.ResultStatistics is null)
                    {
                        fileStream.Write(":\nNo results."u8);
                        continue;
                    }

                    fileStream.Write(":"u8);

                    string mean = GetHumanizedNanoSeconds(report.ResultStatistics.Mean);
                    string standardError = GetHumanizedNanoSeconds(report.ResultStatistics.StandardError);
                    string standardDeviation = GetHumanizedNanoSeconds(report.ResultStatistics.StandardDeviation);

                    // Calculate the ratio compared to the baseline (if not the baseline itself)
                    if (!summary.IsBaseline(report.BenchmarkCase))
                    {
                        BenchmarkReport? baselineReport = summary.BenchmarksCases.Select(benchmarkCase => summary.Reports.FirstOrDefault(r => r.BenchmarkCase == benchmarkCase)).FirstOrDefault(r => summary.IsBaseline(r!.BenchmarkCase));
                        if (baselineReport is not null && baselineReport.ResultStatistics is not null)
                        {
                            double ratio = baselineReport.ResultStatistics.Mean / report.ResultStatistics.Mean;
                            double percentage = -(1 - ratio) * 100;
                            fileStream.Write("\nRatio: "u8);
                            fileStream.Write(Encoding.UTF8.GetBytes(percentage.ToString("N2", CultureInfo.InvariantCulture)));
                            fileStream.Write("% "u8);
                            fileStream.Write(double.IsPositive(percentage) ? "faster"u8 : "slower"u8);
                        }
                    }

                    fileStream.Write("\nMean: "u8);
                    fileStream.Write(Encoding.UTF8.GetBytes(mean));
                    fileStream.Write("\nError: "u8);
                    fileStream.Write(Encoding.UTF8.GetBytes(standardError));
                    fileStream.Write("\nStdDev: "u8);
                    fileStream.Write(Encoding.UTF8.GetBytes(standardDeviation));
                    fileStream.Write("\nMax per second: "u8);
                    fileStream.Write(Encoding.UTF8.GetBytes((1_000_000_000 / report.ResultStatistics.Mean).ToString("N2", CultureInfo.InvariantCulture)));
                    fileStream.Write(" (1,000,000,000ns / "u8);
                    fileStream.Write(Encoding.UTF8.GetBytes(report.ResultStatistics.Mean.ToString("N2", CultureInfo.InvariantCulture)));
                    fileStream.Write("ns)"u8);
                }
            }
        }

        private static string GetHumanizedNanoSeconds(double nanoSeconds) => nanoSeconds switch
        {
            < 1_000 => nanoSeconds.ToString("N0", CultureInfo.InvariantCulture) + "ns",
            < 1_000_000 => (nanoSeconds / 1_000).ToString("N2", CultureInfo.InvariantCulture) + "μs",
            < 1_000_000_000 => (nanoSeconds / 1_000_000).ToString("N2", CultureInfo.InvariantCulture) + "ms",
            _ => GetHumanizedExecutionTime(nanoSeconds / 1_000_000_000)
        };

        private static string GetHumanizedExecutionTime(double seconds)
        {
            StringBuilder stringBuilder = new();
            if (seconds >= 60)
            {
                stringBuilder.Append((seconds / 60).ToString("N0", CultureInfo.InvariantCulture));
                stringBuilder.Append("m and ");
                seconds %= 60;
            }

            stringBuilder.Append(seconds.ToString("N3", CultureInfo.InvariantCulture));
            stringBuilder.Append('s');
            return stringBuilder.ToString();
        }

        public static ServiceProvider CreateServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(services => new ConfigurationBuilder()
                .AddJsonFile("config.json", true, true)
#if DEBUG
                .AddJsonFile("config.debug.json", true, true)
#endif
                .AddEnvironmentVariables("HyperSharp_")
                .Build());

            services.AddLogging(logger => logger.AddProvider(NullLoggerProvider.Instance));
            services.AddHyperSharp((services, hyperConfiguration) =>
            {
                IConfiguration configuration = services.GetRequiredService<IConfiguration>();
                string? host = configuration.GetValue("server:address", "localhost")?.Trim();
                if (string.IsNullOrWhiteSpace(host))
                {
                    throw new ArgumentException("The server address cannot be null or whitespace.", nameof(host));
                }

                if (!IPAddress.TryParse(host, out IPAddress? address))
                {
                    IPAddress[] addresses = Dns.GetHostAddresses(host);
                    address = addresses.Length != 0 ? addresses[0] : throw new InvalidOperationException("The server address could not be resolved to an IP address.");
                }

                hyperConfiguration.ListeningEndpoint = new IPEndPoint(address, configuration.GetValue("server:port", 8080));
                hyperConfiguration.AddResponders(new[] { typeof(HelloWorldValueTaskResponder) });
            });

            services.AddSingleton(services =>
            {
                HyperConfiguration hyperConfiguration = services.GetRequiredService<HyperConfiguration>();
                return new HttpClient()
                {
                    BaseAddress = new Uri($"http://{hyperConfiguration.ListeningEndpoint}/"),
                    DefaultRequestHeaders = { { "User-Agent", $"HyperSharp/{typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion} Github" } },
                    DefaultRequestVersion = HttpVersion.Version11,
                    DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower
                };
            });

            return services.BuildServiceProvider();
        }
    }
}
