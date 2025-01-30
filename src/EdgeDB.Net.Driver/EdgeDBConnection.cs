using EdgeDB.Abstractions;
using EdgeDB.Utils;
using Newtonsoft.Json;
using System.Collections;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace EdgeDB;

/// <summary>
///     A json readable representation of an EdgeDBConnection.
///     When using Credentials to create an EdgeDBConnection, the data must conform to this type.
/// </summary>
internal class ConnectionCredentials
{
    [JsonProperty("host")]
    public string? Host { get; init; }

    [JsonProperty("port")]
    [JsonConverter(typeof(AsStringConverter))]
    public string? Port { get; init; }

    [JsonProperty("database")]
    public string? Database { get; init; }

    [JsonProperty("branch")]
    public string? Branch { get; init; }

    [JsonProperty("user")]
    public string? User { get; init; }

    [JsonProperty("password")]
    public string? Password { get; init; }

    [JsonProperty("tls_ca")]
    public string? TlsCA { get; init; }

    [JsonProperty("tls_security")]
    [JsonConverter(typeof(TLSSecurityModeParser))]
    public TLSSecurityMode? TlsSecurity { get; init; }
}


/// <summary>
///     Represents a class containing information on how to connect to a edgedb instance.
/// </summary>
public sealed class EdgeDBConnection
{
    private const string INSTANCE_ENV_NAME = "INSTANCE";
    private const string DSN_ENV_NAME = "DSN";
    private const string CREDENTIALS_FILE_ENV_NAME = "CREDENTIALS_FILE";
    private const string HOST_ENV_NAME = "HOST";
    private const string PORT_ENV_NAME = "PORT";
    private const string DATABASE_ENV_NAME = "DATABASE";
    private const string BRANCH_ENV_NAME = "BRANCH";
    private const string USER_ENV_NAME = "USER";
    private const string PASSWORD_ENV_NAME = "PASSWORD";
    private const string SECRET_KEY_ENV_NAME = "SECRET_KEY";
    private const string TLS_CA_ENV_NAME = "TLS_CA";
    private const string CLIENT_SECURITY_ENV_NAME = "CLIENT_SECURITY";
    private const string CLIENT_TLS_SECURITY_ENV_NAME = "CLIENT_TLS_SECURITY";
    private const string TLS_SERVER_NAME_ENV_NAME = "TLS_SERVER_NAME";
    private const string WAIT_UNTIL_AVAILABLE_ENV_NAME = "WAIT_UNTIL_AVAILABLE";
    private const string CLOUD_PROFILE_ENV_NAME = "CLOUD_PROFILE";
    private const int DOMAIN_NAME_MAX_LEN = 62;

    private EdgeDBConnection MergeInto(EdgeDBConnection other)
    {
        other._hostname ??= _hostname;
        other._port ??= _port;
        if (other._branch is null && other._database is null)
        {
            if (_branch is not null)
            {
                other._branch = _branch;
            }
            else if (_database is not null)
            {
                other._database = _database;
            }
        }
        other.Password ??= Password;
        other._user ??= _user;
        other._password ??= _password;
        other.TLSCertificateAuthority ??= TLSCertificateAuthority;
        other._tlsSecurity ??= _tlsSecurity;
        return other;
    }

    internal bool ValidateServerCertificateCallback(object sender, X509Certificate? certificate, X509Chain? chain,
        SslPolicyErrors sslPolicyErrors)
    {
        if (TLSSecurity is TLSSecurityMode.Insecure)
            return true;

        if (TLSCertificateAuthority is not null)
        {
            var cert = this.GetCertificate()!;

            X509Chain chain2 = new();
            chain2.ChainPolicy.ExtraStore.Add(cert);
            chain2.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
            chain2.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

            var isValid = chain2.Build(new X509Certificate2(certificate!));
            var chainRoot = chain2.ChainElements[^1].Certificate;
            isValid = isValid && chainRoot.RawData.SequenceEqual(cert.RawData);

            return isValid;
        }

        return sslPolicyErrors is SslPolicyErrors.None;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var str = "gel://";

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

    #region Main connection args

    /// <summary>
    ///     Gets or sets the hostname of the edgedb instance to connect to.
    /// </summary>
    /// <remarks>
    ///     This property defaults to localhost.
    /// </remarks>
    public string Hostname
    {
        get => _hostname ?? "localhost";
        set => _hostname = value;
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
    ///     This property defaults to <c>edgedb</c>.  It is mutually exclusive with <see cref="Branch"/>.
    /// </remarks>
    /// <exception cref="InvalidOperationException"><see cref="Branch"/> already contains a value; they're mutually exclusive</exception>
    [JsonProperty("database")]
    public string? Database
    {
        get => _database ?? _branch ?? _defaultDatabase;
        set
        {
            if (_branch is not null)
            {
                _branch = null;
            }

            if (value == _defaultDatabase)
            {
                _database = null;
            }
            else
            {
                _database = value;
            }
        }
    }
    private static readonly string _defaultDatabase = "edgedb";

    /// <summary>
    ///     Gets or sets the branch name to use when connecting.
    /// </summary>
    /// <remarks>
    ///     This property defaults to <c>__default__</c>. It is mutually exclusive with <see cref="Database"/>
    /// </remarks>
    /// <exception cref="InvalidOperationException"><see cref="Database"/> already contains a value; they're mutually exclusive</exception>
    [JsonProperty("branch")]
    public string? Branch
    {
        get => _database ?? _branch ?? _defaultBranch;
        set
        {
            if (_database is not null)
            {
                _database = null;
            }

            if (value == _defaultBranch)
            {
                _branch = null;
            }
            else
            {
                _branch = value;
            }
        }
    }
    private static readonly string _defaultBranch = "__default__";

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
    public string? Password {
        get => _password ?? "";
        set => _password = value;
    }

    /// <summary>
    ///     Gets or sets the secret key used to authenticate with cloud instances.
    /// </summary>
    public string? SecretKey { get; set; }

    /// <summary>
    ///     Gets or sets the TLS Certificate Authority.
    /// </summary>
    [JsonProperty("tls_ca")]
    public string? TLSCertificateAuthority { get; set; }

    /// <summary>
    ///     Gets or sets the TLS security level.
    /// </summary>
    /// <remarks>
    ///     The default value is <see cref="TLSSecurityMode.Strict" />.
    /// </remarks>
    [JsonProperty("tls_security")]
    public TLSSecurityMode TLSSecurity
    {
        get => _tlsSecurity ?? TLSSecurityMode.Strict;
        set => _tlsSecurity = value;
    }

    /// <summary>
    ///     Gets or sets the TLS server name to be used.
    /// </summary>
    /// <remarks>
    ///     Overrides the value provided by Hostname.
    /// </remarks>
    [JsonProperty("tls_server_name")]
    public string? TLSServerName { get; set; }

    /// <summary>
    ///     Gets or sets the number of miliseconds a client will wait for a connection to be
    ///     established with the server.
    /// </summary>
    [JsonProperty("wait_until_available")]
    public int WaitUntilAvailable
    {
        get => _waitUntilAvailable ?? 30000;
        set => _waitUntilAvailable = value;
    }

    /// <summary>
    ///     Gets or sets the name of the cloud profile to use to resolve the <see cref="SecretKey" />.
    /// </summary>
    /// <remarks>
    ///     The default cloud profile is called 'default'
    /// </remarks>
    public string CloudProfile
    {
        get => _cloudProfile ?? _defaultCloudProfile;
        set
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value), "Cloud Profile must not be null");
            }

