using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Toolkit.HighPerformance;

namespace HyperSharp.Protocol
{
    /// <summary>
    /// Holds a collection of static methods implementing <see cref="HyperSerializerDelegate"/> for the most common of Content-Types.
    /// </summary>
    public static partial class HyperSerializers
    {
        private static readonly byte[] _newLine = "\r\n"u8.ToArray();
        private static readonly byte[] _contentTypeTextEncodingHeader = "Content-Type: text/plain; charset=utf-8\r\nContent-Length: "u8.ToArray();

        /// <summary>
        /// Serializes the body to the client as plain text using the <see cref="object.ToString"/> method with the <see cref="Encoding.UTF8"/> encoding.
        /// </summary>
        /// <inheritdoc cref="HyperSerializerDelegate"/>
        public static ValueTask<bool> PlainTextAsync(HyperContext context, HyperStatus status, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(status);

            // Write Content-Type header and beginning of Content-Length header
            context.Connection.StreamWriter.Write<byte>(_contentTypeTextEncodingHeader);

            byte[] body = Encoding.UTF8.GetBytes(status.Body?.ToString() ?? "");

            // Write Content-Length header
            context.Connection.StreamWriter.Write<byte>(Encoding.ASCII.GetBytes(body.Length.ToString()));
            context.Connection.StreamWriter.Write<byte>(_newLine);

            // Write body
            context.Connection.StreamWriter.Write<byte>(_newLine);
            context.Connection.StreamWriter.Write<byte>(body);

            return ValueTask.FromResult(true);
        }
    }
}
