using EdgeDB.Utils;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Web;

namespace EdgeDB
{
    /// <summary>
    ///     Represents a class containing information on how to connect to a edgedb instance.
    /// </summary>
    public class EdgeDBConnection
    {
        private const string EDGEDB_INSTANCE_ENV_NAME = "EDGEDB_INSTANCE";
        private const string EDGEDB_DSN_ENV_NAME = "EDGEDB_DSN";
        private const string EDGEDB_CREDENTIALS_FILE_ENV_NAME = "EDGEDB_CREDENTIALS_FILE";
        private const string EDGEDB_USER_ENV_NAME = "EDGEDB_USER";
        private const string EDGEDB_PASSWORD_ENV_NAME = "EDGEDB_PASSWORD";
        private const string EDGEDB_DATABASE_ENV_NAME = "EDGEDB_DATABASE";
        private const string EDGEDB_HOST_ENV_NAME = "EDGEDB_HOST";
        private const string EDGEDB_PORT_ENV_NAME = "EDGEDB_PORT";

        #region Main connection args
        /// <summary>
        ///     Gets or sets the username used to connect to the database.
        /// </summary>
        /// <remarks>
        ///     This property defaults to edgedb
        /// </remarks>
        [JsonProperty("user")]
        public string Username
        {
            get => _user ?? "edgedb";
            set => _user = value;
        }

        /// <summary>
        ///     Gets or sets the password to connect to the database.
        /// </summary>
        [JsonProperty("password")]
        public string? Password
        {
            get => _password;
            set => _password = value;
        }

        /// <summary>
        ///     Gets or sets the hostname of the edgedb instance to connect to.
        /// </summary>
        /// <remarks>
        ///     This property defaults to 127.0.0.1.
        /// </remarks>
        public string Hostname
        {
            get => _hostname ?? "127.0.0.1";
            set
            {
                if (value.Contains('/'))
                {
                    throw new ConfigurationException("Cannot use UNIX socket for 'Hostname'");
                }

                if (value.Contains(','))
                    throw new ConfigurationException("DSN cannot contain more than one host");

                _hostname = value;
            }
        }

        /// <summary>
        ///     Gets or sets the port of the edgedb instance to connect to.    
        /// </summary>
        /// <remarks>
        ///     This property defaults to 5656
        /// </remarks>
        [JsonProperty("port")]
        public int Port
        {
            get => _port ?? 5656;
            set => _port = value;
        }

        /// <summary>
        ///     Gets or sets the database name to use when connecting.
        /// </summary>
        /// <remarks>
        ///     This property defaults to edgedb
        /// </remarks>
        [JsonProperty("database")]
        public string? Database
        {
            get => _database ?? "edgedb";
            set => _database = value;
        }

        /// <summary>
        ///     Gets or sets the TLS certificate data used to very the certificate when authenticating.    
        /// </summary>
        /// <remarks>
        ///     This value is a legacy value pre 1.0 and should not be set explicity, use <see cref="TLSCertificateAuthority"/> instead.
        /// </remarks>
        [JsonProperty("tls_cert_data")]
        [Obsolete("his value is a legacy value pre 1.0 and should not be set explicity, use TLSCertificateAuthority instead.")]
        public string? TLSCertData { get; set; }

        /// <summary>
        ///     Gets or sets the TLS Certificate Authority.
        /// </summary>
        [JsonProperty("tls_ca")]
        public string? TLSCertificateAuthority
        {
            get => _tlsca;
            set => _tlsca = value;
        }

        /// <summary>
        ///     Gets or sets the TLS security level.
        /// </summary>
        /// <remarks>
        ///     The default value is <see cref="TLSSecurityMode.Strict"/>.
        /// </remarks>
        [JsonProperty("tls_security")]
        public TLSSecurityMode TLSSecurity
        {
            get => _tlsSecurity ?? TLSSecurityMode.Strict;
            set => _tlsSecurity = value;
        }
        #endregion

        #region Backing fields
        private string? _user;
        private string? _password;
        private string? _database;
        private string? _hostname;
        private int? _port;
        private string? _tlsca;
        private TLSSecurityMode? _tlsSecurity;
        #endregion
        
