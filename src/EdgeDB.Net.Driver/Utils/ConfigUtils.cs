using EdgeDB.Abstractions;
using EdgeDB.Models;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace EdgeDB.Utils;

internal static class ConfigUtils
{
    internal static ISystemProvider DefaultPlatformProvider { get; } = new DefaultSystemProvider();

    private static string GetEdgeDBKnownBasePath(ISystemProvider? platform)
    {
        platform ??= DefaultPlatformProvider;

        if (platform.IsOSPlatform(OSPlatform.Windows))
            return platform.CombinePaths(platform.GetHomeDir(), "AppData", "Local", "EdgeDB");
        if (platform.IsOSPlatform(OSPlatform.OSX))
            return platform.CombinePaths(platform.GetHomeDir(), "Library", "Application Support", "edgedb");
        var xdgConfigDir = platform.GetEnvVariable("XDG_CONFIG_HOME");

        if (xdgConfigDir is null || !platform.IsRooted(xdgConfigDir))
            xdgConfigDir = platform.CombinePaths(platform.GetHomeDir(), ".config");

        return platform.CombinePaths(xdgConfigDir, "edgedb");
    }

    private static string GetEdgeDBBasePath(ISystemProvider? platform)
    {
        platform ??= DefaultPlatformProvider;

        var basePath = GetEdgeDBKnownBasePath(platform);
        return platform.DirectoryExists(basePath)
            ? basePath
            : platform.CombinePaths(platform.GetHomeDir(), ".edgedb");
    }

    public static string GetInstanceProjectDirectory(string projectDir, ISystemProvider? platform)
    {
        platform ??= DefaultPlatformProvider;

        var fullPath = platform.GetFullPath(projectDir);
        var baseName = projectDir.Split(platform.DirectorySeparatorChar).Last();
        var hash = "";

        if (platform.IsOSPlatform(OSPlatform.Windows) && !fullPath.StartsWith("\\\\"))
            fullPath = "\\\\?\\" + fullPath;

        using (var sha1 = SHA1.Create())
            hash = HexConverter.ToHex(sha1.ComputeHash(Encoding.UTF8.GetBytes(fullPath)));

        return platform.CombinePaths(GetEdgeDBConfigDir(platform), "projects", $"{baseName}-{hash.ToLower()}");
    }

    public static string GetEdgeDBConfigDir(ISystemProvider? platform)
        => (platform ?? DefaultPlatformProvider).IsOSPlatform(OSPlatform.Windows)
            ? (platform ?? DefaultPlatformProvider).CombinePaths(GetEdgeDBBasePath(platform), "config")
            : GetEdgeDBBasePath(platform);

    public static string GetCredentialsDir(ISystemProvider? platform)
        => (platform ?? DefaultPlatformProvider).CombinePaths(GetEdgeDBConfigDir(platform), "credentials");

    public static bool TryResolveInstanceTOML([NotNullWhen(true)] out string? tomlPath, ISystemProvider? platform)
    {
        platform ??= DefaultPlatformProvider;

        return TryResolveInstanceTOML(platform.GetCurrentDirectory(), out tomlPath, platform);
    }

    public static bool TryResolveInstanceTOML(string cdir, [NotNullWhen(true)] out string? tomlPath, ISystemProvider? platform)
    {
        platform ??= DefaultPlatformProvider;

        var dir = cdir;

        while (true)
        {
            var target = platform.CombinePaths(dir!, "edgedb.toml");

            if (platform.FileExists(target))
            {
                tomlPath = target;
                return true;
            }


            var parent = platform.DirectoryGetParent(dir!);

            if (parent is null || !parent.Exists)
                break;


            dir = parent.FullName;
        }

        tomlPath = null;
        return false;
    }

    public static bool TryResolveProjectDatabase(string stashDir, [NotNullWhen(true)] out string? database, ISystemProvider? platform)
    {
        platform ??= DefaultPlatformProvider;

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

    public static bool TryResolveInstanceCloudProfile(out string? profile, out string? linkedInstanceName, ISystemProvider? platform)
    {
        platform ??= DefaultPlatformProvider;

        profile = null;
        linkedInstanceName = null;

        if (!TryResolveInstanceTOML(out var toml, platform))
            return false;

        var stashDir = GetInstanceProjectDirectory(platform.DirectoryGetParent(toml)!.FullName!, platform);

        return TryResolveInstanceCloudProfile(stashDir, out profile, out linkedInstanceName, platform);
    }

    public static bool TryResolveInstanceCloudProfile(string stashDir, out string? profile,
        out string? linkedInstanceName, ISystemProvider? platform)
    {
        platform ??= DefaultPlatformProvider;

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

    public static CloudProfile ReadCloudProfile(string profile, ISystemProvider? platform)
    {
        platform ??= DefaultPlatformProvider;

        var profilePath = platform.CombinePaths(GetEdgeDBConfigDir(platform), "cloud-credentials", $"{profile}.json");

        if (!platform.FileExists(profilePath))
            throw new ConfigurationException($"Unknown cloud profile '{profile}'");

        return JsonConvert.DeserializeObject<CloudProfile>(platform.FileReadAllText(profilePath))!;
    }
}
