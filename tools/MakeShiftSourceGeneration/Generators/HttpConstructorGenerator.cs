using System;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.Build.Utilities;
using Namotion.Reflection;

namespace HyperSharp.Tools.MakeShiftSourceGeneration.Generators
{
    /// <summary>
    /// Generates the static methods based off of the <see cref="HttpStatusCode"/> enum for the HyperStatus struct.
    /// </summary>
    public sealed class HttpConstructorGenerator : Task
    {
        private static readonly string[] _httpStatuses = Enum.GetNames(typeof(HttpStatusCode));
        private static readonly string _codeTemplate =
"""
using System.Net;

#nullable enable
namespace HyperSharp.Protocol
{
    public readonly partial record struct HyperStatus
    {
        /// <summary>
        /// {Summary}
        /// </summary>
        public static HyperStatus {Code}() => new(HttpStatusCode.{Code}, new HyperHeaderCollection(), null);

        /// <summary>
        /// {Summary}
        /// </summary>
        public static HyperStatus {Code}(object? body) => new(HttpStatusCode.{Code}, new HyperHeaderCollection(), body);

        /// <summary>
        /// {Summary}
        /// </summary>
        public static HyperStatus {Code}(HyperHeaderCollection headers) => new(HttpStatusCode.{Code}, headers, null);

        /// <summary>
        /// {Summary}
        /// </summary>
        public static HyperStatus {Code}(HyperHeaderCollection headers, object? body) => new(HttpStatusCode.{Code}, headers, body);
    }
}

""";

        /// <inheritdoc/>
        public override bool Execute()
        {
            foreach (string httpStatus in _httpStatuses)
            {
                StringBuilder stringBuilder = new(_codeTemplate);
                _ = stringBuilder.Replace("{Code}", httpStatus);
                _ = stringBuilder.Replace("{Summary}", typeof(HttpStatusCode).GetField(httpStatus).GetXmlDocsSummary());

                File.WriteAllText($"/home/lunar/Code/OoLunar/HyperSharp/src/HyperSharp/Protocol/HyperStatus/HyperStatus.{httpStatus}.g.cs", stringBuilder.ToString());
            }

            return true;
        }
    }
}