        /// <summary>
        ///     Creates an <see cref="EdgeDBConnection"/> from a <see href="https://www.edgedb.com/docs/reference/dsn#dsn-specification">valid DSN</see>.
        /// </summary>
        /// <param name="dsn">The DSN to create the connection from.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">A query parameter has already been defined in the DSN.</exception>
        /// <exception cref="FormatException">Port was not in the correct format of int.</exception>
        /// <exception cref="FileNotFoundException">A file parameter wasn't found.</exception>
        /// <exception cref="KeyNotFoundException">An environment variable couldn't be found.</exception>
        public static EdgeDBConnection FromDSN(string dsn)
        {
            if (!dsn.StartsWith("edgedb://"))
                throw new ConfigurationException("DSN schema 'edgedb' expected but got 'pq'");
            
            string? database = null, username = null, port = null, host = null, password = null;

            Dictionary<string, string> args = new();

            string? proto;

            var formattedDsn = Regex.Replace(dsn, @"^([a-z]+):\/\/", x =>
            {
                proto = x.Groups[1].Value;
                return "";
            });

            var queryParams = Regex.Match(dsn, @"((?:.(?!\?))+$)");

            if (queryParams.Success)
            {
                var parsed = HttpUtility.ParseQueryString(queryParams.Groups[1].Value.Remove(0, 1));

                if (parsed.AllKeys.Length >= 1 && parsed.AllKeys[0] != null)
                {
                    args = parsed.AllKeys.ToDictionary(x => x!, x => parsed[x]!);

                    // remove args from formatted dsn
                    formattedDsn = formattedDsn.Replace(queryParams.Groups[1].Value, "");
                }
            }

            var sub1 = formattedDsn.Split('/');

            if (sub1.Length == 2)
            {
                database = sub1[1];
                formattedDsn = sub1[0];
            }

            var sub2 = formattedDsn.Split('@');

            if (sub2.Length == 2)
            {
                if (sub2[1] == "")
                {
                    // empty host/port
                    goto connectionDefinition;
                }

                if (sub2[1].Contains(','))
                    throw new ConfigurationException("DSN cannot contain more than one host");

                var right = sub2[1].Split(':');

                if (right.Length == 2)
                {
                    host = right[0];
                    port = right[1];
                }
                else
                    host = right[0];

                var left = sub2[0].Split(':');

                if (left.Length == 2)
                {
                    username = left[0];
                    password = left[1];
                }
                else
                    username = left[0];
            }
            else
            {
                var spl = sub2[0].Split(':');

                if (spl.Length == 2)
                {
                    host = spl[0];
                    port = spl[1];
                }
                else if (!string.IsNullOrEmpty(spl[0]))
                    host = spl[0];
            }

connectionDefinition:
            
            var conn = new EdgeDBConnection();

            if (database is not null)
                conn.Database = database;

            if (host is not null)
                conn.Hostname = host;

            if (username is not null)
                conn.Username = username;

            if (password is not null)
                conn.Password = password;

            if (port is not null)
            {
                if (!int.TryParse(port, out var parsedPort))
                    throw new FormatException("port was not in the correct format");

                conn.Port = parsedPort;
            }

            void SetArgument(string name, string? value, EdgeDBConnection conn)
            {
                if (string.IsNullOrEmpty(value))
                    return;

                switch (name)
                {
                    case "port":
                        {
                            if (port is not null)
                                throw new ArgumentException("Port ambiguity mismatch");

                            if (!int.TryParse(value, out var parsedPort))
                                throw new FormatException("port was not in the correct format");

                            conn.Port = parsedPort;
                        }
                        break;
                    case "host":
                        if (host is not null)
                            throw new ArgumentException("Port ambiguity mismatch");

                        conn.Hostname = value;
                        break;
                    case "database":
                        if (database is not null)
                            throw new ArgumentException("Port ambiguity mismatch");

                        conn.Database = value;
                        break;
                    case "user":
                        if (username is not null)
                            throw new ArgumentException("Port ambiguity mismatch");

                        conn.Username = value;
                        break;
                    case "password":
                        if (password is not null)
                            throw new ArgumentException("Port ambiguity mismatch");

                        conn.Password = value;
                        break;
                    case "tls_cert_file":
                        {
                            if (!File.Exists(value))
                                throw new FileNotFoundException("The specified tls_cert_file file was not found");

                            conn.TLSCertificateAuthority = File.ReadAllText(value);
                        }
                        break;
                    case "tls_security":
                        if (!Enum.TryParse<TLSSecurityMode>(value, true, out var result))
                            throw new FormatException($"\"{result}\" must be a value of TLSSecurityMode");

                        conn.TLSSecurity = result;
                        break;

                    default:
                        throw new FormatException($"Unexpected configuration option \"{name}\"");
                }
            }

            // query arguments
            foreach (var arg in args)
            {
                var fileMatch = Regex.Match(arg.Key!, @"(.*?)_file");
                var envMatch = Regex.Match(arg.Key!, @"(.*?)_env");

                if (fileMatch.Success)
                {
                    var val = File.ReadAllText(arg.Value);

                    SetArgument(fileMatch.Groups[1].Value, val, conn);
                }
                else if (envMatch.Success)
                {
                    var val = Environment.GetEnvironmentVariable(arg.Value, EnvironmentVariableTarget.Process);

                    if (val == null)
                        throw new KeyNotFoundException($"Enviroment variable \"{arg.Value}\" couldn't be found");

                    SetArgument(envMatch.Groups[1].Value, val, conn);
                }
                else
                    SetArgument(arg.Key, arg.Value, conn);
            }

            return conn;
        }

