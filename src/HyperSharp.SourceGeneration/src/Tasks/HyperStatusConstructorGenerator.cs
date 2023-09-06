using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using NuGet.Frameworks;

namespace HyperSharp.SourceGeneration.Tasks
{
    public sealed class HyperStatusConstructorGenerator : ITask
    {
        private const string CODE_TEMPLATE = """
// <auto-generated/>

#nullable enable
namespace HyperSharp.Protocol
{
    public readonly partial record struct HyperStatus
    {
        #if {{NetVersion}}_OR_GREATER

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.{{Code}}" />
        public static HyperStatus {{Code}}() => new(global::System.Net.HttpStatusCode.{{Code}}, new HyperHeaderCollection(), null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.{{Code}}" />
        /// <param name="body">The body of the response.</param>
        public static HyperStatus {{Code}}(object? body) => new(global::System.Net.HttpStatusCode.{{Code}}, new HyperHeaderCollection(), body);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.{{Code}}" />
        /// <param name="headers">The headers of the response.</param>
        public static HyperStatus {{Code}}(HyperHeaderCollection headers) => new(global::System.Net.HttpStatusCode.{{Code}}, headers, null);

        /// <inheritdoc cref="global::System.Net.HttpStatusCode.{{Code}}" />
        /// <param name="headers">The headers of the response.</param>
        /// <param name="body">The body of the response.</param>
        public static HyperStatus {{Code}}(HyperHeaderCollection headers, object? body) => new(global::System.Net.HttpStatusCode.{{Code}}, headers, body);

        #endif
    }
}

""";

        public bool Execute(IConfiguration configuration)
        {
            string projectRoot = Directory.GetCurrentDirectory();
            string? targetFrameworks = configuration["TargetFrameworks"];
            if (targetFrameworks is null)
            {
                Console.WriteLine("The 'TargetFrameworks' property is not set.");
                return false;
            }

            string[] targetedFrameworks = targetFrameworks.Split(';');
            string? dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
            if (dotnetRoot is null)
            {
                Console.WriteLine("The DOTNET_ROOT environment variable is not set.");
                return false;
            }

            Dictionary<NuGetFramework, (string, string)> nuGetFrameworks = new();
            foreach (string sdkVersion in Directory.GetDirectories(Path.Combine(dotnetRoot, "shared/Microsoft.NETCore.App/")))
            {
                string msBuildDependenciesJsonPath = Path.Combine(sdkVersion, "Microsoft.NETCore.App.runtimeconfig.json");
                if (!File.Exists(msBuildDependenciesJsonPath))
                {
                    Console.WriteLine($"Skipping '{sdkVersion}' because '{msBuildDependenciesJsonPath}' does not exist.");
                    continue;
                }

                JsonDocument? jsonDocument = JsonDocument.Parse(File.ReadAllText(msBuildDependenciesJsonPath));
                if (jsonDocument is null)
                {
                    Console.WriteLine($"Skipping '{sdkVersion}' because '{msBuildDependenciesJsonPath}' could not be parsed as JSON.");
                    continue;
                }

                if (!jsonDocument.RootElement.TryGetProperty("runtimeOptions", out JsonElement runtimeOptions) || !runtimeOptions.TryGetProperty("tfm", out JsonElement tfm))
                {
                    Console.WriteLine($"Skipping '{sdkVersion}' because '{msBuildDependenciesJsonPath}' does not contain a 'runtimeOptions.tfm' property.");
                    continue;
                }

                string? targetFrameworkMoniker = tfm.GetString();
                if (targetFrameworkMoniker is null)
                {
                    Console.WriteLine($"Skipping '{sdkVersion}' because '{msBuildDependenciesJsonPath}' the 'runtimeOptions.tfm' property contains a null value.");
                    continue;
                }
                else if (!targetedFrameworks.Any(framework => framework.StartsWith(targetFrameworkMoniker, StringComparison.OrdinalIgnoreCase)))
                {
                    Console.WriteLine($"Skipping '{sdkVersion}' because '{msBuildDependenciesJsonPath}' the 'runtimeOptions.tfm' property contains a value that is not targeted by the project.");
                    continue;
                }

                nuGetFrameworks.Add(NuGetFramework.Parse(targetFrameworkMoniker), (msBuildDependenciesJsonPath, targetFrameworkMoniker));
            }

            foreach (KeyValuePair<NuGetFramework, (string, string)> kvp in nuGetFrameworks.OrderBy(framework => framework.Key, new NuGetFrameworkSorter()))
            {
                NuGetFramework nuGetFramework = kvp.Key;
                string msBuildDependenciesJsonPath = kvp.Value.Item1;
                string targetFrameworkMoniker = kvp.Value.Item2;

                Assembly assembly = Assembly.LoadFile(Path.Combine(Path.GetDirectoryName(msBuildDependenciesJsonPath)!, "System.Net.Primitives.dll"));
                string[] httpStatuses = Enum.GetNames(assembly.GetType("System.Net.HttpStatusCode")!);
                foreach (string httpStatus in httpStatuses)
                {
                    if (File.Exists($"{projectRoot}/Protocol/HyperStatus/HyperStatus.{httpStatus}.g.cs"))
                    {
                        continue;
                    }

                    StringBuilder stringBuilder = new StringBuilder(CODE_TEMPLATE)
                        .Replace("{{NetVersion}}", targetFrameworkMoniker.Replace('.', '_').ToUpperInvariant())
                        .Replace("{{Code}}", httpStatus);
                    File.WriteAllText($"{projectRoot}/Protocol/HyperStatus/HyperStatus.{httpStatus}.g.cs", stringBuilder.ToString());
                }
            }

            return true;
        }
    }
}