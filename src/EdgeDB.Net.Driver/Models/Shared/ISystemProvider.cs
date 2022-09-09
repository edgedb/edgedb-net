using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Abstractions
{
    internal interface ISystemProvider
    {
        string GetHomeDir();
        bool IsOSPlatform(OSPlatform platform);
        string CombinePaths(params string[] paths);
        string GetFullPath(string path);
        char DirectorySeparatorChar { get; }
        bool DirectoryExists(string dir);
        bool IsRooted(string path);
        string? GetEnvVariable(string name);
    }

    internal class DefaultSystemProvider : ISystemProvider
    {
        public char DirectorySeparatorChar
            => Path.DirectorySeparatorChar;

        public bool DirectoryExists(string dir)
            => Directory.Exists(dir);

        public string CombinePaths(params string[] paths)
            => Path.Combine(paths);

        public string GetFullPath(string path)
            => Path.GetFullPath(path);

        public string GetHomeDir()
            => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        
        public bool IsOSPlatform(OSPlatform platform)
            => RuntimeInformation.IsOSPlatform(platform);

        public bool IsRooted(string path)
            => Path.IsPathRooted(path);

        public string? GetEnvVariable(string name)
            => Environment.GetEnvironmentVariable(name);
    }
}