        /// <summary>
        ///     Creates a new EdgeDBConnection from a .toml project file.
        /// </summary>
        /// <param name="path">The path to the .toml project file</param>
        /// <returns>A <see cref="EdgeDBConnection"/> allowing you to connect to the projects database.</returns>
        /// <exception cref="FileNotFoundException">The supplied file path, credentials path, or instance-name file doesn't exist.</exception>
        /// <exception cref="DirectoryNotFoundException">The project directory doesn't exist for the supplied toml file.</exception>
        public static EdgeDBConnection FromProjectFile(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Couldn't find the specified project file", path);

            path = Path.GetFullPath(path);

            // get the folder name
            var dirName = Directory.GetParent(path)!.FullName;

            var projectDir = ConfigUtils.GetInstanceProjectDirectory(dirName);

            if (!Directory.Exists(projectDir))
                throw new DirectoryNotFoundException($"Couldn't find project directory for {path}: {projectDir}");

            var instanceName = File.ReadAllText(Path.Combine(projectDir, "instance-name"));

            // get credentials
            return FromInstanceName(instanceName);
        }

        /// <summary>
        ///     Creates a new <see cref="EdgeDBConnection"/> from an instance name.
        /// </summary>
        /// <param name="name">The name of the instance.</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">The instances config file couldn't be found.</exception>
        public static EdgeDBConnection FromInstanceName(string name)
        {
            var configPath = Path.Combine(ConfigUtils.GetCredentialsDir(), $"{name}.json");

            return !File.Exists(configPath)
                ? throw new FileNotFoundException($"Config file couldn't be found at {configPath}")
                : JsonConvert.DeserializeObject<EdgeDBConnection>(File.ReadAllText(configPath))!;
        }

        /// <summary>
        ///     Resolves a connection by traversing the current working directory and its parents
        ///     to find an 'edgedb.toml' file.
        /// </summary>
        /// <returns>A resolved <see cref="EdgeDBConnection"/>.</returns>
        /// <exception cref="FileNotFoundException">No 'edgedb.toml' file could be found.</exception>
        public static EdgeDBConnection ResolveEdgeDBTOML()
        {
            var dir = Environment.CurrentDirectory;

            while (true)
            {
                if (File.Exists(Path.Combine(dir!, "edgedb.toml")))
                    return FromProjectFile(Path.Combine(dir!, "edgedb.toml"));

                var parent = Directory.GetParent(dir!);

                if (parent is null || !parent.Exists)
                    throw new FileNotFoundException("Couldn't resolve edgedb.toml file");

                dir = parent.FullName;
            }
        }

