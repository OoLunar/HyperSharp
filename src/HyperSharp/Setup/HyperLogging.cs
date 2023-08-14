using System;
using System.Collections.Generic;
using System.Net;
using HyperSharp.Protocol;
using HyperSharp.Results;
using Microsoft.Extensions.Logging;

namespace HyperSharp.Setup
{
    internal static class HyperLogging
    {
        // Server related logs
        public static readonly Action<ILogger, IPEndPoint, Exception?> ServerStarting = LoggerMessage.Define<IPEndPoint>(LogLevel.Debug, new EventId(1, nameof(ServerStarting)), "Starting HTTP HyperServer on {ListeningEndpoint}.");
        public static readonly Action<ILogger, IPEndPoint, Exception?> ServerStarted = LoggerMessage.Define<IPEndPoint>(LogLevel.Information, new EventId(1, nameof(ServerStarted)), "Started HTTP HyperServer on {ListeningEndpoint}.");
        public static readonly Action<ILogger, IPEndPoint, Exception?> ServerStopping = LoggerMessage.Define<IPEndPoint>(LogLevel.Debug, new EventId(1, nameof(ServerStopping)), "Stopping HTTP HyperServer on {ListeningEndpoint}.");
        public static readonly Action<ILogger, IPEndPoint, Exception?> ServerStopped = LoggerMessage.Define<IPEndPoint>(LogLevel.Information, new EventId(1, nameof(ServerStopped)), "Stopped HTTP HyperServer on {ListeningEndpoint}.");

        // Connection related logs
        public static readonly Action<ILogger, int, Exception?> ConnectionsPending = LoggerMessage.Define<int>(LogLevel.Information, new EventId(2, nameof(ConnectionsPending)), "Waiting for {ConnectionCount:N0} connections to close...");
        public static readonly Action<ILogger, Ulid, Exception?> ConnectionAlreadyClosed = LoggerMessage.Define<Ulid>(LogLevel.Debug, new EventId(2, nameof(ConnectionAlreadyClosed)), "Connection {ConnectionId} is already closed.");
        public static readonly Action<ILogger, Ulid, Exception?> ConnectionClosing = LoggerMessage.Define<Ulid>(LogLevel.Trace, new EventId(2, nameof(ConnectionClosing)), "Closing connection {ConnectionId}...");
        public static readonly Action<ILogger, Ulid, Exception?> ConnectionClosed = LoggerMessage.Define<Ulid>(LogLevel.Debug, new EventId(2, nameof(ConnectionClosed)), "Connection {ConnectionId} has closed.");
        public static readonly Action<ILogger, string, Ulid, Exception?> ConnectionOpened = LoggerMessage.Define<string, Ulid>(LogLevel.Trace, new EventId(2, nameof(ConnectionOpened)), "Received connection from {RemoteEndPoint} with Id {ConnectionId}.");

        // HTTP related logs
        public static readonly Action<ILogger, Ulid, IEnumerable<Error>, Exception?> HttpInvalidHeaders = LoggerMessage.Define<Ulid, IEnumerable<Error>>(LogLevel.Warning, new EventId(3, nameof(HttpInvalidHeaders)), "Failed to parse headers from {ConnectionId}: {Error}");
        public static readonly Action<ILogger, Ulid, Uri, Exception?> HttpReceivedRequest = LoggerMessage.Define<Ulid, Uri>(LogLevel.Debug, new EventId(3, nameof(HttpReceivedRequest)), "Received request from {ConnectionId} for '{Route}'");
        public static readonly Action<ILogger, Ulid, HyperStatus, Exception?> HttpResponding = LoggerMessage.Define<Ulid, HyperStatus>(LogLevel.Trace, new EventId(3, nameof(HttpResponding)), "Responding to {ConnectionId} with {Status}");
        public static readonly Action<ILogger, Ulid, HyperStatus, Exception?> HttpResponded = LoggerMessage.Define<Ulid, HyperStatus>(LogLevel.Debug, new EventId(3, nameof(HttpResponded)), "Responded to {ConnectionId} with {Status}");

        // Responder related logs
        public static readonly Action<ILogger, Type, Type, Exception?> ResponderTypeMismatch = LoggerMessage.Define<Type, Type>(LogLevel.Trace, new EventId(4, nameof(ResponderTypeMismatch)), "Skipping responder {ResponderType} because it's type signature is not the expected {ResponderInterface}.");
        public static readonly Action<ILogger, Type, Exception?> ResponderSkippedRegistration = LoggerMessage.Define<Type>(LogLevel.Trace, new EventId(4, nameof(ResponderSkippedRegistration)), "Skipping responder {ResponderType} because it's already registered.");
    }
}