            _cloudProfile = value;
        }
    }
    private static readonly string _defaultCloudProfile = "default";

    /// <summary>
    ///     Additional settings for the server connection.
    /// </summary>
    /// <remarks>
    ///     This currently has no effect.
    /// </remarks>
    public Dictionary<string, string> ServerSettings { get; set; } = new();

    #endregion

    #region Backing fields

    private string? _hostname;
    private int? _port;
    private string? _database;
    private string? _branch;
    private string? _user;
    private string? _password;
    private TLSSecurityMode? _tlsSecurity;
    private int? _waitUntilAvailable;
    private string? _cloudProfile;

    #endregion

    #region Create Function

    /// <summary>
    ///     Optional args which can be passed into <see cref="Create"/>.
    /// </summary>
    public class Options
    {
        // Primary args
        // These can set host/port of the connection.
        public string? Instance { get; set; }
        public string? Dsn { get; set; }
        public string? Host { get; set; }
        public int? Port { get; set; }

        // Secondary args
        public string? Database { get; set; }
        public string? Branch { get; set; }
        public string? User { get; set; }
        public string? Password { get; set; }
        public string? SecretKey { get; set; }
        public string? Credentials { get; set; }
        public string? CredentialsFile { get; set; }
        public string? TLSCertificateAuthority { get; set; }
        public string? TLSCertificateAuthorityFile { get; set; }
        public TLSSecurityMode? TLSSecurity { get; set; }
        public string? TLSServerName { get; set; }
        public string? WaitUntilAvailable { get; set; }
        public Dictionary<string, string>? ServerSettings { get; set; }

        public bool IsEmpty =>
            Instance is null
            && Dsn is null
            && Host is null
            && Port is null
            && Database is null
            && Branch is null
            && User is null
            && Password is null
            && SecretKey is null
            && Credentials is null
            && CredentialsFile is null
            && TLSCertificateAuthority is null
            && TLSCertificateAuthorityFile is null
            && TLSSecurity is null
            && TLSServerName is null
            && WaitUntilAvailable is null
            && ServerSettings is null;
    }

    /// <summary>
    ///     Parses the `gel.toml`, optional <see cref="Options"/>, and environment variables to build an
    ///     <see cref="EdgeDBConnection" />.
    /// 
    ///     This function will first search for the first valid primary args (which can set host/port)
    ///     in the following order:
    ///     - <see cref="Options"/>
    ///     - Environment variables
    ///     - `gel.toml` file
    /// 
    ///     It will then apply any secondary args from the environment variables and options.
    /// 
    ///     If any primary <see cref="Options"/> are present, then all environment variables are ignored.
    /// 
    ///     See the <see href="https://www.edgedb.com/docs/reference/connection">documentation</see>
    ///     for more information.
    /// </summary>
    /// <param name="options">Options used to build the <see cref="EdgeDBConnection" />.</param>
    /// <returns>
    ///     A <see cref="EdgeDBConnection" /> class that can be used to connect to a EdgeDB instance.
    /// </returns>
    /// <exception cref="ConfigurationException">
    ///     An error occured while parsing or configuring the <see cref="EdgeDBConnection" />.
    /// </exception>
    public static EdgeDBConnection Create(Options? options = null)
    {
        return _Create(options ?? new(), null);
    }

    internal static EdgeDBConnection _Create(Options options, ISystemProvider? platform)
    {
        platform ??= ConfigUtils.DefaultPlatformProvider;

        ConfigUtils.ResolvedFields resolvedFields = new();

        #region Primary Options

        // First, check primary options
        // If any primary options are present, environment variables are ignored

        bool hasPrimaryOptions = false;
        {
            // These options can set host/port and should be resolved first
            // More than one primary options should raise an error

            Exception primaryError = new ConfigurationException(
                "Connection options cannot have more than one of the following "
                + "values: \"Instance\", \"Dsn\", \"Credentials\", "
                + "\"CredentialsFile\" or \"Host\"/\"Port\"");
            // The primaryError has priority, so hold on to any other exception
            // until all primary options are processed.
            Exception? deferredPrimaryError = null;

            if (options.Instance is not null)
            {
                if (hasPrimaryOptions) { throw primaryError; }
                if (options.Instance != "")
                {
                    try
                    {
                        var fromDSN = _FromInstanceName(options.Instance, null, platform);
                        resolvedFields.MergeFrom(fromDSN);
                    }
                    catch (Exception e)
                    {
                        deferredPrimaryError = e;
                    }
                }
                else
                {
                    deferredPrimaryError = new ConfigurationException(
                        $"Invalid instance name: \"{options.Instance}\"");
                }
                hasPrimaryOptions = true;
            }

            if (options.Dsn is not null)
            {
                if (hasPrimaryOptions) { throw primaryError; }
                var fromDSN = _FromDSN(options.Dsn, platform);
                resolvedFields.MergeFrom(fromDSN);
                hasPrimaryOptions = true;
            }

            {
                string? credentialsText = null;
                if (options.Credentials is not null)
                {
                    if (hasPrimaryOptions) { throw primaryError; }
                    credentialsText = options.Credentials;
                    hasPrimaryOptions = true;
                }
                if (options.CredentialsFile is not null)
                {
                    if (hasPrimaryOptions) { throw primaryError; }
                    if (platform.FileExists(options.CredentialsFile))
                    {
                        credentialsText = platform.FileReadAllText(options.CredentialsFile) ?? "{}";
                    }
                    else
                    {
                        deferredPrimaryError = new ConfigurationException(
                            $"Invalid CredentialsFile: \"{options.CredentialsFile}\", could not find file");
                    }
                    hasPrimaryOptions = true;
                }
                if (credentialsText is not null)
                {
                    try
                    {
                        ConnectionCredentials? credentials =
                            new JsonSerializer().DeserializeObject<ConnectionCredentials>(credentialsText);
                        if (credentials is not null)
                        {
                            resolvedFields.MergeFrom(ConfigUtils.ResolvedFields.FromCredentials(credentials));
                        }
                    }
                    catch (JsonException)
                    {
                        deferredPrimaryError = new ConfigurationException("Invalid Credentials: could not parse json");
                    }
                }
            }

            {
                bool hasHostOrPort = false;
                if (options.Host is not null)
                {
                    if (hasPrimaryOptions) { throw primaryError; }
                    resolvedFields.Host = options.Host;
                    hasHostOrPort = true;
                }
                if (options.Port is not null)
                {
                    if (hasPrimaryOptions) { throw primaryError; }
                    resolvedFields.Port = options.Port;
                    hasHostOrPort = true;
                }
                if (hasHostOrPort)
                {
                    hasPrimaryOptions = true;
                }
            }

            if (deferredPrimaryError is not null)
            {
                throw deferredPrimaryError;
            }
        }

        #endregion

        #region Primary Env

        var envName = string.Empty;
        var envVar = string.Empty;

        bool hasPrimaryEnv = false;

        if (!hasPrimaryOptions)
        {
            // These env vars can set host/port and should be resolved first
            // More than one primary env var should raise an error

            Exception primaryError = new ConfigurationException(
                "Cannot have more than one of the following connection "
                + "environment variables: \"GEL_DSN\", \"GEL_INSTANCE\", "
                + "\"GEL_CREDENTIALS_FILE\" or \"GEL_HOST\"/\"GEL_PORT\"");
            // The primaryError has priority, so hold on to any other exception
            // until all primary env vars are processed.
            Exception? deferredPrimaryError = null;

            if (platform.GetGelEnvVariable(INSTANCE_ENV_NAME, out envName, out envVar))
            {
                if (hasPrimaryEnv) { throw primaryError; }
                try
                {
                    var fromInst = _FromInstanceName(envVar, null, platform);
                    resolvedFields.MergeFrom(fromInst);
                }
                catch (Exception e)
                {
                    deferredPrimaryError = e;
                }
                hasPrimaryEnv = true;
            }

            if (platform.GetGelEnvVariable(DSN_ENV_NAME, out envName, out envVar))
            {
                if (hasPrimaryEnv) { throw primaryError; }
                var fromDSN = _FromDSN(envVar, platform);
                resolvedFields.MergeFrom(fromDSN);
                hasPrimaryEnv = true;
            }

            if (platform.GetGelEnvVariable(CREDENTIALS_FILE_ENV_NAME, out envName, out envVar))
            {
                if (hasPrimaryEnv) { throw primaryError; }
                if (platform.FileExists(envVar))
                {
                    var credentials =
                        JsonConvert.DeserializeObject<ConnectionCredentials>(platform.FileReadAllText(envVar))!;
                    resolvedFields.MergeFrom(ConfigUtils.ResolvedFields.FromCredentials(credentials));
                }
                else
                {
                    deferredPrimaryError = new FileNotFoundException(
                        $"Invalid credential file from {envName}: \"{envVar}\", could not find file");
                }
                hasPrimaryEnv = true;
            }

            {
                bool hasHostOrPort = false;
                if (platform.GetGelEnvVariable(HOST_ENV_NAME, out envName, out envVar))
                {
                    if (hasPrimaryEnv) { throw primaryError; }
                    resolvedFields.Host = envVar;
                    hasHostOrPort = true;
                }
                if (platform.GetGelEnvVariable(PORT_ENV_NAME, out envName, out envVar))
                {
                    ConfigUtils.ResolvedField<int>? port = ConfigUtils.ParsePort(envVar);
                    if (port is not null)
                    {
                        if (hasPrimaryEnv) { throw primaryError; }
                        resolvedFields.Port = ConfigUtils.MergeField(resolvedFields.Port, port);
                        hasHostOrPort = true;
                    }
                }
                if (hasHostOrPort)
                {
                    hasPrimaryEnv = true;
                }
            }

            if (deferredPrimaryError is not null)
            {
                throw deferredPrimaryError;
            }
        }

        #endregion

        #region Toml File

        if (!hasPrimaryOptions && !hasPrimaryEnv)
        {
            ConfigUtils.ResolvedFields? fromToml = _ResolveEdgeDBTOML(platform);
            if (fromToml is not null)
            {
                resolvedFields.MergeFrom(fromToml);
            }
        }

        #endregion

        #region Secondary Env

        if (!hasPrimaryOptions)
        {
            if (platform.GetGelEnvVariable(DATABASE_ENV_NAME, out envName, out envVar))
            {
                var altName = string.Empty;
                var altVal = string.Empty;
                if (platform.GetGelEnvVariable(BRANCH_ENV_NAME, out altName, out altVal))
                {
                    throw new ConfigurationException(
                        $"Environment variables {envName} and {altName} are mutually exclusive");
                }

                resolvedFields.DatabaseOrBranch = new ConfigUtils.DatabaseOrBranch.DatabaseName(envVar);
            }

            if (platform.GetGelEnvVariable(BRANCH_ENV_NAME, out envName, out envVar))
            {
                resolvedFields.DatabaseOrBranch = new ConfigUtils.DatabaseOrBranch.BranchName(envVar);
            }

            if (platform.GetGelEnvVariable(USER_ENV_NAME, out envName, out envVar))
            {
                resolvedFields.User = envVar;
            }

            if (platform.GetGelEnvVariable(PASSWORD_ENV_NAME, out envName, out envVar))
            {
                resolvedFields.Password = envVar;
            }

            if (platform.GetGelEnvVariable(TLS_CA_ENV_NAME, out envName, out envVar))
            {
                resolvedFields.TLSCertificateAuthority = envVar;
            }

            {
                string clientSecurityEnvName;
                string clientTlsSecurityEnvName;
                TLSSecurityMode? clientSecurity = null;
                TLSSecurityMode? clientTlsSecurity = null;
                bool hasDefault = false;
                if (platform.GetGelEnvVariable(CLIENT_SECURITY_ENV_NAME, out clientSecurityEnvName, out envVar))
                {
                    if (TLSSecurityModeParser.TryParse(envVar, true, out clientSecurity))
                    {
                        if (clientSecurity is not null)
                        {
                            resolvedFields.TLSSecurity = clientSecurity;
                        }
                        else
                        {
                            hasDefault = true;
                        }
                    }
                    else
                    {
                        resolvedFields.TLSSecurity = new ConfigurationException(
                            $"Invalid TLS Security from {clientSecurityEnvName}: \"{envVar}\"");
                    }
                }
                if (platform.GetGelEnvVariable(CLIENT_TLS_SECURITY_ENV_NAME, out clientTlsSecurityEnvName, out envVar))
                {
                    if (TLSSecurityModeParser.TryParse(envVar, true, out clientTlsSecurity))
                    {
                        if (clientTlsSecurity is null)
                        {
                            hasDefault = true;
                        }
                        else if (clientSecurity is null)
                        {
                            // overwrite default value
                            resolvedFields.TLSSecurity = clientTlsSecurity.Value;
                        }
                        else if (clientSecurity == TLSSecurityMode.Strict
                            && clientTlsSecurity != TLSSecurityMode.Strict)
                        {
                            throw new ConfigurationException(
                                $"{clientSecurityEnvName}=strict but {clientTlsSecurityEnvName}={envVar}. "
                                + $"{clientTlsSecurityEnvName} must be strict when {clientSecurityEnvName} "
                                + $"is strict"
                            );
                        }
                        else
                        {
                            // overwrite existing value
                            resolvedFields.TLSSecurity = clientTlsSecurity.Value;
                        }
                    }
                    else
                    {
                        resolvedFields.TLSSecurity = new ConfigurationException(
                            $"Invalid TLS Security from {clientTlsSecurityEnvName}: \"{envVar}\"");
                    }
                }
                if (hasDefault)
                {
                    // finally, apply default value if no non-default value or error present
                    resolvedFields.TLSSecurity ??= TLSSecurityMode.Default;
                }
            }

            if (platform.GetGelEnvVariable(TLS_SERVER_NAME_ENV_NAME, out envName, out envVar))
            {
                resolvedFields.TLSServerName = envVar;
            }

            if (platform.GetGelEnvVariable(WAIT_UNTIL_AVAILABLE_ENV_NAME, out envName, out envVar))
            {
                resolvedFields.WaitUntilAvailable = ConfigUtils.ParseWaitUntilAvailable(envVar);
            }
        }

        #endregion

        #region Secondary Options

        // Finally, check secondary options
        // Secondary options should override environment variables

        if (options.Database is not null && options.Branch is not null)
        {
            throw new ConfigurationException("Invalid options: Database and Branch are mutually exclusive.");
        }
        else if (options.Database is not null)
        {
            resolvedFields.DatabaseOrBranch = new ConfigUtils.DatabaseOrBranch.DatabaseName(options.Database);
        }
        else if (options.Branch is not null)
        {
            resolvedFields.DatabaseOrBranch = new ConfigUtils.DatabaseOrBranch.BranchName(options.Branch);
        }

        if (options.User is not null) { resolvedFields.User = options.User; }
        if (options.Password is not null) { resolvedFields.Password = options.Password; }
        if (options.SecretKey is not null) { resolvedFields.SecretKey = options.SecretKey; }
        if (options.TLSCertificateAuthority is not null)
        {
            resolvedFields.TLSCertificateAuthority = options.TLSCertificateAuthority;
        }
        if (options.TLSCertificateAuthorityFile is not null)
        {
            if (platform.FileExists(options.TLSCertificateAuthorityFile))
            {
                resolvedFields.TLSCertificateAuthority =
                    platform.FileReadAllText(options.TLSCertificateAuthorityFile);
            }
            else
            {
                throw new ConfigurationException(
                    $"Invalid TLSCertificateAuthorityFile: \"{options.TLSCertificateAuthorityFile}\", could not find file");
            }
        }
        if (options.TLSSecurity is not null) { resolvedFields.TLSSecurity = options.TLSSecurity; }
        if (options.TLSServerName is not null) { resolvedFields.TLSServerName = options.TLSServerName; }
        if (options.WaitUntilAvailable is not null)
        {
            resolvedFields.WaitUntilAvailable = ConfigUtils.MergeField(
                resolvedFields.WaitUntilAvailable,
                ConfigUtils.ParseWaitUntilAvailable(options.WaitUntilAvailable));
        }
        if (options.ServerSettings is not null)
        {
            foreach (KeyValuePair<string,string> entry in options.ServerSettings)
            {
                resolvedFields.ServerSettings = ConfigUtils.AddServerSettingField(
                    resolvedFields.ServerSettings, entry.Key, entry.Value);
            }
        }

        #endregion

        if (options.IsEmpty && resolvedFields.IsEmpty)
        {
            throw new ConfigurationException("No `gel.toml` found and no connection options specified.");
        }

        return _FromResolvedFields(resolvedFields, platform);
    }

    #endregion

    #region Create Helpers

    internal static EdgeDBConnection _FromResolvedFields(ConfigUtils.ResolvedFields resolvedFields, ISystemProvider? platform)
    {
        platform ??= ConfigUtils.DefaultPlatformProvider;

        if (ConfigUtils.TryGetFieldValue(resolvedFields.Host, out string host))
        {
            if (host.Contains(','))
            {
                throw new ConfigurationException(
                    $"Invalid host: \"{host}\", DSN cannot contain more than one host");
            }
            if (host == "")
            {
                throw new ConfigurationException($"Invalid host: \"{host}\"");
            }
            if (host.StartsWith("/"))
            {
                throw new ConfigurationException($"Invalid host: \"{host}\", unix socket paths not supported");
            }
        }
        if (ConfigUtils.TryGetFieldValue(resolvedFields.Port, out int port))
        {
            if (port < 1 || 65535 < port)
            {
                throw new ConfigurationException($"Invalid port: \"{port}\", must be between 1 and 65535");
            }
        }
        if (ConfigUtils.TryGetFieldValue(resolvedFields.DatabaseOrBranch, out ConfigUtils.DatabaseOrBranch databaseOrBranch))
        {
            if (databaseOrBranch.Value == "")
            {
                throw databaseOrBranch switch
                {
                    ConfigUtils.DatabaseOrBranch.DatabaseName name => new ConfigurationException(
                        $"Invalid database name: \"{name.Value}\""),
                    ConfigUtils.DatabaseOrBranch.BranchName name => new ConfigurationException(
                        $"Invalid branch name: \"{name.Value}\""),
                    _ => new ConfigurationException("Invalid database or branch name"),
                };
            }
        }
        if (ConfigUtils.TryGetFieldValue(resolvedFields.User, out string user))
        {
            if (user == "")
            {
                throw new ConfigurationException($"Invalid user: \"{user}\"");
            }
        }

        return new()
        {
            _hostname = resolvedFields.Host?.CheckAndGetValue(),
            _port = resolvedFields.Port?.CheckAndGetValue(),
            _database = (
                resolvedFields.DatabaseOrBranch?.Value switch
                {
                    ConfigUtils.DatabaseOrBranch.DatabaseName name => name.Value,
                    _ => null,
                }
            ),
            _branch = (
                resolvedFields.DatabaseOrBranch?.Value switch
                {
                    ConfigUtils.DatabaseOrBranch.BranchName name => name.Value,
                    _ => null,
                }
            ),
            _user = resolvedFields.User?.CheckAndGetValue(),
            _password = resolvedFields.Password?.CheckAndGetValue(),
            SecretKey = resolvedFields.SecretKey?.CheckAndGetValue(),
            TLSCertificateAuthority = resolvedFields.TLSCertificateAuthority?.CheckAndGetValue(),
            _tlsSecurity = resolvedFields.TLSSecurity?.CheckAndGetValue(),
            TLSServerName = resolvedFields.TLSServerName?.CheckAndGetValue(),
            _waitUntilAvailable = resolvedFields.WaitUntilAvailable?.CheckAndGetValue(),
            ServerSettings = ConfigUtils.CheckAndGetServerSettings(resolvedFields.ServerSettings),
        };
    }

    /// <summary>
    ///     Creates an <see cref="EdgeDBConnection" /> from a
    ///     <see href="https://www.edgedb.com/docs/reference/dsn#dsn-specification">valid DSN</see>.
    /// </summary>
    /// <param name="dsn">The DSN to create the connection from.</param>
    /// <returns>A <see cref="EdgeDBConnection" /> representing the DSN.</returns>
    /// <exception cref="ArgumentException">A query parameter has already been defined in the DSN.</exception>
    /// <exception cref="FormatException">Port was not in the correct format of int.</exception>
    /// <exception cref="FileNotFoundException">A file parameter wasn't found.</exception>
    /// <exception cref="KeyNotFoundException">An environment variable couldn't be found.</exception>
    public static EdgeDBConnection FromDSN(string dsn)
    {
        return _FromResolvedFields(_FromDSN(dsn, null), null);
    }

    static private readonly Regex _dsnRegex = new(
        @"^(?:(?:edgedb|gel|(?<invalid_scheme>\w+))://)"
        + @"(?:"
            + @"(?:"
                + @"(?<user>[^@/?:,]+)(?::(?<password>[^@/?:,]+))?@"
                + @"|(?<invalid_user>[^@/?]+)@"
                + @")?"
            + @"(?:"
                + @"(?:(?<host>[^@/?:]+)|\[(?<host>[^\[\]]+)\])"
                    + @"(?::(?<port>[^@/?:,]+))?"
                + @"|(?<invalid_host>[^@/?]+)"
                + @")"
            + @")?"
        + @"(?:"
            + @"/(?<branch>[^@/?:,]*(?:/[^@/?:,]+)*)"
            + @"|/(?<invalid_branch>[^/?]+)"
            + @")?"
        + @"(?:\?(?<params>.*))?"
        + @"$",
        RegexOptions.Compiled
    );
    static private readonly Regex _dsnParamsRegex = new(
        @"^(?<entry>[^=&]+(?:=[^=&]*)?)(?:&(?<entry>[^=&]+(?:=[^=&]*)?))*$"
    );

    internal static ConfigUtils.ResolvedFields _FromDSN(string dsn, ISystemProvider? platform)
    {
        platform ??= ConfigUtils.DefaultPlatformProvider;

        Match dsnMatch = _dsnRegex.Match(Uri.UnescapeDataString(dsn));
        if (!dsnMatch.Success)
        {
            throw new ConfigurationException($"Invalid DSN: \"{dsn}\"");
        }
        if (dsnMatch.Groups["invalid_scheme"].Success)
        {
            string scheme = dsnMatch.Groups["invalid_scheme"].Value;
            throw new ConfigurationException(
                $"Invalid DSN scheme. Expected \"gel\" but got \"{scheme}\"");
        }
        if (dsnMatch.Groups["invalid_user"].Success)
        {
            throw new ConfigurationException($"Invalid DSN: Could not parse user/password");
        }
        if (dsnMatch.Groups["invalid_host"].Success)
        {
            throw new ConfigurationException($"Invalid DSN: Could not parse host/port");
        }
        if (dsnMatch.Groups["invalid_branch"].Success)
        {
            throw new ConfigurationException($"Invalid DSN: Could not parse branch");
        }

        string? GetMatchGroupOrNull(string groupName)
        {
            return dsnMatch.Groups[groupName].Success ? dsnMatch.Groups[groupName].Value : null;
        }

        string? username = GetMatchGroupOrNull("user");
        string? password = GetMatchGroupOrNull("password");
        string? port = GetMatchGroupOrNull("port");
        string? host = GetMatchGroupOrNull("host");
        string? branch = GetMatchGroupOrNull("branch");

        // Check that a param is not used twice in the dsn
        HashSet<string> usedParamNames = new();
        if (branch is not null && branch != "") usedParamNames.Add("branch");
        if (host is not null && host != "") usedParamNames.Add("host");
        if (username is not null && username != "") usedParamNames.Add("user");
        if (password is not null && password != "") usedParamNames.Add("password");
        if (port is not null && port != "") usedParamNames.Add("port");

        // Parse query params
        Dictionary<string, string> args = new();
        if (dsnMatch.Groups["params"].Success)
        {
            Match paramsMatch = _dsnParamsRegex.Match(dsnMatch.Groups["params"].Value);
            if (!paramsMatch.Success)
            {
                throw new ConfigurationException("Invalid DSN: could not parse query parameters");
            }

            foreach (Capture capture in paramsMatch.Groups["entry"].Captures)
            {
                string[] entry = capture.Value.Split('=');
                if (entry.Length == 2)
                {
                    if (args.ContainsKey(entry[0]))
                    {
                        throw new ConfigurationException($"Invalid DSN: dupliate query parameter \"{entry[0]}\"");
                    }

                    string paramName = entry[0];
                    if (paramName.EndsWith("_env")) paramName = paramName.Substring(0, paramName.Length - "_env".Length);
                    if (paramName.EndsWith("_file")) paramName = paramName.Substring(0, paramName.Length - "_file".Length);

                    if (usedParamNames.Contains(paramName))
                    {
                        throw new ConfigurationException(
                            $"Invalid DSN: more than one of "
                            + $"\"{paramName}\", "
                            + $"\"?{paramName}=\", \"?{paramName}_env=\", \"?{paramName}_file=\" "
                            + $"was specified.");
                    }

                    args[entry[0]] = entry[1];
                    usedParamNames.Add(paramName);
                }
                else
                {
                    if (entry[0] == "port")
                    {
                        throw new ConfigurationException("Invalid port in dsn query parameters");
                    }
                    else if (entry[0] == "database")
                    {
                        throw new ConfigurationException("Invalid database in dsn query parameters");
                    }
                    else if (entry[0] == "branch")
                    {
                        throw new ConfigurationException("Invalid branch in dsn query parameters");
                    }
                    else if (entry[0] == "tls_security")
                    {
                        throw new ConfigurationException("Invalid TLS Security in dsn query parameters");
                    }
                }
            }
        }

        var resolvedFields = new ConfigUtils.ResolvedFields();

        if (host is not null) { resolvedFields.Host = host; }
        if (port is not null)
        {
            if (!int.TryParse(port, out var parsedPort))
                throw new ConfigurationException("Invalid DSN: port was not in the correct format");

            resolvedFields.Port = parsedPort;
        }
        if (branch is not null && branch != "")
        {
            resolvedFields.DatabaseOrBranch = new ConfigUtils.DatabaseOrBranch.BranchName(branch);
        }
        if (username is not null) { resolvedFields.User = username; }
        if (password is not null) { resolvedFields.Password = password; }

        if (args.Any(x => x.Key.StartsWith("branch", StringComparison.InvariantCultureIgnoreCase))
            && args.Any(x => x.Key.StartsWith("database", StringComparison.InvariantCultureIgnoreCase)))
        {
            throw new ConfigurationException("Invalid DSN: branch and database are mutually exclusive");
        }

        // Resolve query arguments
        foreach (var arg in args)
        {
            string key = arg.Key;
            ConfigUtils.ResolvedField<string> value = arg.Value;

            if (key.EndsWith("_env") && ConfigUtils.TryGetFieldValue(value, out string envName))
            {
                string oldKey = key;
                key = key.Substring(0, key.Length - "_env".Length);
                string? envVar = platform.GetEnvVariable(envName);
                if (envVar is not null)
                {
                    value = envVar;
                }
                else
                {
                    value = new ConfigurationException(
                        $"Invalid DSN query parameter: \"{oldKey}\", environment variable \"{envName}\" doesn\'t exist");
                }
            }

            if (key.EndsWith("_file") && ConfigUtils.TryGetFieldValue(value, out string fileName))
            {
                string oldKey = key;
                key = key.Substring(0, key.Length - "_file".Length);
                if (platform.FileExists(fileName))
                {
                    value = platform.FileReadAllText(fileName);
                }
                else
                {
                    throw new ConfigurationException(
                        $"Invalid DSN query parameter: \"{oldKey}\" could not find file \"{fileName}\"");
                }
            }

            if (value is null) { continue; }

            switch (key)
            {
                case "host":
                    resolvedFields.Host = value;
                    break;
                case "port":
                    resolvedFields.Port = value.Convert(ConfigUtils.ParsePort);
                    break;
                case "database":
                    resolvedFields.DatabaseOrBranch = value.Convert<ConfigUtils.DatabaseOrBranch>(v =>
                    {
                        if (v.StartsWith("/"))
                        {
                            v = v.Substring(1);
                        }
                        if (v != "")
                        {
                            return new ConfigUtils.DatabaseOrBranch.DatabaseName(v);
                        }
                        return null;
                    });
                    break;
                case "branch":
                    resolvedFields.DatabaseOrBranch = value.Convert<ConfigUtils.DatabaseOrBranch>(v =>
                    {
                        if (v.StartsWith("/"))
                        {
                            v = v.Substring(1);
                        }
                        if (v != "")
                        {
                            return new ConfigUtils.DatabaseOrBranch.BranchName(v);
                        }
                        return null;
                    });
                    break;
                case "user":
                    resolvedFields.User = value;
                    break;
                case "password":
                    resolvedFields.Password = value;
                    break;
                case "secret_key":
                    resolvedFields.SecretKey = value;
                    break;
                case "tls_cert_file":
                    resolvedFields.TLSCertificateAuthority = value.Convert<string>(v =>
                    {
                        if (platform.FileExists(v))
                        {
                            return platform.FileReadAllText(v);
                        }
                        else
                        {
                            return new FileNotFoundException("The specified tls_cert_file file was not found");
                        }
                    });
                    break;
                case "tls_server_name":
                    resolvedFields.TLSServerName = value;
                    break;
                case "tls_security":
                    resolvedFields.TLSSecurity = value.Convert<TLSSecurityMode>(v =>
                    {
                        try
                        {
                            return TLSSecurityModeParser.Parse(v);
                        }
                        catch (Exception e)
                        {
                            return e;
                        }
                    });
                    break;
                case "wait_until_available":
                    resolvedFields.WaitUntilAvailable = value.Convert(ConfigUtils.ParseWaitUntilAvailable);
                    break;

                default:
                    resolvedFields.ServerSettings =
                        ConfigUtils.AddServerSettingField(resolvedFields.ServerSettings, key, value);
                    break;
            }
        }

        return resolvedFields;
    }

    /// <summary>
    ///     Creates a new EdgeDBConnection from a .toml project file.
    /// </summary>
    /// <param name="path">The path to the .toml project file.</param>
    /// <returns>A <see cref="EdgeDBConnection" /> representing the project defined in the .toml file.</returns>
    /// <exception cref="FileNotFoundException">The supplied file path, credentials path, or instance-name file doesn't exist.</exception>
    /// <exception cref="DirectoryNotFoundException">The project directory doesn't exist for the supplied toml file.</exception>
    public static EdgeDBConnection FromProjectFile(string path)
    {
        return _FromResolvedFields(_FromProjectFile(path, null), null);
    }

    internal static ConfigUtils.ResolvedFields _FromProjectFile(string path, ISystemProvider? platform)
    {
        platform ??= ConfigUtils.DefaultPlatformProvider;

        if (!platform.FileExists(path))
            throw new FileNotFoundException("Couldn't find the specified project file", path);

        path = platform.GetFullPath(path);

        // get the folder name
        var dirName = platform.DirectoryGetParent(path)!.FullName;

        var projectDir = ConfigUtils.GetInstanceProjectDirectory(dirName, platform);

        if (!platform.DirectoryExists(projectDir))
            throw new DirectoryNotFoundException($"Couldn't find project directory for {path}: {projectDir}");

        if (!ConfigUtils.TryResolveInstanceCloudProfile(projectDir, out var profile, out var inst, platform) || inst is null)
            throw new FileNotFoundException($"Could not find instance name under project directory {projectDir}");

        var resolvedFields = _FromInstanceName(inst, profile, platform);

        if (ConfigUtils.TryResolveProjectDatabase(projectDir, out var database, platform) && database is not null)
        {
            resolvedFields.DatabaseOrBranch = new ConfigUtils.DatabaseOrBranch.DatabaseName(database);
        }

        return resolvedFields;
    }

    /// <summary>
    ///     Creates a new <see cref="EdgeDBConnection" /> from an instance name.
    /// </summary>
    /// <remarks>
    ///     This method supports both local instances and cloud instances. Environment
    ///     variables will not be applied to the returned <see cref="EdgeDBConnection" />,
    ///     instead, use <see cref="Parse(string?, string?, Action{EdgeDBConnection}?, bool)" /> to
    ///     apply environment variables.
    /// </remarks>
    /// <param name="name">The name of the instance.</param>
    /// <param name="cloudProfile">The optional cloud profile if the instance name is a cloud instance.</param>
    /// <returns>A <see cref="EdgeDBConnection" /> containing connection details for the specific instance.</returns>
    /// <exception cref="FileNotFoundException">The instances config file couldn't be found.</exception>
    /// <exception cref="ConfigurationException">The configuration is invalid.</exception>
    public static EdgeDBConnection FromInstanceName(string name, string? cloudProfile = null)
    {
        return _FromResolvedFields(_FromInstanceName(name, cloudProfile, null), null);
    }

    internal static ConfigUtils.ResolvedFields _FromInstanceName(string name, string? cloudProfile, ISystemProvider? platform)
    {
        platform ??= ConfigUtils.DefaultPlatformProvider;

        if (Regex.IsMatch(name, @"^\w(-?\w)*$"))
        {
            var configPath = platform.CombinePaths(ConfigUtils.GetCredentialsDir(platform), $"{name}.json");

            if (!platform.FileExists(configPath))
            {
                throw new FileNotFoundException($"Config file couldn't be found at {configPath}");
            }

            ConnectionCredentials credentials = JsonConvert.DeserializeObject<ConnectionCredentials>(
                platform.FileReadAllText(configPath))!;

            return ConfigUtils.ResolvedFields.FromCredentials(credentials);
        }

        if (Regex.IsMatch(name, @"^([A-Za-z0-9](-?[A-Za-z0-9])*)\/([A-Za-z0-9](-?[A-Za-z0-9])*)$"))
        {
            return ParseCloudInstanceName(name, null, cloudProfile, platform);
        }

        throw new ConfigurationException($"Invalid instance name '{name}'");
    }

    /// <summary>
    ///     Resolves a connection by traversing the current working directory and its parents
    ///     to find an 'edgedb.toml' file.
    /// </summary>
    /// <returns>A resolved <see cref="EdgeDBConnection" />.</returns>
    /// <exception cref="FileNotFoundException">No 'edgedb.toml' file could be found.</exception>
    public static EdgeDBConnection ResolveEdgeDBTOML()
    {
        ConfigUtils.ResolvedFields? resolvedFields = _ResolveEdgeDBTOML(null);
        if (resolvedFields is null)
        {
            throw new ConfigurationException("Couldn't resolve gel.toml file");
        }
        return _FromResolvedFields(resolvedFields, null);
    }

    internal static ConfigUtils.ResolvedFields? _ResolveEdgeDBTOML(ISystemProvider? platform)
    {
        platform ??= ConfigUtils.DefaultPlatformProvider;

        var dir = platform.GetCurrentDirectory();

        while (true)
        {
            if (platform.FileExists(platform.CombinePaths(dir!, "gel.toml")))
                return _FromProjectFile(platform.CombinePaths(dir!, "gel.toml"), platform);

            if (platform.FileExists(platform.CombinePaths(dir!, "edgedb.toml")))
                return _FromProjectFile(platform.CombinePaths(dir!, "edgedb.toml"), platform);

            var parent = platform.DirectoryGetParent(dir!);

            if (parent is null || !parent.Exists)
                return null;

            dir = parent.FullName;
        }
    }

    private static ConfigUtils.ResolvedFields ParseCloudInstanceName(
        string name, string? secretKey, string? cloudProfile, ISystemProvider? platform)
    {
        if (name.Length > DOMAIN_NAME_MAX_LEN)
        {
            throw new ConfigurationException($"Cloud instance name must be {DOMAIN_NAME_MAX_LEN} characters or less");
        }

        if (secretKey is null)
        {
            var profile = ConfigUtils.ReadCloudProfile(cloudProfile ?? _defaultCloudProfile, platform);

            if (profile.SecretKey is null)
            {
                throw new ConfigurationException("Secret key cannot be null");
            }

            secretKey = profile.SecretKey;
        }

        var spl = secretKey.Split('.');

        if (spl.Length < 2)
        {
            throw new ConfigurationException("Invalid secret key: does not contain payload");
        }

        var json = Convert.FromBase64String(spl[1]);

        var jsonData = JsonConvert.DeserializeObject<Dictionary<string, object?>>(Encoding.UTF8.GetString(json))!;

        if (!jsonData.TryGetValue("iss", out var dnsZone))
        {
            throw new ConfigurationException("Invalid secret key: payload does not contain 'iss' value");
        }

        name = name.ToLowerInvariant();

        // safe: checks name length above to be less than DOMAIN_NAME_MAX_LEN
        Span<byte> instanceNameBuffer = stackalloc byte[name.Length];

        Encoding.UTF8.GetBytes(name, instanceNameBuffer);

        var dnsBucket = (CRCHQX.CRCHqx(instanceNameBuffer, 0) % 100)
            .ToString()
            .PadLeft(2, '0');

        spl = name.Split("/");

        return new()
        {
            Host = $"{spl[1]}--{spl[0]}.c-{dnsBucket}.i.{dnsZone}",
            SecretKey = secretKey,
        };
    }

    /// <summary>
    ///     Parses the provided arguments to build an <see cref="EdgeDBConnection" /> class; Parse logic follows
    ///     the
    ///     <see href="https://www.edgedb.com/docs/reference/connection#ref-reference-connection-priority">Priority levels</see>
    ///     of arguments.
    /// </summary>
    /// <param name="instance">The instance name to connect to.</param>
    /// <param name="dsn">A DSN string or cloud instance name.</param>
    /// <param name="configure">A configuration delegate.</param>
    /// <param name="autoResolve">Whether or not to autoresolve a connection using <see cref="ResolveEdgeDBTOML" />.</param>
    /// <returns>
    ///     A <see cref="EdgeDBConnection" /> class that can be used to connect to a EdgeDB instance.
    /// </returns>
    /// <exception cref="ConfigurationException">
    ///     An error occured while parsing or configuring the <see cref="EdgeDBConnection" />.
    /// </exception>
    /// <exception cref="FileNotFoundException">A configuration file could not be found.</exception>
    public static EdgeDBConnection Parse(string? instance = null, string? dsn = null,
        Action<EdgeDBConnection>? configure = null, bool autoResolve = true)
    {
        return _Parse(instance, dsn, configure, autoResolve, null);
    }

    internal static EdgeDBConnection _Parse(string? instance, string? dsn,
        Action<EdgeDBConnection>? configure, bool autoResolve, ISystemProvider? platform)
    {
        platform ??= ConfigUtils.DefaultPlatformProvider;

        EdgeDBConnection? connection = null;

        // try to resolve the toml, don't do this for cloud-like conn params.
        if (autoResolve && !((instance is not null && instance.Contains('/')) ||
                             (dsn is not null && !dsn.StartsWith("edgedb://") && !dsn.StartsWith("gel://"))))
        {
            ConfigUtils.ResolvedFields? resolvedFields = _ResolveEdgeDBTOML(platform);
            if (resolvedFields is not null)
            {
                connection = _FromResolvedFields(resolvedFields, platform);
            }
        }

        #region Old Env

        var envName = string.Empty;
        var envVar = string.Empty;

        if (platform.GetGelEnvVariable(CLOUD_PROFILE_ENV_NAME, out envName, out envVar))
        {
            connection ??= new EdgeDBConnection();
            connection.CloudProfile = envVar;
        }

        if (platform.GetGelEnvVariable(SECRET_KEY_ENV_NAME, out envName, out envVar))
        {
            connection ??= new EdgeDBConnection();
            connection.SecretKey = envVar;
        }

        if (platform.GetGelEnvVariable(INSTANCE_ENV_NAME, out envName, out envVar))
        {
            var fromInst = _FromResolvedFields(_FromInstanceName(envVar, null, platform), platform);
            connection = connection?.MergeInto(fromInst) ?? fromInst;
        }

        if (platform.GetGelEnvVariable(DSN_ENV_NAME, out envName, out envVar))
        {
            var fromDSN = _FromResolvedFields(_FromDSN(envVar, platform), platform);
            connection = connection?.MergeInto(fromDSN) ?? fromDSN;
        }

        if (platform.GetGelEnvVariable(HOST_ENV_NAME, out envName, out envVar))
        {
            connection ??= new EdgeDBConnection();
            try
            {
                connection.Hostname = envVar;
            }
            catch (ConfigurationException x)
            {
                switch (x.Message)
                {
                    case "DSN cannot contain more than one host":
                        throw new ConfigurationException(
                            $"Enviroment variable '{envName}' cannot contain more than one host", x);
                    default:
                        throw;
                }
            }
        }

        if (platform.GetGelEnvVariable(PORT_ENV_NAME, out envName, out envVar))
        {
            connection ??= new EdgeDBConnection();

            if (!int.TryParse(envVar, out var port))
                throw new ConfigurationException(
                    $"Expected integer for environment variable '{envName}' but got '{envVar}'");

            connection.Port = port;
        }

        if (platform.GetGelEnvVariable(CREDENTIALS_FILE_ENV_NAME, out envName, out envVar))
        {
            // check if file exists
            var path = envVar;
            if (!platform.FileExists(path))
                throw new FileNotFoundException(
                    $"Could not find the file specified in '{envName}'");

            var credentials = JsonConvert.DeserializeObject<EdgeDBConnection>(platform.FileReadAllText(path))!;
            connection = connection?.MergeInto(credentials) ?? credentials;
        }

        if (platform.GetGelEnvVariable(USER_ENV_NAME, out envName, out envVar))
        {
            connection ??= new EdgeDBConnection();
            connection.Username = envVar;
        }

        if (platform.GetGelEnvVariable(PASSWORD_ENV_NAME, out envName, out envVar))
        {
            connection ??= new EdgeDBConnection();
            connection.Password = envVar;
        }

        if (platform.GetGelEnvVariable(DATABASE_ENV_NAME, out envName, out envVar))
        {
            var altName = string.Empty;
            var altVal = string.Empty;
            if (platform.GetGelEnvVariable(BRANCH_ENV_NAME, out altName, out altVal))
                throw new ArgumentException($"{envName} and {altName} are mutually exclusive");

            connection ??= new EdgeDBConnection();

            connection.Database = envVar;
        }

        if (platform.GetGelEnvVariable(BRANCH_ENV_NAME, out envName, out envVar))
        {
            connection ??= new EdgeDBConnection();

            connection.Branch = envVar;
        }

        {
            string clientSecurityEnvName;
            string clientTlsSecurityEnvName;
            TLSSecurityMode? clientSecurity = null;
            TLSSecurityMode? clientTlsSecurity = null;
            if (platform.GetGelEnvVariable(CLIENT_SECURITY_ENV_NAME, out clientSecurityEnvName, out envVar))
            {
                connection ??= new EdgeDBConnection();

                clientSecurity = TLSSecurityModeParser.Parse(envVar);
                if (clientSecurity == TLSSecurityMode.Default)
                {
                    // ignore explicit defaults
                    clientSecurity = null;
                }

                if (clientSecurity is not null)
                {
                    connection.TLSSecurity = clientSecurity.Value;
                }
            }
            if (platform.GetGelEnvVariable(CLIENT_TLS_SECURITY_ENV_NAME, out clientTlsSecurityEnvName, out envVar))
            {
                connection ??= new EdgeDBConnection();

                clientTlsSecurity = TLSSecurityModeParser.Parse(envVar);
                if (clientTlsSecurity == TLSSecurityMode.Default)
                {
                    // ignore explicit defaults
                    clientTlsSecurity = null;
                }

                if (clientTlsSecurity is null)
                {
                    // do nothing
                }
                else if (clientSecurity is null)
                {
                    // overwrite default value
                    connection.TLSSecurity = clientTlsSecurity.Value;
                }
                else if (clientSecurity == TLSSecurityMode.Strict
                    && clientTlsSecurity != TLSSecurityMode.Strict)
                {
                    throw new ConfigurationException(
                        $"{clientSecurityEnvName}=strict but {clientTlsSecurityEnvName}={envVar}. "
                        + $"{clientTlsSecurityEnvName} must be strict when {clientSecurityEnvName} "
                        + $"is strict"
                    );
                }
                else
                {
                    // overwrite existing value
                    connection.TLSSecurity = clientTlsSecurity.Value;
                }
            }
        }

        #endregion

        if (instance is not null)
        {
            var fromInst = _FromResolvedFields(_FromInstanceName(instance, null, platform), platform);
            connection = connection?.MergeInto(fromInst) ?? fromInst;
        }

        if (dsn is not null)
        {
            if (Regex.IsMatch(dsn, @"^([A-Za-z0-9](-?[A-Za-z0-9])*)\/([A-Za-z0-9](-?[A-Za-z0-9])*)$"))
            {
                // cloud
                var fromCloud = _FromResolvedFields(ParseCloudInstanceName(dsn, connection?.SecretKey, null, platform), platform);
                connection = connection?.MergeInto(fromCloud) ?? fromCloud;
            }
            else
            {
                var fromDSN = _FromResolvedFields(_FromDSN(dsn, platform), platform);
                connection = connection?.MergeInto(fromDSN) ?? fromDSN;
            }
        }

        if (configure is not null)
        {
            connection ??= new EdgeDBConnection();

            var cloned = (EdgeDBConnection)connection.MemberwiseClone()!;
            configure(cloned);

            if (dsn is not null && cloned._hostname is not null)
                throw new ConfigurationException("Cannot specify DSN and 'Hostname'; they are mutually exclusive");

            connection = connection.MergeInto(cloned);
        }

        return connection ?? new EdgeDBConnection();
    }

    #endregion

    #region HTTP-based connection methods

    private string? _baseUri;
    private string? _authUri;
    private string? _execUri;

    internal string GetBaseUri()
        => _baseUri ??= $"https://{Hostname}:{Port}";

    internal string GetAuthUri()
        => _authUri ??= GetBaseUri() + "/auth/token";

    internal string GetExecUri()
        => _execUri ??= GetBaseUri() + $"/db/{Database}";

    #endregion
}
