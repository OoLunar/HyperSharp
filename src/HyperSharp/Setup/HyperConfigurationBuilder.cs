using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using HyperSharp.Responders;

namespace HyperSharp.Setup
{
    /// <summary>
    /// Represents the configuration of a HyperSharp server.
    /// </summary>
    public sealed class HyperConfigurationBuilder
    {
        /// <summary>
        /// The maximum size of each header, name and value combined.
        /// </summary>
        public int MaxHeaderSize { get; set; } = 8192;

        /// <summary>
        /// The value of the Server header.
        /// </summary>
        public string ServerName { get; set; } = "HyperSharp";

        /// <summary>
        /// The name of the JSON serializer options to grab from the service collection.
        /// </summary>
        public string JsonSerializerOptionsName { get; set; } = "HyperSharp";

        /// <summary>
        /// The default timeout for a request.
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Which IP address and port to listen on.
        /// </summary>
        public IPEndPoint ListeningEndpoint { get; set; } = new(IPAddress.Any, 8080);

        /// <summary>
        /// A list of types which will be executed when a new HTTP request is received.
        /// </summary>
        /// <remarks>
        /// All types implement <see cref="IResponderBase"/>.
        /// </remarks>
        public List<Type> Responders { get; set; } = new();

        /// <summary>
        /// Adds all responders in the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly to add responders from.</param>
        public void AddResponders(Assembly assembly) => Responders.AddRange(assembly.GetTypes());

        /// <summary>
        /// Adds all responders from the specified types.
        /// </summary>
        /// <param name="responders">The types to search.</param>
        public void AddResponders(IEnumerable<Type> responders) => Responders.AddRange(responders);

        /// <summary>
        /// Adds all responders from the specified types.
        /// </summary>
        /// <param name="responders">The types to search.</param>
        public void AddResponders(params Type[] responders) => Responders.AddRange(responders);

        /// <summary>
        /// Adds the specified responder.
        /// </summary>
        /// <typeparam name="T">The type of the responder.</typeparam>
        public void AddResponder<T>() where T : IResponderBase => Responders.Add(typeof(T));
    }
}
