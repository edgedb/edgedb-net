using EdgeDB.Abstractions;
using EdgeDB.Models;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace EdgeDB.Utils
{
    internal static class ConfigUtils
    {
        private static readonly ISystemProvider _defaultPlatformProvider = new DefaultSystemProvider();

        private static string GetEdgeDBKnownBasePath(ISystemProvider? platform = null)
        {
            platform ??= _defaultPlatformProvider;

            if (platform.IsOSPlatform(OSPlatform.Windows))
                return platform.CombinePaths(platform.GetHomeDir(), "AppData", "Local", "EdgeDB");
            else if (platform.IsOSPlatform(OSPlatform.OSX))
                return platform.CombinePaths(platform.GetHomeDir(), "Library", "Application Support", "edgedb");
            else
            {
                var xdgConfigDir = platform.GetEnvVariable("XDG_CONFIG_HOME");

                if (xdgConfigDir is null || !platform.IsRooted(xdgConfigDir))
                    xdgConfigDir = platform.CombinePaths(platform.GetHomeDir(), ".config");

                return platform.CombinePaths(xdgConfigDir, "edgedb");
            }
        }
        private static string GetEdgeDBBasePath(ISystemProvider? platform = null)
        {
            platform ??= _defaultPlatformProvider;
            
            var basePath = GetEdgeDBKnownBasePath(platform);
            return platform.DirectoryExists(basePath)
                ? basePath
                : platform.CombinePaths(platform.GetHomeDir(), ".edgedb");
        }

        public static string GetInstanceProjectDirectory(string projectDir, ISystemProvider? platform = null)
        {
            platform ??= _defaultPlatformProvider;

            var fullPath = platform.GetFullPath(projectDir);
            var baseName = projectDir.Split(platform.DirectorySeparatorChar).Last();
            var hash = "";

            if (platform.IsOSPlatform(OSPlatform.Windows) && !fullPath.StartsWith("\\\\"))
                fullPath = "\\\\?\\" + fullPath;

            using (var sha1 = SHA1.Create())
                hash = HexConverter.ToHex(sha1.ComputeHash(Encoding.UTF8.GetBytes(fullPath)));

            return platform.CombinePaths(GetEdgeDBConfigDir(platform), "projects", $"{baseName}-{hash.ToLower()}");
        }

        public static string GetEdgeDBConfigDir(ISystemProvider? platform = null)
            => (platform ?? _defaultPlatformProvider).IsOSPlatform(OSPlatform.Windows)
                ? (platform ?? _defaultPlatformProvider).CombinePaths(GetEdgeDBBasePath(platform), "config")
                : GetEdgeDBBasePath(platform);
        
        public static string GetCredentialsDir(ISystemProvider? platform = null)
            => (platform ?? _defaultPlatformProvider).CombinePaths(GetEdgeDBConfigDir(platform), "credentials");

        public static bool TryResolveInstanceTOML([NotNullWhen(true)] out string? tomlPath)
            => TryResolveInstanceTOML(Environment.CurrentDirectory, out tomlPath);

        public static bool TryResolveInstanceTOML(string cdir, [NotNullWhen(true)] out string? tomlPath)
        {
            var dir = cdir;

            while (true)
            {
                var target = Path.Combine(dir!, "edgedb.toml");

                if (File.Exists(target))
                {
                    tomlPath = target;
                    return true;
                }
                    

                var parent = Directory.GetParent(dir!);

                if (parent is null || !parent.Exists)
                    break;
                    

                dir = parent.FullName;
            }

            tomlPath = null;
            return false;
        }

        public static bool TryResolveDatabase()

        public static bool TryResolveInstanceCloudProfile(out string? profile, out string? linkedInstanceName)
        {
            profile = null;
            linkedInstanceName = null;

            if (!TryResolveInstanceTOML(out var toml))
                return false;

            var stashDir = GetInstanceProjectDirectory(Directory.GetParent(toml)!.FullName!);

            return TryResolveInstanceCloudProfile(stashDir, out profile, out linkedInstanceName);
        }

        public static bool TryResolveInstanceCloudProfile(string stashDir, out string? profile, out string? linkedInstanceName)
        {
            profile = null;
            linkedInstanceName = null;

            if (!Directory.Exists(stashDir))
                return false;

            var cloudProfilePath = Path.Combine(stashDir, "cloud-profile");

            if (File.Exists(cloudProfilePath))
            {
                profile = File.ReadAllText(cloudProfilePath);
            }    

            var linkedInstancePath = Path.Combine(stashDir, "instance-name");

            if (File.Exists(linkedInstancePath))
            {
                linkedInstanceName = File.ReadAllText(linkedInstancePath);
            }

            return profile is not null || linkedInstanceName is not null;
        }

        public static CloudProfile ReadCloudProfile(string profile, ISystemProvider? platform = null)
        {
            platform ??= _defaultPlatformProvider;

            var profilePath = platform.CombinePaths(GetEdgeDBConfigDir(platform), "cloud-credentials", $"{profile}.json");

            if (!File.Exists(profilePath))
                throw new ConfigurationException($"Unknown cloud profile '{profile}'");

            return JsonConvert.DeserializeObject<CloudProfile>(File.ReadAllText(profilePath))!;
        }
    }
}
