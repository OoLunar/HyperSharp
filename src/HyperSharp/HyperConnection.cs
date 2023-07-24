using System;
using System.IO.Pipelines;
using System.Net.Sockets;

namespace OoLunar.HyperSharp
{
    public sealed record HyperConnection
    {
        public Ulid Id { get; init; }
        public TcpClient Client { get; init; }
        public string RemoteEndPoint { get; init; }
        // Set in HyperServer, always non-null when the user receives the context.
        public PipeReader StreamReader { get; internal set; } = null!;
        public PipeWriter StreamWriter { get; internal set; } = null!;

        public HyperConnection(TcpClient client)
        {
            ArgumentNullException.ThrowIfNull(client);

            Id = Ulid.NewUlid();
            Client = client;
            RemoteEndPoint = Client.Client.RemoteEndPoint?.ToString() ?? "<Unknown Network EndPoint>";
        }
    }
}
