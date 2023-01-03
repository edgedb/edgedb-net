using CommandLine;
using EdgeDB.CLI.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.CLI.Arguments
{
    public class ConnectionArguments : LogArgs
    {
        [Option("dsn", HelpText = "DSN for EdgeDB to connect to (overrides all other options except password)")]
        public string? DSN { get; set; }

        [Option("credentials-file", HelpText = "Path to JSON file to read credentials from")]
        public string? CredentialsFile { get; set; }

        [Option('I', "instance", HelpText = "Local instance name created with edgedb instance create to connect to (overrides host and port)")]
        public string? Instance { get; set; }

        [Option('H', "host", HelpText = "Host of the EdgeDB instance")]
        public string? Host { get; set; }

        [Option('P', "port", HelpText = "Port to connect to EdgeDB")]
        public int? Port { get; set; }

        [Option('d', "database", HelpText = "Database name to connect to")]
        public string? Database { get; set; }

        [Option('u', "user", HelpText = "User name of the EdgeDB user")]
        public string? User { get; set; }

        [Option("password", HelpText = "Ask for password on the terminal (TTY)")]
        public bool Password { get; set; }

        [Option("password-from-stdin", HelpText = "Read the password from stdin rather than TTY (useful for scripts)")]
        public bool PasswordFromSTDIN { get; set; }

        [Option("tls-ca-file", HelpText = "Certificate to match server against\n\nThis might either be full self-signed server certificate or certificate authority (CA) certificate that server certificate is signed with.")]
        public string? TLSCAFile { get; set; }

        [Option("tls-security", HelpText = "Specify the client-side TLS security mode.")]
        public TLSSecurityMode? TLSSecurity { get; set; }

        [Option("raw-connection", Hidden = true)]
        public string? ConnectionJson { get; set; }

        public EdgeDBConnection GetConnection()
        {
            if (ConnectionJson is not null)
                return JsonConvert.DeserializeObject<EdgeDBConnection>(ConnectionJson) ?? throw new Exception("Cannot decode connection args");

            if (DSN is not null)
                return EdgeDBConnection.FromDSN(DSN);

            if (Instance is not null)
                return EdgeDBConnection.FromInstanceName(Instance);

            if (CredentialsFile is not null)
                return JsonConvert.DeserializeObject<EdgeDBConnection>(File.ReadAllText(CredentialsFile)) 
                    ?? throw new NullReferenceException($"The file '{CredentialsFile}' didn't contain a valid credential definition");

            // create the resolved connection
            var resolved = EdgeDBConnection.ResolveEdgeDBTOML();

            if (Host is not null)
                resolved.Hostname = Host;

            if (Port.HasValue)
                resolved.Port = Port.Value;

            if (Database is not null)
                resolved.Database = Database;

            if (User is not null)
                resolved.Username = User;

            if (Password)
            {
                // read password from console
                Console.Write($"Password for '{resolved.Database}': ");

                resolved.Password = ConsoleUtils.ReadSecretInput();
            }

            if (PasswordFromSTDIN)
                resolved.Password = Console.ReadLine();

            if (TLSCAFile is not null)
                resolved.TLSCertificateAuthority = TLSCAFile;

            if (TLSSecurity.HasValue)
                resolved.TLSSecurity = TLSSecurity.Value;

            return resolved;
        }
    }
}
