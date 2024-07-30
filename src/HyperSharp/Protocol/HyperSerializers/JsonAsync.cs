using System;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Toolkit.HighPerformance;

namespace HyperSharp.Protocol
{
    public static partial class HyperSerializers
    {
        private static readonly byte[] _contentTypeJsonEncodingHeader = "Content-Type: application/json\r\nContent-Length: "u8.ToArray();

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
            context.Connection.StreamWriter.Write<byte>(_contentTypeJsonEncodingHeader);
            byte[] body = JsonSerializer.SerializeToUtf8Bytes(status.Body ?? new object(), context.Connection.Server.Configuration.JsonSerializerOptions);

            // Finish the Content-Length header
            context.Connection.StreamWriter.Write<byte>(Encoding.ASCII.GetBytes(body.Length.ToString(CultureInfo.InvariantCulture)));
            context.Connection.StreamWriter.Write<byte>(_newLine);

            // Write body
            context.Connection.StreamWriter.Write<byte>(_newLine);
            context.Connection.StreamWriter.Write<byte>(body);

            return ValueTask.FromResult(true);
        }
    }
}
