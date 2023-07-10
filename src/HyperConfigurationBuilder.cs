using System;
using System.Collections.Generic;
using System.Net;

namespace OoLunar.HyperSharp
{
    public sealed class HyperConfigurationBuilder
    {
        public IPEndPoint ListeningEndpoint { get; set; } = new(IPAddress.Any, 8080);
        public int MaxHeaderSize { get; set; } = 8192;
        public List<Type> HeaderResponders { get; set; } = new();

        public void AddHeaderResponder<T>() where T : IHeaderResponder => HeaderResponders.Add(typeof(T));
        public bool RemoveHeaderResponder<T>() where T : IHeaderResponder => HeaderResponders.Remove(typeof(T));
    }
}
