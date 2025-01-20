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
    private const string USER_ENV_NAME = "USER";
    private const string PASSWORD_ENV_NAME = "PASSWORD";
    private const string DATABASE_ENV_NAME = "DATABASE";
    private const string BRANCH_ENV_NAME = "BRANCH";
    private const string HOST_ENV_NAME = "HOST";
    private const string PORT_ENV_NAME = "PORT";
    private const string CLOUD_PROFILE_ENV_NAME = "CLOUD_PROFILE";
    private const string SECRET_KEY_ENV_NAME = "SECRET_KEY";
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
    ///     This property defaults to 127.0.0.1.
    /// </remarks>
    public string Hostname
    {
        get => _hostname ?? "127.0.0.1";
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
    public int Timeout
    {
        get => _timeout ?? 30000;
        set => _timeout = value;
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
    private int? _timeout;
    private string? _cloudProfile;

    #endregion

    #region Construct methods

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
        return _FromDSN(dsn, null);
    }

    internal static EdgeDBConnection _FromDSN(string dsn, ISystemProvider? platform)
    {
        platform ??= ConfigUtils.DefaultPlatformProvider;

        if (!dsn.StartsWith("edgedb://") && !dsn.StartsWith("gel://"))
            throw new ConfigurationException("DSN schema 'gel' expected but got 'pq'");

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
                        throw new ArgumentException("Host ambiguity mismatch");

                    conn.Hostname = value;
                    break;
                case "database":
                    if (database is not null)
                        throw new ArgumentException("Database ambiguity mismatch");

                    conn.Database = value;
                    break;
                case "branch":
                    if (database is not null)
                        throw new ArgumentException("Database ambiguity mismatch");

                    conn.Branch = value;
                    break;
                case "user":
                    if (username is not null)
                        throw new ArgumentException("User ambiguity mismatch");

                    conn.Username = value;
                    break;
                case "password":
                    if (password is not null)
                        throw new ArgumentException("Password ambiguity mismatch");

                    conn.Password = value;
                    break;
                case "tls_cert_file":
                {
                    if (!platform.FileExists(value))
                        throw new FileNotFoundException("The specified tls_cert_file file was not found");

                    conn.TLSCertificateAuthority = platform.FileReadAllText(value);
                }
                    break;
                case "tls_security":
                    if (!Enum.TryParse<TLSSecurityMode>(value, true, out var result))
                        throw new FormatException($"\"{result}\" must be a value of TLSSecurityMode");

                    conn.TLSSecurity = result;
                    break;
                case "wait_until_available":
                    conn.Timeout = ParseWaitUntilAvailable(value);
                    break;

                default:
                    throw new FormatException($"Unexpected configuration option \"{name}\"");
            }
        }

        if (args.Any(x => x.Key.StartsWith("branch", StringComparison.InvariantCultureIgnoreCase)) && args.Any(x =>
                x.Key.StartsWith("database", StringComparison.InvariantCultureIgnoreCase)))
        {
            throw new ArgumentException("branch and database are mutually exclusive");
        }


        // query arguments
        foreach (var arg in args)
        {
            var fileMatch = Regex.Match(arg.Key!, @"(.*?)_file");
            var envMatch = Regex.Match(arg.Key!, @"(.*?)_env");

            if (fileMatch.Success)
            {
                var val = platform.FileReadAllText(arg.Value);

                SetArgument(fileMatch.Groups[1].Value, val, conn);
            }
            else if (envMatch.Success)
            {
                var val = platform.GetEnvVariable(arg.Value);

                if (val == null)
                    throw new KeyNotFoundException($"Environment variable \"{arg.Value}\" couldn't be found");

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
    /// <param name="path">The path to the .toml project file.</param>
    /// <returns>A <see cref="EdgeDBConnection" /> representing the project defined in the .toml file.</returns>
    /// <exception cref="FileNotFoundException">The supplied file path, credentials path, or instance-name file doesn't exist.</exception>
    /// <exception cref="DirectoryNotFoundException">The project directory doesn't exist for the supplied toml file.</exception>
    public static EdgeDBConnection FromProjectFile(string path)
    {
        return _FromProjectFile(path, null);
    }

    internal static EdgeDBConnection _FromProjectFile(string path, ISystemProvider? platform)
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

        var connection = _FromInstanceName(inst, profile, platform);

        if (ConfigUtils.TryResolveProjectDatabase(projectDir, out var database, platform))
            connection.Database = database;

        return connection;
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
        return _FromInstanceName(name, cloudProfile, null);
    }

    internal static EdgeDBConnection _FromInstanceName(string name, string? cloudProfile, ISystemProvider? platform)
    {
        platform ??= ConfigUtils.DefaultPlatformProvider;

        if (Regex.IsMatch(name, @"^\w(-?\w)*$"))
        {
            var configPath = platform.CombinePaths(ConfigUtils.GetCredentialsDir(platform), $"{name}.json");

            return !platform.FileExists(configPath)
                ? throw new FileNotFoundException($"Config file couldn't be found at {configPath}")
                : JsonConvert.DeserializeObject<EdgeDBConnection>(platform.FileReadAllText(configPath))!;
        }

        if (Regex.IsMatch(name, @"^([A-Za-z0-9](-?[A-Za-z0-9])*)\/([A-Za-z0-9](-?[A-Za-z0-9])*)$"))
        {
            var conn = new EdgeDBConnection();
            conn.ParseCloudInstanceName(name, cloudProfile, platform);
            return conn;
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
        return _ResolveEdgeDBTOML(null);
    }

    internal static EdgeDBConnection _ResolveEdgeDBTOML(ISystemProvider? platform)
    {
        platform ??= ConfigUtils.DefaultPlatformProvider;

        var dir = platform.GetCurrentDirectory();

        while (true)
        {
            if (platform.FileExists(platform.CombinePaths(dir!, "edgedb.toml")))
                return _FromProjectFile(platform.CombinePaths(dir!, "edgedb.toml"), platform);

            var parent = platform.DirectoryGetParent(dir!);

            if (parent is null || !parent.Exists)
                throw new FileNotFoundException("Couldn't resolve edgedb.toml file");

            dir = parent.FullName;
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

    internal static int ParseWaitUntilAvailable(string text)
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

        throw new ConfigurationException($"invalid duration {originalText}");
    }

    private void ParseCloudInstanceName(string name, string? cloudProfile, ISystemProvider? platform)
    {
        if (name.Length > DOMAIN_NAME_MAX_LEN)
        {
            throw new ConfigurationException($"Cloud instance name must be {DOMAIN_NAME_MAX_LEN} characters or less");
        }

        var secretKey = SecretKey;

        if (secretKey is null)
        {
            var profile = ConfigUtils.ReadCloudProfile(cloudProfile ?? CloudProfile, platform);

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

        Hostname = $"{spl[1]}--{spl[0]}.c-{dnsBucket}.i.{dnsZone}";
        SecretKey ??= secretKey;
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
            try
            {
                connection = _ResolveEdgeDBTOML(platform);
            }
            catch (FileNotFoundException)
            {
                // ignore
            }
        }

        #region Env

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
            var fromInst = _FromInstanceName(envVar, null, platform);
            connection = connection?.MergeInto(fromInst) ?? fromInst;
        }

        if (platform.GetGelEnvVariable(DSN_ENV_NAME, out envName, out envVar))
        {
            var fromDSN = _FromDSN(envVar, platform);
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

        #endregion

        if (instance is not null)
        {
            var fromInst = _FromInstanceName(instance, null, platform);
            connection = connection?.MergeInto(fromInst) ?? fromInst;
        }

        if (dsn is not null)
        {
            if (Regex.IsMatch(dsn, @"^([A-Za-z0-9](-?[A-Za-z0-9])*)\/([A-Za-z0-9](-?[A-Za-z0-9])*)$"))
            {
                // cloud
                connection ??= new EdgeDBConnection();
                connection.ParseCloudInstanceName(dsn, null, platform);
            }
            else
            {
                var fromDSN = _FromDSN(dsn, platform);
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
