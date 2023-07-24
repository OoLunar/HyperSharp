using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace OoLunar.HyperSharp
{
    public sealed class HyperConfigurationBuilder
    {
        public IPEndPoint ListeningEndpoint { get; set; } = new(IPAddress.Any, 8080);
        public int MaxHeaderSize { get; set; } = 8192;
        public List<Type> Responders { get; set; } = new();
        public string JsonSerializerOptionsName { get; set; } = "HyperSharp";

        public void AddResponders(Assembly assembly) => Responders.AddRange(assembly.GetTypes());
    }
}
