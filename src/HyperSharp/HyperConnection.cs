using System;
using System.IO;
using System.IO.Pipelines;
using System.Net.Sockets;

namespace OoLunar.HyperSharp
{
    public sealed record HyperConnection
    {
        public Ulid Id { get; init; }
        public TcpClient Client { get; init; }
        public string RemoteEndPoint { get; init; }
        public PipeReader StreamReader { get; private set; }
        public PipeWriter StreamWriter { get; private set; }
        private Stream _baseStream { get; set; }

        public HyperConnection(TcpClient client)
        {
            ArgumentNullException.ThrowIfNull(client);

            Id = Ulid.NewUlid();
            Client = client;
            RemoteEndPoint = Client.Client.RemoteEndPoint?.ToString() ?? "<Unknown Network EndPoint>";
            _baseStream = Client.GetStream();
            StreamReader = PipeReader.Create(_baseStream, new StreamPipeReaderOptions(leaveOpen: true));
            StreamWriter = PipeWriter.Create(_baseStream, new StreamPipeWriterOptions(leaveOpen: true));
        }

        public HyperConnection(Stream baseStream)
        {
            ArgumentNullException.ThrowIfNull(baseStream);

            Id = Ulid.NewUlid();
            Client = new TcpClient();
            RemoteEndPoint = "<Unknown Network EndPoint>";
            _baseStream = baseStream;
            StreamReader = PipeReader.Create(_baseStream, new StreamPipeReaderOptions(leaveOpen: true));
            StreamWriter = PipeWriter.Create(_baseStream, new StreamPipeWriterOptions(leaveOpen: true));
        }

        public void ApplyStreamLayer(Func<Stream, Stream> applyNewStreamLayer)
        {
            _baseStream = applyNewStreamLayer(_baseStream);
            StreamReader = PipeReader.Create(_baseStream, new StreamPipeReaderOptions(leaveOpen: true));
            StreamWriter = PipeWriter.Create(_baseStream, new StreamPipeWriterOptions(leaveOpen: true));
        }
    }
}
