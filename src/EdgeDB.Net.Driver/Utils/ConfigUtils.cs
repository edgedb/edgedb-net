using EdgeDB.Abstractions;
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
    }
}
