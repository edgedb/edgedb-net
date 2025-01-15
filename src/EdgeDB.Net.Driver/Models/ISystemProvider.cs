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
    DirectoryInfo? DirectoryGetParent(string dir);
    bool IsRooted(string path);
    string? GetEnvVariable(string name);
    bool FileExists(string path);
    string FileReadAllText(string path);
}

internal sealed class DefaultSystemProvider : ISystemProvider
{
    public char DirectorySeparatorChar
        => Path.DirectorySeparatorChar;

    public bool DirectoryExists(string dir)
        => Directory.Exists(dir);

    public DirectoryInfo? DirectoryGetParent(string dir)
        => Directory.GetParent(dir);

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

    public bool FileExists(string path)
        => File.Exists(path);

    public string FileReadAllText(string path)
        => File.ReadAllText(path);
}
