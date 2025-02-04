using System.Collections;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace Gel.Abstractions;

internal interface ISystemProvider
{
    char DirectorySeparatorChar { get; }
    string GetHomeDir();
    string GetCurrentDirectory();
    bool IsOSPlatform(OSPlatform platform);
    string CombinePaths(params string[] paths);
    string GetFullPath(string path);
    bool DirectoryExists(string dir);
    DirectoryInfo? DirectoryGetParent(string dir);
    bool IsRooted(string path);
    string? GetEnvVariable(string name);
    bool FileExists(string path);
    string FileReadAllText(string path);
    void WriteWarning(string message);

    public virtual bool GetGelEnvVariable(string key, out string name, out string value)
    {
        string edgedbKey = $"EDGEDB_{key}";
        string? edgedbVal = GetEnvVariable(edgedbKey);
        string gelKey = $"GEL_{key}";
        string? gelVal = GetEnvVariable(gelKey);
        if (edgedbVal is not null && gelVal is not null)
        {
            WriteWarning($"Both GEL_{key} and EDGEDB_{key} are set; EDGEDB_{key} will be ignored");
        }

        if (gelVal is not null)
        {
            name = gelKey;
            value = gelVal;
            return true;
        }
        else if (edgedbVal is not null)
        {
            name = edgedbKey;
            value = edgedbVal;
            return true;
        }
        else
        {
            name = string.Empty;
            value = string.Empty;
            return false;
        }
    }
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

    public virtual string GetCurrentDirectory()
        => Environment.CurrentDirectory;

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

    public virtual void WriteWarning(string message)
        => Console.WriteLine(message);
}

internal sealed class DefaultSystemProvider : BaseDefaultSystemProvider
{
}
