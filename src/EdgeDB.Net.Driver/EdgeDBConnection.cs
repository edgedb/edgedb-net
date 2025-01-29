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
    private const string TIMEOUT_ENV_NAME = "WAIT_UNTIL_AVAILABLE";
    private const string CLOUD_PROFILE_ENV_NAME = "CLOUD_PROFILE";
    private const int DOMAIN_NAME_MAX_LEN = 62;

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
    ///     Gets the hostname of the edgedb instance to connect to.
    /// </summary>
    /// <remarks>
    ///     This property defaults to 127.0.0.1.
    /// </remarks>
    public string Hostname
    {
        get => _hostname ?? "127.0.0.1";
    }
    private string? _hostname;

    /// <summary>
    ///     Gets the port of the edgedb instance to connect to.
    /// </summary>
    /// <remarks>
    ///     This property defaults to 5656
    /// </remarks>
    [JsonProperty("port")]
    public int Port
    {
        get => _port ?? 5656;
    }
    private int? _port;

    /// <summary>
    ///     Gets the database name to use when connecting.
    /// </summary>
    /// <remarks>
    ///     This property defaults to <c>edgedb</c>.  It is mutually exclusive with <see cref="Branch"/>.
    /// </remarks>
    /// <exception cref="InvalidOperationException"><see cref="Branch"/> already contains a value; they're mutually exclusive</exception>
    [JsonProperty("database")]
    public string? Database
    {
        get => _database ?? _branch ?? _defaultDatabase;
    }
    private string? _database;
    private static readonly string _defaultDatabase = "edgedb";

    /// <summary>
    ///     Gets the branch name to use when connecting.
    /// </summary>
    /// <remarks>
    ///     This property defaults to <c>__default__</c>. It is mutually exclusive with <see cref="Database"/>
    /// </remarks>
    /// <exception cref="InvalidOperationException"><see cref="Database"/> already contains a value; they're mutually exclusive</exception>
    [JsonProperty("branch")]
    public string? Branch
    {
        get => _database ?? _branch ?? _defaultBranch;
    }
    private string? _branch;
    private static readonly string _defaultBranch = "__default__";

    /// <summary>
    ///     Gets the username used to connect to the database.
    /// </summary>
    /// <remarks>
    ///     This property defaults to edgedb
    /// </remarks>
    [JsonProperty("user")]
    public string Username
    {
        get => _user ?? "edgedb";
    }
    private string? _user;

    /// <summary>
    ///     Gets the password to connect to the database.
    /// </summary>
    [JsonProperty("password")]
    public string? Password
    {
        get => _password ?? "";
    }
    private string? _password;

    /// <summary>
    ///     Gets the secret key used to authenticate with cloud instances.
    /// </summary>
    public string? SecretKey { get; private set; }

    /// <summary>
    ///     Gets the TLS Certificate Authority.
    /// </summary>
    [JsonProperty("tls_ca")]
    public string? TLSCertificateAuthority { get; private set; }

    /// <summary>
    ///     Gets the TLS security level.
    /// </summary>
    /// <remarks>
    ///     The default value is <see cref="TLSSecurityMode.Strict" />.
    /// </remarks>
    [JsonProperty("tls_security")]
    public TLSSecurityMode TLSSecurity
    {
        get => _tlsSecurity ?? TLSSecurityMode.Strict;
    }
    private TLSSecurityMode? _tlsSecurity;

    /// <summary>
    ///     Gets the TLS server name to be used.
    /// </summary>
    /// <remarks>
    ///     Overrides the value provided by Hostname.
    /// </remarks>
    [JsonProperty("tls_server_name")]
    public string? TLSServerName { get; private set; }

    /// <summary>
    ///     Gets the number of miliseconds a client will wait for a connection to be
    ///     established with the server.
    /// </summary>
    [JsonProperty("wait_until_available")]
    public int Timeout
    {
        get => _timeout ?? 30000;
    }
    private int? _timeout;

    /// <summary>
    ///     Additional settings for the server connection.
    /// </summary>
    /// <remarks>
    ///     This currently has no effect.
    /// </remarks>
    public Dictionary<string, string> ServerSettings { get; private set; } = new();

    #endregion

    #region Construct methods

    internal class Credentials
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

    internal record DatabaseOrBranch
    {
        private DatabaseOrBranch(string value) { Value = value; }
        internal record DatabaseName(string value) : DatabaseOrBranch(value);
        internal record BranchName(string value) : DatabaseOrBranch(value);

        internal string Value { get; set; }
    }

    internal record ResolvedField<T>
    {
        private ResolvedField(T? value, Exception? error) { Value = value; Error = error; }
        internal record Valid(T v) : ResolvedField<T>(v, null);
        internal record Invalid(Exception error) : ResolvedField<T>(default(T), error);

        public static implicit operator ResolvedField<T>(T value) { return new Valid(value); }
        public static implicit operator ResolvedField<T>(Exception error) { return new Invalid(error); }

        internal T? Value { get; private init; }
        internal Exception? Error { get; private init; }

        internal ResolvedField<U>? Convert<U>(Func<T, ResolvedField<U>?> func)
        {
            if (Error is not null)
            {
                return Error;
            }
            else
            {
                return func(Value!);
            }
        }

        internal T CheckAndGetValue()
        {
            if (Error is not null)
            {
                throw Error;
            }
            else
            {
                return Value!;
            }
        }
    }

    internal static bool TryGetFieldValue<T>(ResolvedField<T>? field, out T value)
    {
        if (field is ResolvedField<T>.Valid)
        {
            value = field.Value!;
            return true;
        }
        else
        {
            value = default(T)!;
            return false;
        }
    }

    static ResolvedField<T>? MergeField<T>(ResolvedField<T>? to, ResolvedField<T>? from)
    {
        if (to is null)
        {
            return from;
        }
        else if (from is ResolvedField<T>.Valid)
        {
            return from;
        }
        else
        {
            return to;
        }
    }

    static Dictionary<string, ResolvedField<string>> AddServerSettingField(
        Dictionary<string, ResolvedField<string>> serverSettings, string key, ResolvedField<string> field)
    {
        if (serverSettings.ContainsKey(key))
        {
            serverSettings[key] = MergeField(serverSettings[key], field)!;
        }
        else
        {
            serverSettings[key] = field;
        }

        return serverSettings;
    }

    static Dictionary<string, string> CheckAndGetServerSettings(
        Dictionary<string, ResolvedField<string>> serverSettings)
    {
        Dictionary<string, string> result = new();
        foreach (KeyValuePair<string, ResolvedField<string>> entry in serverSettings)
        {
            result[entry.Key] = entry.Value.CheckAndGetValue()!;
        }

        return result;
    }

    internal class ResolvedFields
    {
        public ResolvedField<string>? Host { get; set; }
        public ResolvedField<int>? Port { get; set; }
        public ResolvedField<DatabaseOrBranch>? DatabaseOrBranch { get; set; }
        public ResolvedField<string>? User { get; set; }
        public ResolvedField<string>? Password { get; set; }
        public ResolvedField<string>? SecretKey { get; set; }
        public ResolvedField<string>? TLSCertificateAuthority { get; set; }
        public ResolvedField<TLSSecurityMode>? TLSSecurity { get; set; }
        public ResolvedField<string>? TLSServerName { get; set; }
        public ResolvedField<int>? Timeout { get; set; }
        public Dictionary<string, ResolvedField<string>> ServerSettings { get; set; } = new();

        public void MergeFrom(ResolvedFields other)
        {
            Host = MergeField(Host, other.Host);
            Port = MergeField(Port, other.Port);
            DatabaseOrBranch = MergeField(DatabaseOrBranch, other.DatabaseOrBranch);
            User = MergeField(User, other.User);
            Password = MergeField(Password, other.Password);
            SecretKey = MergeField(SecretKey, other.SecretKey);
            TLSCertificateAuthority = MergeField(TLSCertificateAuthority, other.TLSCertificateAuthority);
            TLSSecurity = MergeField(TLSSecurity, other.TLSSecurity);
            TLSServerName = MergeField(TLSServerName, other.TLSServerName);
            Timeout = MergeField(Timeout, other.Timeout);

            foreach (KeyValuePair<string,ResolvedField<string>> entry in other.ServerSettings)
            {
                ServerSettings = AddServerSettingField(ServerSettings, entry.Key, entry.Value);
            }
        }

        public bool IsEmpty =>
            Host is null
            && Port is null
            && DatabaseOrBranch is null
            && User is null
            && Password is null
            && SecretKey is null
            && TLSCertificateAuthority is null
            && TLSSecurity is null
            && TLSServerName is null
            && Timeout is null
            && ServerSettings.Count == 0;

        internal static ResolvedFields FromConnection(EdgeDBConnection connection)
        {
            ResolvedFields result = new();

            if (connection._hostname is not null) { result.Host = connection._hostname; }
            if (connection._port is not null) { result.Port = connection._port; }
            if (connection._database is not null)
            {
                result.DatabaseOrBranch = new DatabaseOrBranch.DatabaseName(connection._database);
            }
            if (connection._branch is not null)
            {
                result.DatabaseOrBranch = new DatabaseOrBranch.BranchName(connection._branch);
            }
            if (connection._user is not null) { result.User = connection._user; }
            if (connection._password is not null) { result.Password = connection._password; }
            if (connection.SecretKey is not null) { result.SecretKey = connection.SecretKey; }
            if (connection.TLSCertificateAuthority is not null) { result.TLSCertificateAuthority = connection.TLSCertificateAuthority; }
            if (connection._tlsSecurity is not null) { result.TLSSecurity = connection._tlsSecurity; }
            if (connection.TLSServerName is not null) { result.TLSServerName = connection.TLSServerName; }
            if (connection._timeout is not null) { result.Timeout = connection._timeout; }
            foreach (KeyValuePair<string, string> entry in connection.ServerSettings)
            {
                result.ServerSettings[entry.Key] = entry.Value;
            }

            return result;
        }

        internal static ResolvedFields FromCredentials(Credentials credentials)
        {
            ResolvedFields result = new();

            if (credentials.Host is not null) { result.Host = credentials.Host; }
            if (credentials.Port is not null)
            {
                result.Port = MergeField(result.Port, ParsePort(credentials.Port));
            }
            if (credentials.Database is not null)
            {
                result.DatabaseOrBranch = new DatabaseOrBranch.DatabaseName(credentials.Database);
            }
            if (credentials.Branch is not null)
            {
                result.DatabaseOrBranch = new DatabaseOrBranch.BranchName(credentials.Branch);
            }
            if (credentials.User is not null) { result.User = credentials.User; }
            if (credentials.Password is not null) { result.Password = credentials.Password; }
            if (credentials.TlsCA is not null) { result.TLSCertificateAuthority = credentials.TlsCA; }
            if (credentials.TlsSecurity is not null) { result.TLSSecurity = credentials.TlsSecurity; }

            return result;
        }
    }

    internal static EdgeDBConnection _FromResolvedFields(ResolvedFields resolvedFields, ISystemProvider? platform)
    {
        platform ??= ConfigUtils.DefaultPlatformProvider;

        if (TryGetFieldValue(resolvedFields.Host, out string host))
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
        if (TryGetFieldValue(resolvedFields.Port, out int port))
        {
            if (port < 1 || 65535 < port)
            {
                throw new ConfigurationException($"Invalid port: \"{port}\", must be between 1 and 65535");
            }
        }
        if (TryGetFieldValue(resolvedFields.DatabaseOrBranch, out DatabaseOrBranch databaseOrBranch))
        {
            if (databaseOrBranch.Value == "")
            {
                throw databaseOrBranch switch
                {
                    DatabaseOrBranch.DatabaseName name => new ConfigurationException(
                        $"Invalid database name: \"{name.Value}\""),
                    DatabaseOrBranch.BranchName name => new ConfigurationException(
                        $"Invalid branch name: \"{name.Value}\""),
                    _ => new ConfigurationException("Invalid database or branch name"),
                };
            }
        }
        if (TryGetFieldValue(resolvedFields.User, out string user))
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
                    DatabaseOrBranch.DatabaseName name => name.Value,
                    _ => null,
                }
            ),
            _branch = (
                resolvedFields.DatabaseOrBranch?.Value switch
                {
                    DatabaseOrBranch.BranchName name => name.Value,
                    _ => null,
                }
            ),
            _user = resolvedFields.User?.CheckAndGetValue(),
            _password = resolvedFields.Password?.CheckAndGetValue(),
            SecretKey = resolvedFields.SecretKey?.CheckAndGetValue(),
            TLSCertificateAuthority = resolvedFields.TLSCertificateAuthority?.CheckAndGetValue(),
            _tlsSecurity = resolvedFields.TLSSecurity?.CheckAndGetValue(),
            TLSServerName = resolvedFields.TLSServerName?.CheckAndGetValue(),
            _timeout = resolvedFields.Timeout?.CheckAndGetValue(),
            ServerSettings = CheckAndGetServerSettings(resolvedFields.ServerSettings),
        };
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

    internal static ResolvedFields _FromDSN(string dsn, ISystemProvider? platform)
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
                $"Invalid DSN scheme. Expected \"edgedb\" or \"gel\" but got \"{scheme}\"");
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

        var resolvedFields = new ResolvedFields();

        if (host is not null) { resolvedFields.Host = host; }
        if (port is not null)
        {
            if (!int.TryParse(port, out var parsedPort))
                throw new ConfigurationException("Invalid DSN: port was not in the correct format");

            resolvedFields.Port = parsedPort;
        }
        if (branch is not null && branch != "")
        {
            resolvedFields.DatabaseOrBranch = new DatabaseOrBranch.BranchName(branch);
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
            ResolvedField<string> value = arg.Value;

            if (key.EndsWith("_env") && TryGetFieldValue(value, out string envName))
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

            if (key.EndsWith("_file") && TryGetFieldValue(value, out string fileName))
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
                    resolvedFields.Port = value.Convert(ParsePort);
                    break;
                case "database":
                    resolvedFields.DatabaseOrBranch = value.Convert<DatabaseOrBranch>(v =>
                    {
                        if (v.StartsWith("/"))
                        {
                            v = v.Substring(1);
                        }
                        if (v != "")
                        {
                            return new DatabaseOrBranch.DatabaseName(v);
                        }
                        return null;
                    });
                    break;
                case "branch":
                    resolvedFields.DatabaseOrBranch = value.Convert<DatabaseOrBranch>(v =>
                    {
                        if (v.StartsWith("/"))
                        {
                            v = v.Substring(1);
                        }
                        if (v != "")
                        {
                            return new DatabaseOrBranch.BranchName(v);
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
                    resolvedFields.Timeout = value.Convert(ParseWaitUntilAvailable);
                    break;

                default:
                    resolvedFields.ServerSettings =
                        AddServerSettingField(resolvedFields.ServerSettings, key, value);
                    break;
            }
        }

        return resolvedFields;
    }

    internal static ResolvedFields _FromProjectFile(string path, ISystemProvider? platform)
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
            resolvedFields.DatabaseOrBranch = new DatabaseOrBranch.DatabaseName(database);
        }

        return resolvedFields;
    }

    internal static ResolvedFields _FromInstanceName(string name, string? cloudProfile, ISystemProvider? platform)
    {
        platform ??= ConfigUtils.DefaultPlatformProvider;

        if (Regex.IsMatch(name, @"^\w(-?\w)*$"))
        {
            var configPath = platform.CombinePaths(ConfigUtils.GetCredentialsDir(platform), $"{name}.json");

            if (!platform.FileExists(configPath))
            {
                throw new FileNotFoundException($"Config file couldn't be found at {configPath}");
            }

            EdgeDBConnection connection = JsonConvert.DeserializeObject<EdgeDBConnection>(
                platform.FileReadAllText(configPath))!;

            return ResolvedFields.FromConnection(connection);
        }

        if (Regex.IsMatch(name, @"^([A-Za-z0-9](-?[A-Za-z0-9])*)\/([A-Za-z0-9](-?[A-Za-z0-9])*)$"))
        {
            return ParseCloudInstanceName(name, null, cloudProfile, platform);
        }

        throw new ConfigurationException($"Invalid instance name '{name}'");
    }

    internal static ResolvedFields? _ResolveEdgeDBTOML(ISystemProvider? platform)
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

    internal static ResolvedField<int>? ParsePort(string text)
    {
        if (text.StartsWith("tcp://"))
        {
            return null;
        }
        else if (int.TryParse(text, out var port))
        {
            return port;
        }
        else
        {
            return new ConfigurationException($"Invalid port: \"{text}\", not an integer");
        }
    }

    private static readonly Regex _isoUnitlessHours = new Regex(
        @"^(-?\d+|-?\d+\.\d*|-?\d*\.\d+)$",
        RegexOptions.Compiled);
    private static readonly Regex _isoTimeWithUnits = new Regex(
        @"(?<Hours>(?<vh>-?\d+|-?\d+\.\d*|-?\d*\.\d+)H)?"
        + @"(?<Minutes>(?<vm>-?\d+|-?\d+\.\d*|-?\d*\.\d+)M)?"
        + @"(?<Seconds>(?<vs>-?\d+|-?\d+\.\d*|-?\d*\.\d+)S)?",
        RegexOptions.Compiled);
    private static readonly Regex _humanHours = new Regex(
        @"(?<time>(?:(?<=\s|^)-\s*)?\d*\.?\d*)\s*(?:h(?=\s|\d|\.|$)|hours?(?:\s|$))",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex _humanMinutes = new Regex(
        @"(?<time>(?:(?<=\s|^)-\s*)?\d*\.?\d*)\s*(?:m(?=\s|\d|\.|$)|minutes?(?:\s|$))",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex _humanSeconds = new Regex(
        @"(?<time>(?:(?<=\s|^)-\s*)?\d*\.?\d*)\s*(?:s(?=\s|\d|\.|$)|seconds?(?:\s|$))",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex _humanMilliseconds = new Regex(
        @"(?<time>(?:(?<=\s|^)-\s*)?\d*\.?\d*)\s*(?:ms(?=\s|\d|\.|$)|milliseconds?(?:\s|$))",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex _humanNanoseconds = new Regex(
        @"(?<time>(?:(?<=\s|^)-\s*)?\d*\.?\d*)\s*(?:us(\s|\d|\.|$)|microseconds?(?:\s|$))",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    internal static ResolvedField<int> ParseWaitUntilAvailable(string text)
    {
        string originalText = text;

        if (text.StartsWith("PT"))
        {
            // ISO duration
            text = text.Substring(2);
            Match match = _isoUnitlessHours.Match(text);
            if (match.Success)
            {
                double hours = double.Parse(match.Groups[0].Value);
                return Convert.ToInt32(hours * 3600)* 1000;
            }

            match = _isoTimeWithUnits.Match(text);
            if (match.Success)
            {
                static void PopIsoDuration(
                    Match match, string groupName, string valueName, int factor, ref string text, ref int time)
                {
                    if (match.Groups.TryGetValue(valueName, out Group? value) && value.Value != "")
                    {
                        text = text.Replace(match.Groups[groupName].Value, "");
                        time += Convert.ToInt32(double.Parse(value.Value) * factor);
                    }
                }

                int time = 0;
                PopIsoDuration(match, "Hours", "vh", 3600 * 1000, ref text, ref time);
                PopIsoDuration(match, "Minutes", "vm", 60 * 1000, ref text, ref time);
                PopIsoDuration(match, "Seconds", "vs", 1 * 1000, ref text, ref time);
                if (text == "")
                {
                    return time;
                }
            }
        }
        else
        {
            // human duration
            static bool PopHumanDuration(Regex regex, int factor, ref string text, ref int time)
            {
                Match match = regex.Match(text);
                if (!match.Success || string.IsNullOrEmpty(match.Groups["time"].Value))
                {
                    return false;
                }

                string part = Regex.Replace(match.Groups["time"].Value, @"\s+", "");
                if (part == "" || part.EndsWith('.') || part.StartsWith("-."))
                {
                    return false;
                }

                time += Convert.ToInt32(double.Parse(part) * factor);
                text = text.Replace(match.Value, "");

                return true;
            }

            bool found = false;
            int time = 0;
            if (PopHumanDuration(_humanHours, 3600 * 1000, ref text, ref time)) { found = true; }
            if (PopHumanDuration(_humanMinutes, 60 * 1000, ref text, ref time)) { found = true; }
            if (PopHumanDuration(_humanSeconds, 1 * 1000, ref text, ref time)) { found = true; }
            if (PopHumanDuration(_humanMilliseconds, 1, ref text, ref time)) { found = true; }
            // We parse nanoseconds, but don't support them
            if (PopHumanDuration(_humanNanoseconds, 0, ref text, ref time)) { found = true; }
            if (found && text.Trim() == "")
            {
                return time;
            }
        }

        return new ConfigurationException($"invalid duration {originalText}");
    }

    private static readonly string _defaultCloudProfile = "default";
    private static ResolvedFields ParseCloudInstanceName(
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

    public class Options
    {
        public string? Instance { get; set; }
        public string? Dsn { get; set; }
        public string? Database { get; set; }
        public string? Branch { get; set; }
        public string? Host { get; set; }
        public int? Port { get; set; }
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
            && Database is null
            && Branch is null
            && Host is null
            && Port is null
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

    public static EdgeDBConnection Create(Options? options = null)
    {
        return _Create(options ?? new(), null);
    }

    internal static EdgeDBConnection _Create(Options options, ISystemProvider? platform)
    {
        platform ??= ConfigUtils.DefaultPlatformProvider;

        ResolvedFields resolvedFields = new();

        #region Compound Options

        // First, check compound options
        // If any compound options are present, environment variables are ignored

        bool hasCompoundOptions = false;
        {
            // These options can set multiple fields and should be resolved first
            // More than one compound options should raise an error

            Exception compoundError = new ConfigurationException(
                "Connection options cannot have more than one of the following "
                + "values: \"Instance\", \"Dsn\", \"Credentials\", "
                + "\"CredentialsFile\" or \"Host\"/\"Port\"");
            // The compoundEnvError has priority, so hold on to any other exception
            // until all compound options are processed.
            Exception? deferredCompoundError = null;

            if (options.Instance is not null)
            {
                if (hasCompoundOptions) { throw compoundError; }
                if (options.Instance == "")
                {
                    try
                    {
                        var fromDSN = _FromInstanceName(options.Instance, null, platform);
                        resolvedFields.MergeFrom(fromDSN);
                    }
                    catch (Exception e)
                    {
                        deferredCompoundError = e;
                    }
                }
                else
                {
                    deferredCompoundError = new ConfigurationException(
                        $"Invalid instance name: \"{options.Instance}\"");
                }
                hasCompoundOptions = true;
            }

            if (options.Dsn is not null)
            {
                if (hasCompoundOptions) { throw compoundError; }
                var fromDSN = _FromDSN(options.Dsn, platform);
                resolvedFields.MergeFrom(fromDSN);
                hasCompoundOptions = true;
            }

            {
                string? credentialsText = null;
                if (options.Credentials is not null)
                {
                    if (hasCompoundOptions) { throw compoundError; }
                    credentialsText = options.Credentials;
                    hasCompoundOptions = true;
                }
                if (options.CredentialsFile is not null)
                {
                    if (hasCompoundOptions) { throw compoundError; }
                    if (platform.FileExists(options.CredentialsFile))
                    {
                        credentialsText = platform.FileReadAllText(options.CredentialsFile) ?? "{}";
                    }
                    else
                    {
                        deferredCompoundError = new ConfigurationException(
                            $"Invalid CredentialsFile: \"{options.CredentialsFile}\", could not find file");
                    }
                    hasCompoundOptions = true;
                }
                if (credentialsText is not null)
                {
                    try
                    {
                        Credentials? credentials = new JsonSerializer().DeserializeObject<Credentials>(credentialsText);
                        if (credentials is not null)
                        {
                            resolvedFields.MergeFrom(ResolvedFields.FromCredentials(credentials));
                        }
                    }
                    catch (JsonException)
                    {
                        deferredCompoundError = new ConfigurationException("Invalid Credentials: could not parse json");
                    }
                }
            }

            {
                bool hasHostOrPort = false;
                if (options.Host is not null)
                {
                    if (hasCompoundOptions) { throw compoundError; }
                    resolvedFields.Host = options.Host;
                    hasHostOrPort = true;
                }
                if (options.Port is not null)
                {
                    if (hasCompoundOptions) { throw compoundError; }
                    resolvedFields.Port = options.Port;
                    hasHostOrPort = true;
                }
                if (hasHostOrPort)
                {
                    hasCompoundOptions = true;
                }
            }

            if (deferredCompoundError is not null)
            {
                throw deferredCompoundError;
            }
        }

        #endregion

        #region Compound Env

        var envName = string.Empty;
        var envVar = string.Empty;

        bool hasCompoundEnv = false;

        if (!hasCompoundOptions)
        {
            // These env vars can set multiple fields and should be resolved first
            // More than one compound env var should raise an error

            Exception compoundError = new ConfigurationException(
                "Cannot have more than one of the following connection "
                + "environment variables: \"GEL_DSN\", \"GEL_INSTANCE\", "
                + "\"GEL_CREDENTIALS_FILE\" or \"GEL_HOST\"/\"GEL_PORT\"");
            // The compoundError has priority, so hold on to any other exception
            // until all compound env vars are processed.
            Exception? deferredCompoundError = null;

            if (platform.GetGelEnvVariable(INSTANCE_ENV_NAME, out envName, out envVar))
            {
                if (hasCompoundEnv) { throw compoundError; }
                try
                {
                    var fromInst = _FromInstanceName(envVar, null, platform);
                    resolvedFields.MergeFrom(fromInst);
                }
                catch (Exception e)
                {
                    deferredCompoundError = e;
                }
                hasCompoundEnv = true;
            }

            if (platform.GetGelEnvVariable(DSN_ENV_NAME, out envName, out envVar))
            {
                if (hasCompoundEnv) { throw compoundError; }
                var fromDSN = _FromDSN(envVar, platform);
                resolvedFields.MergeFrom(fromDSN);
                hasCompoundEnv = true;
            }

            if (platform.GetGelEnvVariable(CREDENTIALS_FILE_ENV_NAME, out envName, out envVar))
            {
                if (hasCompoundEnv) { throw compoundError; }
                if (platform.FileExists(envVar))
                {
                    var credentials = JsonConvert.DeserializeObject<EdgeDBConnection>(platform.FileReadAllText(envVar))!;
                    resolvedFields.MergeFrom(ResolvedFields.FromConnection(credentials));
                }
                else
                {
                    deferredCompoundError = new FileNotFoundException(
                        $"Invalid credential file from {envName}: \"{envVar}\", could not find file");
                }
                hasCompoundEnv = true;
            }

            {
                bool hasHostOrPort = false;
                if (platform.GetGelEnvVariable(HOST_ENV_NAME, out envName, out envVar))
                {
                    if (hasCompoundEnv) { throw compoundError; }
                    resolvedFields.Host = envVar;
                    hasHostOrPort = true;
                }
                if (platform.GetGelEnvVariable(PORT_ENV_NAME, out envName, out envVar))
                {
                    ResolvedField<int>? port = ParsePort(envVar);
                    if (port is not null)
                    {
                        if (hasCompoundEnv) { throw compoundError; }
                        resolvedFields.Port = MergeField(resolvedFields.Port, port);
                        hasHostOrPort = true;
                    }
                }
                if (hasHostOrPort)
                {
                    hasCompoundEnv = true;
                }
            }

            if (deferredCompoundError is not null)
            {
                throw deferredCompoundError;
            }
        }

        #endregion

        #region Toml File

        if (!hasCompoundOptions && !hasCompoundEnv)
        {
            ResolvedFields? fromToml = _ResolveEdgeDBTOML(platform);
            if (fromToml is not null)
            {
                resolvedFields.MergeFrom(fromToml);
            }
        }

        #endregion

        #region Other Env

        if (!hasCompoundOptions)
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

                resolvedFields.DatabaseOrBranch = new DatabaseOrBranch.DatabaseName(envVar);
            }

            if (platform.GetGelEnvVariable(BRANCH_ENV_NAME, out envName, out envVar))
            {
                resolvedFields.DatabaseOrBranch = new DatabaseOrBranch.BranchName(envVar);
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

            if (platform.GetGelEnvVariable(TIMEOUT_ENV_NAME, out envName, out envVar))
            {
                resolvedFields.Timeout = ParseWaitUntilAvailable(envVar);
            }
        }

        #endregion

        #region Other Options

        // Finally, check non-compound options
        // Non-compound options should override environment variables

        // Validate options

        if (options.Database is not null && options.Branch is not null)
        {
            throw new ConfigurationException("Invalid options: Database and Branch are mutually exclusive.");
        }

        // Resolve other options

        if (options.Database is not null)
        {
            resolvedFields.DatabaseOrBranch = new DatabaseOrBranch.DatabaseName(options.Database);
        }
        else if (options.Branch is not null)
        {
            resolvedFields.DatabaseOrBranch = new DatabaseOrBranch.BranchName(options.Branch);
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
            resolvedFields.Timeout = MergeField(resolvedFields.Timeout, ParseWaitUntilAvailable(options.WaitUntilAvailable));
        }
        if (options.ServerSettings is not null)
        {
            foreach (KeyValuePair<string,string> entry in options.ServerSettings)
            {
                resolvedFields.ServerSettings = AddServerSettingField(
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
