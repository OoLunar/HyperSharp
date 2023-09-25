using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Toolkit.HighPerformance;

namespace HyperSharp.Protocol
{
    public static partial class HyperSerializers
    {
        private static readonly byte[] _jsonEncodingHeader = "Content-Type: application/json; charset=utf-8\r\nContent-Length: "u8.ToArray();

        /// <summary>
        /// Serializes the body to the client as JSON using the <see cref="JsonSerializer.SerializeToUtf8Bytes{TValue}(TValue, JsonSerializerOptions?)"/> method with the <see cref="HyperConfiguration.JsonSerializerOptions"/> options.
        /// </summary>
        /// <remarks>
        /// This serializer is the default serializer for HyperSharp.
        /// </remarks>
        /// <inheritdoc cref="HyperSerializerDelegate"/>
        public static ValueTask<bool> JsonAsync(HyperContext context, HyperStatus status, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(status);

            // Write Content-Type header and beginning of Content-Length header
            context.Connection.StreamWriter.Write<byte>(_jsonEncodingHeader);
            byte[] body = JsonSerializer.SerializeToUtf8Bytes(status.Body, context.Connection.Server.Configuration.JsonSerializerOptions);

            // Finish the Content-Length header
            context.Connection.StreamWriter.Write<byte>(Encoding.ASCII.GetBytes(body.Length.ToString())); // TODO: This could probably be done without allocating a string
            context.Connection.StreamWriter.Write<byte>(_newLine);

            // Write body
            context.Connection.StreamWriter.Write<byte>(_newLine);
            context.Connection.StreamWriter.Write<byte>(body);

            return ValueTask.FromResult(true);
        }
    }
}
