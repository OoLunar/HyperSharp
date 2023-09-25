using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace HyperSharp.Protocol
{
    /// <summary>
    /// Represents a client connection to the Hyper server.
    /// </summary>
    public sealed record HyperConnection : IAsyncDisposable
    {
        /// <summary>
        /// The unique identifier of the connection, containing the timestamp of when the connection was created.
        /// </summary>
        public Ulid Id { get; init; }

        /// <summary>
        /// The server that the connection is intended for.
        /// </summary>
        public HyperServer Server { get; init; }

        /// <summary>
        /// A pipe reader for the connection.
        /// </summary>
        public PipeReader StreamReader { get; private set; }

        /// <summary>
        /// A pipe writer for the connection.
        /// </summary>
        public PipeWriter StreamWriter { get; private set; }

        /// <summary>
        /// The base stream of the connection.
        /// TODO: Application stream (Compression), protocol stream (SSL)
        /// </summary>
        private Stream _baseStream { get; set; }

        private bool _isDisposed;

        /// <summary>
        /// Creates a new mock client connection to the Hyper server.
        /// </summary>
        /// <param name="baseStream">The base stream of the connection.</param>
        /// <param name="server">The server that the connection is intended for.</param>
        public HyperConnection(Stream baseStream, HyperServer server)
        {
            ArgumentNullException.ThrowIfNull(baseStream);
            ArgumentNullException.ThrowIfNull(server);

            Id = Ulid.NewUlid();
            Server = server;
            _baseStream = baseStream;
            StreamReader = PipeReader.Create(_baseStream, new StreamPipeReaderOptions(leaveOpen: true));
            StreamWriter = PipeWriter.Create(_baseStream, new StreamPipeWriterOptions(leaveOpen: true));
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (_isDisposed)
            {
                return;
            }

            await StreamWriter.CompleteAsync();
            await StreamReader.CompleteAsync();
            await _baseStream.DisposeAsync();
            _isDisposed = true;
        }
    }
}
