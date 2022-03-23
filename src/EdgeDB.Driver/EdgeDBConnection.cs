using EdgeDB.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public class EdgeDBConnection
    {
        /// <summary>
        ///     Gets or sets the username used to connect to the database.
        /// </summary>
        /// <remarks>
        ///     The default edgedb username is 'edgedb'.
        /// </remarks>
        [JsonProperty("user")]
        public string? Username { get; set; }

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
        [JsonProperty("port")]
        public int Port { get; set; }

        /// <summary>
        ///     Gets or sets the database name to use when connecting.
        /// </summary>
        /// <remarks>
        ///     The default database name is 'edgedb'.
        /// </remarks>
        [JsonProperty("database")]
        public string? Database { get; set; }

        /// <summary>
        ///     Gets or sets the TLS certificate data used to very the certificate when authenticating.    
        /// </summary>
        [JsonProperty("tls_cert_data")]
        public string? TLSCertData { get; set; }

        /// <summary>
        ///     Gets or sets the TLS Certificate Authority.
        /// </summary>
        [JsonProperty("tls_ca")]
        public string? TLSCA { get; set; }

        /// <summary>
        ///     Gets or sets the TLS security level.
        /// </summary>
        /// <remarks>
        ///     The default value is 'default'.
        /// </remarks>
        [JsonProperty("tls_security")]
        public string? TLSSecurity { get; set; }

        public override string ToString()
        {
            return $"edgedb://{Username}:{Password}@{Hostname}:{Port}/{Database}";
        }

        public static EdgeDBConnection FromDSN(string dsn)
        {


            return new EdgeDBConnection { };
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

        public static EdgeDBConnection FromInstanceName(string name)
        {
            var configPath = Path.Combine(ConfigUtils.CredentialsDir, $"{name}.json");

            if (!File.Exists(configPath))
                throw new FileNotFoundException($"Config file couldn't be found at {configPath}");

            return JsonConvert.DeserializeObject<EdgeDBConnection>(File.ReadAllText(configPath))!;
        }

        internal static EdgeDBConnection ResolveConnection()
            => FromProjectFile("./edgedb.toml");
    }
}
