using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using HyperSharp.Responders;

namespace HyperSharp.Setup
{
    public sealed class HyperConfigurationBuilder
    {
        public int MaxHeaderSize { get; set; } = 8192;
        public string ServerName { get; set; } = "HyperSharp";
        public string JsonSerializerOptionsName { get; set; } = "HyperSharp";
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
        public IPEndPoint ListeningEndpoint { get; set; } = new(IPAddress.Any, 8080);
        public List<Type> Responders { get; set; } = new();

        public void AddResponders(Assembly assembly) => Responders.AddRange(assembly.GetTypes());
        public void AddResponders(IEnumerable<Type> responders) => Responders.AddRange(responders);
        public void AddResponders(params Type[] responders) => Responders.AddRange(responders);
        public void AddResponder<T>() where T : IResponderBase => Responders.Add(typeof(T));
    }
}
