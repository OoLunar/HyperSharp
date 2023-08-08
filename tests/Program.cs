using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using OoLunar.HyperSharp.Setup;
using OoLunar.HyperSharp.Tests.Responders;
using System.IO;
using System.Text;
using System.Linq;
using BenchmarkDotNet.Portability.Cpu;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
#if DEBUG
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
#endif

namespace OoLunar.HyperSharp.Tests
{
    public sealed class Program
    {
        public static void Main()
        {
#if DEBUG
            Summary[] summaries = BenchmarkRunner.Run(typeof(Program).Assembly, ManualConfig.CreateMinimumViable().WithOptions(ConfigOptions.DisableOptimizationsValidator).AddJob(Job.Dry));
#else
            Summary[] summaries = BenchmarkRunner.Run(typeof(Program).Assembly);
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
                foreach (BenchmarkReport report in summary.Reports.OrderBy(report => !summary.IsBaseline(report.BenchmarkCase)).ThenBy(report => !report.Success).ThenBy(report => report.BenchmarkCase.Descriptor.WorkloadMethodDisplayInfo))
                {
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

                    string mean = GetHumanizedNanoSeconds(report.ResultStatistics.Mean);
                    string standardError = GetHumanizedNanoSeconds(report.ResultStatistics.StandardError);
                    string standardDeviation = GetHumanizedNanoSeconds(report.ResultStatistics.StandardDeviation);

                    fileStream.Write(":\nMean: "u8);
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

            services.AddLogging(logger =>
            {
                const string loggingFormat = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u4}] {SourceCont`t}: {Message:lj}{NewLine}{Exception}";
                IConfiguration configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
                LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
                    .MinimumLevel.Is(configuration.GetValue("logging:level", LogEventLevel.Debug))
                    .WriteTo.Console(outputTemplate: loggingFormat, formatProvider: CultureInfo.InvariantCulture, theme: new AnsiConsoleTheme(new Dictionary<ConsoleThemeStyle, string>
                    {
                        [ConsoleThemeStyle.Text] = "\x1b[0m",
                        [ConsoleThemeStyle.SecondaryText] = "\x1b[90m",
                        [ConsoleThemeStyle.TertiaryText] = "\x1b[90m",
                        [ConsoleThemeStyle.Invalid] = "\x1b[31m",
                        [ConsoleThemeStyle.Null] = "\x1b[95m",
                        [ConsoleThemeStyle.Name] = "\x1b[93m",
                        [ConsoleThemeStyle.String] = "\x1b[96m",
                        [ConsoleThemeStyle.Number] = "\x1b[95m",
                        [ConsoleThemeStyle.Boolean] = "\x1b[95m",
                        [ConsoleThemeStyle.Scalar] = "\x1b[95m",
                        [ConsoleThemeStyle.LevelVerbose] = "\x1b[34m",
                        [ConsoleThemeStyle.LevelDebug] = "\x1b[90m",
                        [ConsoleThemeStyle.LevelInformation] = "\x1b[36m",
                        [ConsoleThemeStyle.LevelWarning] = "\x1b[33m",
                        [ConsoleThemeStyle.LevelError] = "\x1b[31m",
                        [ConsoleThemeStyle.LevelFatal] = "\x1b[97;91m"
                    }))
                    .WriteTo.File(
                        $"logs/{DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd' 'HH'.'mm'.'ss", CultureInfo.InvariantCulture)}.log",
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: loggingFormat,
                        formatProvider: CultureInfo.InvariantCulture
                    );

                // Allow specific namespace log level overrides, which allows us to hush output from things like the database basic SELECT queries on the Information level.
                foreach (IConfigurationSection logOverride in configuration.GetSection("logging:overrides").GetChildren())
                {
                    if (logOverride.Value is null || !Enum.TryParse(logOverride.Value, out LogEventLevel logEventLevel))
                    {
                        continue;
                    }

                    loggerConfiguration.MinimumLevel.Override(logOverride.Key, logEventLevel);
                }

                logger.AddSerilog(loggerConfiguration.CreateLogger());
            });

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
                hyperConfiguration.AddResponders(new[] { typeof(HelloWorldResponder) });
            });

            services.AddSingleton(services =>
            {
                HyperConfiguration hyperConfiguration = services.GetRequiredService<HyperConfiguration>();
                return new HttpClient()
                {
                    BaseAddress = new Uri($"http://{hyperConfiguration.ListeningEndpoint}/"),
                    DefaultRequestHeaders = { { "User-Agent", $"HyperSharp/{typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion} Github" } }
                };
            });

            return services.BuildServiceProvider();
        }
    }
}
