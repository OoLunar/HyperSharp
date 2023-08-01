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
        public HyperServer Server { get; init; }
        public string RemoteEndPoint { get; init; }
        public PipeReader StreamReader { get; private set; }
        public PipeWriter StreamWriter { get; private set; }
        private Stream _baseStream { get; set; }

        public HyperConnection(TcpClient client, HyperServer server)
        {
            ArgumentNullException.ThrowIfNull(client);
            ArgumentNullException.ThrowIfNull(server);

            Id = Ulid.NewUlid();
            Client = client;
            Server = server;
            RemoteEndPoint = Client.Client.RemoteEndPoint?.ToString() ?? "<Unknown Network EndPoint>";
            _baseStream = Client.GetStream();
            StreamReader = PipeReader.Create(_baseStream, new StreamPipeReaderOptions(leaveOpen: true));
            StreamWriter = PipeWriter.Create(_baseStream, new StreamPipeWriterOptions(leaveOpen: true));
        }

        public HyperConnection(Stream baseStream, HyperServer server)
        {
            ArgumentNullException.ThrowIfNull(baseStream);
            ArgumentNullException.ThrowIfNull(server);

            Id = Ulid.NewUlid();
            Client = new TcpClient();
            Server = server;
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
