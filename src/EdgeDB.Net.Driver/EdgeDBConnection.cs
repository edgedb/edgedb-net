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
        /// <summary>
        ///     Gets or sets the username used to connect to the database.
        /// </summary>
        /// <remarks>
        ///     This property defaults to edgedb
        /// </remarks>
        [JsonProperty("user")]
        public string? Username { get; set; } = "edgedb";

        /// <summary>
        ///     Gets or sets the password to connect to the database.
        /// </summary>
        [JsonProperty("password")]
        public string? Password { get; set; }

        /// <summary>
        ///     Gets or sets the hostname of the edgedb instance to connect to.
        /// </summary>
        /// <remarks>
        ///     This property defaults to 127.0.0.1.
        /// </remarks>
        public string? Hostname { get; set; } = "127.0.0.1";

        /// <summary>
        ///     Gets or sets the port of the edgedb instance to connect to.    
        /// </summary>
        /// <remarks>
        ///     This property defaults to 5656
        /// </remarks>
        [JsonProperty("port")]
        public int Port { get; set; } = 5656;

        /// <summary>
        ///     Gets or sets the database name to use when connecting.
        /// </summary>
        /// <remarks>
        ///     This property defaults to edgedb
        /// </remarks>
        [JsonProperty("database")]
        public string? Database { get; set; } = "edgedb";

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
        public string? TLSCertificateAuthority { get; set; }

        /// <summary>
        ///     Gets or sets the TLS security level.
        /// </summary>
        /// <remarks>
        ///     The default value is <see cref="TLSSecurityMode.Strict"/>.
        /// </remarks>
        [JsonProperty("tls_security")]
        public TLSSecurityMode TLSSecurity { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            string str = "edgedb://";

            if (Username != null)
                str += Username;
            if (Password != null && Username != null)
                str += $":{Password}";

            if (Hostname != null)
                str += $"@{Hostname}:{Port}";

            if (Database != null)
                str += $"/{Database}";

            return str;
        }

        /// <summary>
        ///     Creates an <see cref="EdgeDBConnection"/> from a <see href="https://www.edgedb.com/docs/reference/dsn#dsn-specification">valid DSN</see>.
        /// </summary>
        /// <param name="dsn">The DSN to create the connection from.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">A query parameter has already been defined in the DSN.</exception>
        /// <exception cref="FormatException">Port was not in the correct format of int.</exception>
        /// <exception cref="FileNotFoundException">A file parameter wasn't found.</exception>
        /// <exception cref="KeyNotFoundException">An enviorment variable couldn't be found.</exception>
        public static EdgeDBConnection FromDSN(string dsn)
        {
            if (!Regex.IsMatch(dsn, @"^[a-z]+:\/\/"))
                throw new ArgumentException("DSN doesn't match the format \"edgedb://\"");

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

            var conn = new EdgeDBConnection();

            if (database != null)
                conn.Database = database;

            if (host != null)
                conn.Hostname = host;

            if (username != null)
                conn.Username = username;

            if (password != null)
                conn.Password = password;

            if (port != null)
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
                            if (port != null)
                                throw new ArgumentException("Port ambiguity mismatch");

                            if (!int.TryParse(value, out var parsedPort))
                                throw new FormatException("port was not in the correct format");

                            conn.Port = parsedPort;
                        }
                        break;
                    case "host":
                        if (host != null)
                            throw new ArgumentException("Port ambiguity mismatch");

                        conn.Hostname = value;
                        break;
                    case "database":
                        if (database != null)
                            throw new ArgumentException("Port ambiguity mismatch");

                        conn.Database = value;
                        break;
                    case "user":
                        if (username != null)
                            throw new ArgumentException("Port ambiguity mismatch");

                        conn.Username = value;
                        break;
                    case "password":
                        if (password != null)
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
            var configPath = Path.Combine(ConfigUtils.CredentialsDir, $"{name}.json");

            return !File.Exists(configPath)
                ? throw new FileNotFoundException($"Config file couldn't be found at {configPath}")
                : JsonConvert.DeserializeObject<EdgeDBConnection>(File.ReadAllText(configPath))!;
        }

        /// <summary>
        ///     Resolves a connection by traversing the current working directory and its parents.
        /// </summary>
        /// <returns>A resolved <see cref="EdgeDBConnection"/>.</returns>
        /// <exception cref="FileNotFoundException">No config file could be found.</exception>
        public static EdgeDBConnection ResolveConnection()
        {
            string dir = Environment.CurrentDirectory;

            while (true)
            {
                if (File.Exists(Path.Combine(dir!, "edgedb.toml")))
                    return FromProjectFile(Path.Combine(dir!, "edgedb.toml"));

                var parent = Directory.GetParent(dir!);

                if (parent == null || !parent.Exists)
                    throw new FileNotFoundException("Couldn't resolve edgedb.toml file");

                dir = parent.FullName;
            }
        }
    }
}
