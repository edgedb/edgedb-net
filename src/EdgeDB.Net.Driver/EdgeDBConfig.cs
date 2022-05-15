using Microsoft.Extensions.Logging;

namespace EdgeDB
{
    public class EdgeDBClientPoolConfig : EdgeDBConfig
    {
        /// <summary>
        ///     Gets or sets the client type the pool will use.
        /// </summary>
        public EdgeDBClientType ClientType { get; set; }
    }

    public enum EdgeDBClientType
    {
        /// <summary>
        ///     The client pool will use <see cref="EdgeDBTcpClient"/>s
        /// </summary>
        Tcp,

        /// <summary>
        ///     The client pool will use <see cref="EdgeDBHttpClient"/>s
        /// </summary>
        Http
    }

    /// <summary>
    ///     Represents the configuration options for a <see cref="EdgeDBClient"/> or <see cref="EdgeDBTcpClient"/>
    /// </summary>
    public class EdgeDBConfig
    {
        /// <summary>
        ///     Gets or sets the default client pool size.
        /// </summary>
        public int DefaultPoolSize { get; set; } = 50;

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
    }
}
