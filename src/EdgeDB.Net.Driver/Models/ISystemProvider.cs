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

internal class BaseDefaultSystemProvider : ISystemProvider
{
    public virtual char DirectorySeparatorChar
        => Path.DirectorySeparatorChar;

    public virtual bool DirectoryExists(string dir)
        => Directory.Exists(dir);

    public virtual DirectoryInfo? DirectoryGetParent(string dir)
        => Directory.GetParent(dir);

    public virtual string CombinePaths(params string[] paths)
        => Path.Combine(paths);

    public virtual string GetFullPath(string path)
        => Path.GetFullPath(path);

    public virtual string GetHomeDir()
        => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    public virtual bool IsOSPlatform(OSPlatform platform)
        => RuntimeInformation.IsOSPlatform(platform);

    public virtual bool IsRooted(string path)
        => Path.IsPathRooted(path);

    public virtual string? GetEnvVariable(string name)
        => Environment.GetEnvironmentVariable(name);

    public virtual bool FileExists(string path)
        => File.Exists(path);

    public virtual string FileReadAllText(string path)
        => File.ReadAllText(path);
}

internal sealed class DefaultSystemProvider : BaseDefaultSystemProvider
{
}
