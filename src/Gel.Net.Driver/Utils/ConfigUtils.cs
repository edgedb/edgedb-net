using Gel.Abstractions;
using Gel.Models;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Gel.Utils;

internal static class ConfigUtils
{
    internal static ISystemProvider DefaultPlatformProvider { get; } = new DefaultSystemProvider();

    private static string GetEdgeDBKnownBasePath(ISystemProvider platform)
    {
        if (platform.IsOSPlatform(OSPlatform.Windows))
            return platform.CombinePaths(platform.GetHomeDir(), "AppData", "Local", "EdgeDB");
        if (platform.IsOSPlatform(OSPlatform.OSX))
            return platform.CombinePaths(platform.GetHomeDir(), "Library", "Application Support", "edgedb");
        var xdgConfigDir = platform.GetEnvVariable("XDG_CONFIG_HOME");

        if (xdgConfigDir is null || !platform.IsRooted(xdgConfigDir))
            xdgConfigDir = platform.CombinePaths(platform.GetHomeDir(), ".config");

        return platform.CombinePaths(xdgConfigDir, "edgedb");
    }

    private static string GetEdgeDBBasePath(ISystemProvider platform)
    {
        var basePath = GetEdgeDBKnownBasePath(platform);
        return platform.DirectoryExists(basePath)
            ? basePath
            : platform.CombinePaths(platform.GetHomeDir(), ".edgedb");
    }

    public static string GetInstanceProjectDirectory(string projectDir, ISystemProvider platform)
    {
        var fullPath = platform.GetFullPath(projectDir);
        var baseName = projectDir.Split(platform.DirectorySeparatorChar).Last();
        var hash = "";

        if (platform.IsOSPlatform(OSPlatform.Windows) && !fullPath.StartsWith("\\\\"))
            fullPath = "\\\\?\\" + fullPath;

        using (var sha1 = SHA1.Create())
            hash = HexConverter.ToHex(sha1.ComputeHash(Encoding.UTF8.GetBytes(fullPath)));

        return platform.CombinePaths(GetEdgeDBConfigDir(platform), "projects", $"{baseName}-{hash.ToLower()}");
    }

    public static string GetEdgeDBConfigDir(ISystemProvider platform)
        => platform.IsOSPlatform(OSPlatform.Windows)
            ? platform.CombinePaths(GetEdgeDBBasePath(platform), "config")
            : GetEdgeDBBasePath(platform);

    public static string GetCredentialsDir(ISystemProvider platform)
        => platform.CombinePaths(GetEdgeDBConfigDir(platform), "credentials");

    public static bool TryResolveProjectDatabase(
        string stashDir,
        [NotNullWhen(true)] out string? database,
        ISystemProvider platform)
    {
        database = null;

        if (!platform.DirectoryExists(stashDir))
            return false;

        var databasePath = platform.CombinePaths(stashDir, "database");

        if (platform.FileExists(databasePath))
        {
            database = platform.FileReadAllText(databasePath);
            return true;
        }

        return false;
    }

    public static bool TryResolveInstanceCloudProfile(
        string stashDir,
        out string? profile,
        out string? linkedInstanceName,
        ISystemProvider platform)
    {
        profile = null;
        linkedInstanceName = null;

        if (!platform.DirectoryExists(stashDir))
            return false;

        var cloudProfilePath = platform.CombinePaths(stashDir, "cloud-profile");

        if (platform.FileExists(cloudProfilePath))
        {
            profile = platform.FileReadAllText(cloudProfilePath);
        }

        var linkedInstancePath = platform.CombinePaths(stashDir, "instance-name");

        if (platform.FileExists(linkedInstancePath))
        {
            linkedInstanceName = platform.FileReadAllText(linkedInstancePath);
        }

        return profile is not null || linkedInstanceName is not null;
    }

    public static CloudProfile ReadCloudProfile(string profile, ISystemProvider platform)
    {
        var profilePath = platform.CombinePaths(GetEdgeDBConfigDir(platform), "cloud-credentials", $"{profile}.json");

        if (!platform.FileExists(profilePath))
            throw new ConfigurationException($"Unknown cloud profile '{profile}'");

        return JsonConvert.DeserializeObject<CloudProfile>(platform.FileReadAllText(profilePath))!;
    }

    #region ResolvedFields

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

    internal static ResolvedField<T>? MergeField<T>(ResolvedField<T>? to, ResolvedField<T>? from)
    {
        if (to is null)
        {
            return from;
        }
        else if (from is not null && from.Value is not null)
        {
            return from;
        }
        else
        {
            return to;
        }
    }

    internal static Dictionary<string, ResolvedField<string>> AddServerSettingField(
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

    internal static Dictionary<string, string> CheckAndGetServerSettings(
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
        public ResolvedField<int>? WaitUntilAvailable { get; set; }
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
            WaitUntilAvailable = MergeField(WaitUntilAvailable, other.WaitUntilAvailable);

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
            && WaitUntilAvailable is null
            && ServerSettings.Count == 0;

        internal static ResolvedFields FromCredentials(ConnectionCredentials credentials)
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

    #endregion

    #region Parse Functions

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
    private static readonly Regex _humanMicroseconds = new Regex(
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
            // We parse microseconds, but don't support them
            if (PopHumanDuration(_humanMicroseconds, 0, ref text, ref time)) { found = true; }
            if (found && text.Trim() == "")
            {
                return time;
            }
        }

        return new ConfigurationException($"invalid duration {originalText}");
    }

    #endregion
}
