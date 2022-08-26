using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace EdgeDB.Utils
{
    internal class ConfigUtils
    {
        public static string EdgeDBConfigDir
            => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Path.Combine(GetEdgeDBBasePath(), "config")
                : GetEdgeDBBasePath();

        public static string CredentialsDir
            => Path.Combine(EdgeDBConfigDir, "credentials");

        private static string GetEdgeDBKnownBasePath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EdgeDB");

            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Application Support", "edgedb");

            else
            {
                var xdgConfigDir = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");

                if (xdgConfigDir is null || !Path.IsPathRooted(xdgConfigDir))
                    xdgConfigDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");

                return Path.Combine(xdgConfigDir, "edgedb");
            }
        }
        private static string GetEdgeDBBasePath()
        {
            var basePath = GetEdgeDBKnownBasePath();
            return Directory.Exists(basePath) ? basePath : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".edgedb");
        }

        public static string GetInstanceProjectDirectory(string projectDir)
        {
            var fullPath = Path.GetFullPath(projectDir);
            var baseName = projectDir.Split(Path.DirectorySeparatorChar).Last();
            string hash = "";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !fullPath.StartsWith("\\\\"))
                fullPath = "\\\\?\\" + fullPath;

            using (var sha1 = SHA1.Create())
                hash = HexConverter.ToHex(sha1.ComputeHash(Encoding.UTF8.GetBytes(fullPath)));

            return Path.Combine(EdgeDBConfigDir, "projects", $"{baseName}-{hash.ToLower()}");
        }
    }
}