        /// <summary>
        ///     Parses the provided arguments to build an <see cref="EdgeDBConnection"/> class; Parse logic follows
        ///     the <see href="https://www.edgedb.com/docs/reference/connection#ref-reference-connection-priority">Priority levels</see>
        ///     of arguments.
        /// </summary>
        /// <param name="instance">The instance name to connect to.</param>
        /// <param name="dsn">The DSN string to use to connect.</param>
        /// <param name="configure">A configuration delegate.</param>
        /// <param name="autoResolve">Whether or not to autoresolve a connection using <see cref="ResolveEdgeDBTOML"/>.</param>
        /// <returns>
        ///     A <see cref="EdgeDBConnection"/> class that can be used to connect to a EdgeDB instance.
        /// </returns>
        /// <exception cref="ConfigurationException">
        ///     An error occured while parsing or configuring the <see cref="EdgeDBConnection"/>.
        /// </exception>
        /// <exception cref="FileNotFoundException">A configuration file could not be found.</exception>
        public static EdgeDBConnection Parse(string? instance = null, string? dsn = null, Action<EdgeDBConnection>? configure = null, bool autoResolve = true)
        {
            EdgeDBConnection? connection = null;

            // try to resolve the toml
            if (autoResolve)
            {
                try
                {
                    connection = ResolveEdgeDBTOML();
                }
                catch (FileNotFoundException)
                {
                    // ignore
                }
            }

            #region Env
            var env = Environment.GetEnvironmentVariables();

            if (env.Contains(EDGEDB_INSTANCE_ENV_NAME))
            {
                var fromInst = FromInstanceName((string)env[EDGEDB_INSTANCE_ENV_NAME]!);
                connection = connection?.MergeInto(fromInst) ?? fromInst;
            }

            if (env.Contains(EDGEDB_DSN_ENV_NAME))
            {
                var fromDSN = FromDSN((string)env[EDGEDB_INSTANCE_ENV_NAME]!);
                connection = connection?.MergeInto(fromDSN) ?? fromDSN;
            }

            if (env.Contains(EDGEDB_HOST_ENV_NAME))
            {
                connection ??= new();
                try
                {
                    connection.Hostname = (string)env[EDGEDB_HOST_ENV_NAME]!;
                }
                catch (ConfigurationException x)
                {
                    switch (x.Message)
                    {
                        case "DSN cannot contain more than one host":
                            throw new ConfigurationException("Enviroment variable 'EDGEDB_HOST' cannot contain more than one host", x);
                        default:
                            throw;
                    }
                }
            }

            if (env.Contains(EDGEDB_PORT_ENV_NAME))
            {
                connection ??= new();

                if (!int.TryParse((string)env[EDGEDB_PORT_ENV_NAME]!, out var port))
                    throw new ConfigurationException($"Expected integer for environment variable '{EDGEDB_PORT_ENV_NAME}' but got '{env[EDGEDB_PORT_ENV_NAME]}'");

                connection.Port = port;
            }

            if (env.Contains(EDGEDB_CREDENTIALS_FILE_ENV_NAME))
            {
                // check if file exists
                var path = (string)env[EDGEDB_CREDENTIALS_FILE_ENV_NAME]!;
                if (!File.Exists(path))
                    throw new FileNotFoundException($"Could not find the file specified in '{EDGEDB_CREDENTIALS_FILE_ENV_NAME}'");

                var credentials = JsonConvert.DeserializeObject<EdgeDBConnection>(File.ReadAllText(path))!;
                connection = connection?.MergeInto(credentials) ?? credentials;
            }

            if (env.Contains(EDGEDB_USER_ENV_NAME))
            {
                connection ??= new();
                connection.Username = (string)env[EDGEDB_USER_ENV_NAME]!;
            }

            if (env.Contains(EDGEDB_PASSWORD_ENV_NAME))
            {
                connection ??= new();
                connection.Password = (string)env[EDGEDB_PASSWORD_ENV_NAME]!;
            }

            if (env.Contains(EDGEDB_DATABASE_ENV_NAME))
            {
                connection ??= new();
                connection.Database = (string)env[EDGEDB_DATABASE_ENV_NAME]!;
            }
            #endregion

            if (instance is not null)
            {
                var fromInst = FromInstanceName(instance);
                connection = connection?.MergeInto(fromInst) ?? fromInst;
            }    

            if (dsn is not null)
            {
                var fromDSN = FromDSN(dsn);
                connection = connection?.MergeInto(fromDSN) ?? fromDSN;
            }    
            
            if (configure is not null)
            {
                connection ??= new();

                var cloned = (EdgeDBConnection)connection.MemberwiseClone()!;
                configure(cloned);

                if (dsn is not null && cloned._hostname is not null)
                    throw new ConfigurationException("Cannot specify DSN and 'Hostname'; they are mutually exclusive");

                connection = connection.MergeInto(cloned);
            }

            return connection ?? new();
        }

        private EdgeDBConnection MergeInto(EdgeDBConnection other)
        {
            other._tlsSecurity ??= _tlsSecurity;
            other._database ??= _database;
            other._hostname ??= _hostname;
            other._password ??= _password;
            other._tlsca ??= _tlsca;
            other._port ??= _port;
            other._user ??= _user;
            return other;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var str = "edgedb://";

            if (Username is not null)
                str += Username;
            if (Password is not null && Username is not null)
                str += $":{Password}";

            if (Hostname is not null)
                str += $"@{Hostname}:{Port}";

            if (Database is not null)
                str += $"/{Database}";

            return str;
        }
    }
}
