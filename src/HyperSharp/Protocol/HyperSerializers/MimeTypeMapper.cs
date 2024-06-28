using System;
using System.Collections.Generic;
#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#else
using System.Collections.Immutable;
#endif
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.HighPerformance;
using System.Linq;

namespace HyperSharp.Protocol
{
    /// <summary>
    /// Holds a collection of static methods implementing <see cref="HyperSerializerDelegate"/> for the most common of Content-Types.
    /// </summary>
    public static partial class HyperSerializers
    {
        private static readonly FrozenDictionary<string, IReadOnlyList<string>> _mimeTypes;
        private static readonly FrozenDictionary<string, Lazy<HyperSerializerDelegate>> _mimeTypeSerializers;
        private static readonly FrozenDictionary<string, Lazy<HyperSerializerDelegate>> _fileExtensionSerializers;

        static HyperSerializers()
        {
            string? mimeFile = null;
            if (Path.Exists("/etc/mime.types"))
            {
                mimeFile = "/etc/mime.types";
            }
            else
            {
                string? home = Environment.GetEnvironmentVariable(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "HOMEPATH" : "HOME");
                if (!string.IsNullOrWhiteSpace(home) && Path.Exists(Path.Combine(home, ".mime.types")))
                {
                    mimeFile = Path.Combine(home, ".mime.types");
                }
            }

            Dictionary<string, List<string>> mimeTypes = new(StringComparer.OrdinalIgnoreCase)
            {
                { "application/json", new List<string>() { "json" } },
                { "text/plain", new List<string>() { "txt" } },
            };

            Dictionary<string, Lazy<HyperSerializerDelegate>> mimeTypeSerializers = new(StringComparer.OrdinalIgnoreCase)
            {
                { "application/json", new Lazy<HyperSerializerDelegate>(JsonAsync) },
                { "text/plain", new Lazy<HyperSerializerDelegate>(PlainTextAsync) },
            };

            Dictionary<string, Lazy<HyperSerializerDelegate>> fileExtensionSerializers = new(StringComparer.OrdinalIgnoreCase)
            {
                { "json", new Lazy<HyperSerializerDelegate>(JsonAsync) },
                { "txt", new Lazy<HyperSerializerDelegate>(PlainTextAsync) },
            };

            if (!string.IsNullOrWhiteSpace(mimeFile))
            {
                FileStream fileStream = File.OpenRead(mimeFile);
                StreamReader streamReader = new(fileStream);

                string? line;
                while ((line = streamReader.ReadLine()) is not null)
                {
                    if (line.StartsWith('#') || string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    string[] parts = line.Replace('\t', ' ').Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    string mimeType = parts[0];
                    Lazy<HyperSerializerDelegate> serializer = new(() => (context, status, cancellationToken) =>
                    {
                        ArgumentNullException.ThrowIfNull(context);
                        ArgumentNullException.ThrowIfNull(status);

                        // Write Content-Type header and beginning of Content-Length header
                        context.Connection.StreamWriter.Write<byte>(Encoding.ASCII.GetBytes($"Content-Type: {mimeType}\r\nContent-Length: "));

                        byte[] body = Encoding.UTF8.GetBytes(status.Body?.ToString() ?? "");

                        // Write Content-Length header
                        context.Connection.StreamWriter.Write<byte>(Encoding.ASCII.GetBytes(body.Length.ToString()));
                        context.Connection.StreamWriter.Write<byte>(_newLine);

                        // Write body
                        context.Connection.StreamWriter.Write<byte>(_newLine);
                        context.Connection.StreamWriter.Write<byte>(body);

                        return ValueTask.FromResult(true);
                    });

                    mimeTypeSerializers[mimeType] = serializer;

                    List<string> fileExtensions = mimeTypes.TryGetValue(mimeType, out List<string>? extensions) ? extensions : [];
                    foreach (string fileExtension in parts[1..])
                    {
                        fileExtensionSerializers[fileExtension] = serializer;
                        fileExtensions.Add(fileExtension);
                    }
                }
            }

            _fileExtensionSerializers = fileExtensionSerializers.ToFrozenDictionary();
            _mimeTypeSerializers = mimeTypeSerializers.ToFrozenDictionary();
            _mimeTypes = mimeTypes.Select(x => (x.Key, (IReadOnlyList<string>)x.Value)).ToDictionary(x => x.Key, x => x.Item2).ToFrozenDictionary();
        }

        /// <summary>
        /// Gets the <see cref="HyperSerializerDelegate"/> for the specified MIME type.
        /// </summary>
        /// <param name="mimeType">The MIME type to get the <see cref="HyperSerializerDelegate"/> for.</param>
        /// <param name="context">The <see cref="HyperContext"/> to get the <see cref="HyperSerializerDelegate"/> for.</param>
        /// <returns>The <see cref="HyperSerializerDelegate"/> for the specified MIME type. Defaults to <see cref="PlainTextAsync"/> if the MIME type is not found.</returns>
        public static HyperSerializerDelegate GetSerializerFromMimeType(string mimeType, HyperContext? context = null)
        {
            if (context is not null && context.Headers.TryGetValues("Accept", out List<string>? accept) && !accept.Contains(mimeType) && mimeType.StartsWith("text", StringComparison.OrdinalIgnoreCase))
            {
                mimeType = "text/html";
            }

            return _mimeTypeSerializers.TryGetValue(mimeType, out Lazy<HyperSerializerDelegate>? serializer) ? serializer.Value : PlainTextAsync;
        }

        /// <summary>
        /// Gets the <see cref="HyperSerializerDelegate"/> associated with the specified file extension.
        /// </summary>
        /// <param name="fileExtension">The file extension to get the <see cref="HyperSerializerDelegate"/> for.</param>
        /// <returns>The <see cref="HyperSerializerDelegate"/> for the specified file extension. Defaults to <see cref="PlainTextAsync"/> if the file extension is not found.</returns>
        public static HyperSerializerDelegate GetSerializerFromFileExtension(string fileExtension) => _fileExtensionSerializers.TryGetValue(fileExtension, out Lazy<HyperSerializerDelegate>? serializer)
            ? serializer.Value
            : PlainTextAsync;

        public static string GetMimeTypeFromFileExtension(string fileExtension) => _fileExtensionSerializers.TryGetValue(fileExtension, out Lazy<HyperSerializerDelegate>? serializer)
            ? serializer.Value.Method.Name switch
            {
                nameof(JsonAsync) => "application/json",
                nameof(PlainTextAsync) => "text/plain",
                _ => "text/html",
            }
            : "text/html";
    }
}
