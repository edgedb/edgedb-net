using EdgeDB.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents a class used to interact with EdgeDB.
    /// </summary>
    public class EdgeDB
    {
        private readonly EdgeDBConnection _connection;
        private readonly EdgeDBConfig _config;
#pragma warning disable IDE0052 // Remove unread private members
        private readonly ConcurrentQueue<EdgeDBTcpClient> _clients;
#pragma warning restore IDE0052 // Remove unread private members

        /// <summary>
        ///     Creates a new instance of a EdgeDB client pool allowing you to execute commands.
        /// </summary>
        /// <remarks>
        ///     This constructor uses the default config and will attempt to find your EdgeDB project toml file in the current working directory. If 
        ///     no file is found this method will throw a <see cref="FileNotFoundException"/>.
        /// </remarks>
        public EdgeDB() : this(EdgeDBConnection.ResolveConnection(), new EdgeDBConfig()) { }

        /// <summary>
        ///     Creates a new instance of a EdgeDB client pool allowing you to execute commands.
        /// </summary>
        /// <remarks>
        ///     This constructor will attempt to find your EdgeDB project toml file in the current working directory. If 
        ///     no file is found this method will throw a <see cref="FileNotFoundException"/>.
        /// </remarks>
        /// <param name="config">The config for this client pool.</param>
        public EdgeDB(EdgeDBConfig config) : this(EdgeDBConnection.ResolveConnection(), config) { }

        /// <summary>
        ///     Creates a new instance of a EdgeDB client pool allowing you to execute commands.
        /// </summary>
        /// <param name="connection">The connection parameters used to create new clients.</param>
        public EdgeDB(EdgeDBConnection connection) : this(connection, new EdgeDBConfig()) { }

        /// <summary>
        ///     Creates a new instance of a EdgeDB client pool allowing you to execute commands.
        /// </summary>
        /// <param name="connection">The connection parameters used to create new clients.</param>
        /// <param name="config">The config for this client pool.</param>
        public EdgeDB(EdgeDBConnection connection, EdgeDBConfig config)
        {
            _config = config;
            _connection = connection;
            _clients = new();
        }

        //internal async Task<ExecuteResult<TResult>?> ExecuteAsync<TResult>(string query, IDictionary<string, object?> arguments, Cardinality cardinality)
        //{
        //    _clients
        //}


    }
}
