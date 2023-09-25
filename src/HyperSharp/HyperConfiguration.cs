using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HyperSharp.Protocol;
using HyperSharp.Responders;
using HyperSharp.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace HyperSharp
{
    /// <summary>
    /// Represents the configuration of a HyperSharp server.
    /// </summary>
    public sealed record HyperConfiguration
    {
        /// <summary>
        /// The default server name to use for the Server header.
        /// </summary>
        public string ServerName { get; init; }

        /// <summary>
        /// The maximum size of each header, name and value combined.
        /// </summary>
        public int MaxHeaderSize { get; init; }

        /// <summary>
        /// The timeout for a request.
        /// </summary>
        public TimeSpan Timeout { get; init; }

        /// <summary>
        /// The endpoint to listen on.
        /// </summary>
        public IPEndPoint ListeningEndpoint { get; init; }

        /// <summary>
        /// The default JSON serializer options to use for <see cref="HyperContext.RespondAsync(HyperStatus, JsonSerializerOptions?, CancellationToken)"/>.
        /// </summary>
        public JsonSerializerOptions JsonSerializerOptions { get; init; }

        /// <summary>
        /// The delegate to invoke when a request is received. This is set by the <see cref="ResponderCompiler"/>.
        /// </summary>
        public ValueTaskResponderDelegate<HyperContext, HyperStatus> RespondersDelegate { get; init; }

        internal byte[] _serverNameBytes { get; init; }
        internal Uri _host { get; init; }

        /// <summary>
        /// Creates a new <see cref="HyperConfiguration"/> with the default values.
        /// </summary>
        public HyperConfiguration() : this(new ServiceCollection().AddLogging(builder => builder.AddProvider(NullLoggerProvider.Instance)).AddHyperSharp(), new HyperConfigurationBuilder()) { }

        /// <summary>
        /// Creates a new <see cref="HyperConfiguration"/> with the specified values.
        /// </summary>
        /// <param name="builder">The <see cref="HyperConfigurationBuilder"/> to use.</param>
        public HyperConfiguration(HyperConfigurationBuilder builder) : this(new ServiceCollection().AddLogging(builder => builder.AddProvider(NullLoggerProvider.Instance)).AddHyperSharp(), builder) { }

        /// <summary>
        /// Creates a new <see cref="HyperConfiguration"/> using the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="serviceDescriptors">The <see cref="IServiceCollection"/> to use when creating the responders.</param>
        public HyperConfiguration(IServiceCollection serviceDescriptors) : this(serviceDescriptors, new HyperConfigurationBuilder()) { }

        /// <summary>
        /// Creates a new <see cref="HyperConfiguration"/> using the specified <see cref="IServiceCollection"/> and <see cref="HyperConfigurationBuilder"/>.
        /// </summary>
        /// <param name="serviceDescriptors">The <see cref="IServiceCollection"/> to use when creating the responders.</param>
        /// <param name="builder">The <see cref="HyperConfigurationBuilder"/> to use.</param>
        public HyperConfiguration(IServiceCollection serviceDescriptors, HyperConfigurationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(serviceDescriptors, nameof(serviceDescriptors));
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));
            IServiceProvider serviceProvider = serviceDescriptors.BuildServiceProvider();

            if (!Uri.TryCreate($"http://{builder.ListeningEndpoint}/", UriKind.Absolute, out Uri? host))
            {
                throw new ArgumentException("The listening endpoint is invalid.", nameof(builder));
            }
            else if (!HyperHeaderCollection.IsValidName(ServerName))
            {
                throw new ArgumentException("The server name is invalid.", nameof(builder));
            }

            _host = host;
            _serverNameBytes = Encoding.ASCII.GetBytes(builder.ServerName);
            ServerName = builder.ServerName;
            MaxHeaderSize = builder.MaxHeaderSize;
            Timeout = builder.Timeout;
            ListeningEndpoint = builder.ListeningEndpoint;
            JsonSerializerOptions = serviceProvider.GetService<IOptionsSnapshot<JsonSerializerOptions>>()?.Get(builder.JsonSerializerOptionsName) ?? HyperJsonSerializationOptions.Default;

            ResponderCompiler responderCompiler = serviceProvider.GetRequiredService<ResponderCompiler>();
            responderCompiler.Search(builder.Responders);

            if (!responderCompiler.IsSynchronous())
            {
                RespondersDelegate = responderCompiler.CompileAsyncResponders<HyperContext, HyperStatus>(serviceProvider);
                return;
            }

            ResponderDelegate<HyperContext, HyperStatus> synchronousResponders = responderCompiler.CompileResponders<HyperContext, HyperStatus>(serviceProvider);
            RespondersDelegate = (HyperContext context, CancellationToken cancellationToken) => ValueTask.FromResult(synchronousResponders(context, cancellationToken));
        }
    }
}
