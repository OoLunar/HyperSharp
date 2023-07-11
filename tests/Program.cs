using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace OoLunar.HyperSharp.Tests
{
    public sealed class Program
    {
        public static async Task Main()
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
                string? host = configuration.GetValue("listening:address", "localhost")?.Trim();
                if (string.IsNullOrWhiteSpace(host))
                {
                    throw new ArgumentException("The listening address cannot be null or whitespace.", nameof(host));
                }

                if (!IPAddress.TryParse(host, out IPAddress? address))
                {
                    IPAddress[] addresses = Dns.GetHostAddresses(host);
                    address = addresses.Length != 0 ? addresses[0] : throw new InvalidOperationException("The listening address could not be resolved to an IP address.");
                }

                hyperConfiguration.ListeningEndpoint = new IPEndPoint(address, configuration.GetValue("listening:port", 8080));
            });

            services.AddSingleton(new HttpClient() { DefaultRequestHeaders = { { "User-Agent", $"HyperSharp/{typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion} Github" } } });
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetRequiredService<HyperServer>().Run();
            await Task.Delay(TimeSpan.FromSeconds(5));

            HttpClient httpClient = serviceProvider.GetRequiredService<HttpClient>();
            HyperConfiguration hyperConfiguration = serviceProvider.GetRequiredService<HyperConfiguration>();
            HttpResponseMessage response = await httpClient.GetAsync($"http://{hyperConfiguration.ListeningEndpoint}/");

            await Task.Delay(-1);
        }
    }
}
