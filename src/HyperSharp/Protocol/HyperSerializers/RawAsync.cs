using System;
using System.Globalization;
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
        private static readonly byte[] _contentLengthHeader = "Content-Length: "u8.ToArray();

        /// <summary>
        /// Serializes the body to the client as plain text using the <see cref="object.ToString"/> method with the <see cref="Encoding.UTF8"/> encoding.
        /// </summary>
        /// <inheritdoc cref="HyperSerializerDelegate"/>
        public static ValueTask<bool> RawAsync(HyperContext context, HyperStatus status, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(status);

            byte[] body = status.Body as byte[] ?? Encoding.UTF8.GetBytes(status.Body?.ToString() ?? "");
            int bodyLength = body.Length;

            // Write Content-Length header
            context.Connection.StreamWriter.Write<byte>(_contentLengthHeader);
            context.Connection.StreamWriter.Write<byte>(Encoding.UTF8.GetBytes(bodyLength.ToString(CultureInfo.InvariantCulture)));
            context.Connection.StreamWriter.Write<byte>(_newLine);

            // Write body
            context.Connection.StreamWriter.Write<byte>(_newLine);
            context.Connection.StreamWriter.Write<byte>(body);

            return ValueTask.FromResult(true);
        }
    }
}
