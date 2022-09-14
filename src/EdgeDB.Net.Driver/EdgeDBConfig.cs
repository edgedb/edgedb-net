using EdgeDB.Binary;
using EdgeDB.ContractResolvers;
using EdgeDB.State;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Numerics;

namespace EdgeDB
{
    /// <summary>
    ///     Represents a config for a <see cref="EdgeDBClient"/>, extending <see cref="EdgeDBConfig"/>.
    /// </summary>
    public class EdgeDBClientPoolConfig : EdgeDBConfig
    {
        /// <summary>
        ///     Gets or sets the default client pool size.
        /// </summary>
        public int DefaultPoolSize
        {
            get => _poolSize;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException($"{nameof(DefaultPoolSize)} must be greater than 0");
                _poolSize = value;
            }
        }

        /// <summary>
        ///     Gets or sets the client type the pool will use.
        /// </summary>
        internal EdgeDBClientType ClientType { get; set; }

        /// <summary>
        ///     Gets or sets the client factory to use when adding new clients to the client pool.
        /// </summary>
        /// <remarks>
        ///     The <see cref="ClientType"/> must be <see cref="EdgeDBClientType.Custom"/> to use this property.
        /// </remarks>
        internal Func<ulong, EdgeDBConnection, EdgeDBConfig, ValueTask<BaseEdgeDBClient>>? ClientFactory { get; set; }

        private int _poolSize = 50;
    }

    /// <summary>
    ///     Represents different client types used in a <see cref="EdgeDBClient"/>.
    /// </summary>
    public enum EdgeDBClientType
    {
        /// <summary>
        ///     The client pool will use <see cref="EdgeDBTcpClient"/>s
        /// </summary>
        Tcp,

        /// <summary>
        ///     The client pool will use <see cref="EdgeDBHttpClient"/>s
        /// </summary>
        Http,

        /// <summary>
        ///     The client pool will use unix domain sockets to connect.
        /// </summary>
        [Obsolete("EdgeDB servers no longer support unix domain sockets.", true)]
        Unix,

        /// <summary>
        ///     The client pool will use the <see cref="EdgeDBClientPoolConfig.ClientFactory"/> to add new clients.
        /// </summary>
        Custom
    }

    /// <summary>
    ///     Represents the configuration options for a <see cref="EdgeDBClient"/> or <see cref="EdgeDBTcpClient"/>
    /// </summary>
    public class EdgeDBConfig
    {
        /// <summary>
        ///     Gets the <see cref="JsonSerializer"/> capable of serializing/deserializing edgedb types.
        /// </summary>
        public static readonly JsonSerializer JsonSerializer = new JsonSerializer()
        {
            ContractResolver = new EdgeDBContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
        };

        /// <summary>
        ///     Gets or sets the logger used for logging messages from the driver.
        /// </summary>
        public ILogger? Logger { get; set; }

        /// <summary>
        ///     Gets or sets the retry mode for connecting new clients.
        /// </summary>
        public ConnectionRetryMode RetryMode { get; set; }

        /// <summary>
        ///     Gets or sets the maximum number of times to retry to connect.
        /// </summary>
        public uint MaxConnectionRetries { get; set; } = 5;

        /// <summary>
        ///     Gets or sets the number of miliseconds a client will wait for a connection to be 
        ///     established with the server.
        /// </summary>
        public uint ConnectionTimeout { get; set; } = 5000;

        /// <summary>
        ///     Gets or sets the max amount of miliseconds a client will wait for an expected message.
        /// </summary>
        public uint MessageTimeout { get; set; } = 5000;

        /// <summary>
        ///     Gets or sets whether or not to always return object ids.
        /// </summary>
        /// <remarks>
        ///     If set to <see langword="true"/> returned objects will not have an implicit id property i.e. query 
        ///     shapes will have to explicitly list id properties.
        /// </remarks>
        public bool ExplicitObjectIds { get; set; }
        
        /// <summary>
        ///     Gets or sets the implicit object limit for all queries. By default there is not limit.
        /// </summary>
        public ulong ImplicitLimit { get; set; }

        /// <summary>
        ///     Gets or sets the default naming strategy for the type builder.
        /// </summary>
        /// <remarks>
        ///     See <seealso cref="TypeBuilder.NamingStrategy"/>
        /// </remarks>
        public INamingStrategy SerializerNamingStrategy
        {
            get => TypeBuilder.NamingStrategy;
            set => TypeBuilder.NamingStrategy = value;
        }
    }
}
