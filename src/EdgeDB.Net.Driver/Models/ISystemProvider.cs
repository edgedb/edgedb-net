using System.Runtime.InteropServices;

namespace EdgeDB.Abstractions;

internal interface ISystemProvider
{
    char DirectorySeparatorChar { get; }
    string GetHomeDir();
    bool IsOSPlatform(OSPlatform platform);
    string CombinePaths(params string[] paths);
    string GetFullPath(string path);
    bool DirectoryExists(string dir);
    bool IsRooted(string path);
    string? GetEnvVariable(string name);
}

internal sealed class DefaultSystemProvider : ISystemProvider
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
