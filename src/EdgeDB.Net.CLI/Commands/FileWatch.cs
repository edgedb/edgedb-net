using CommandLine;
using EdgeDB.CLI;
using EdgeDB.CLI.Arguments;
using EdgeDB.CLI.Utils;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.CLI;

/// <summary>
///     A class representing the <c>watch</c> command.
/// </summary>
[Verb("watch", HelpText = "Configure the file watcher")]
public class FileWatch : ConnectionArguments, ICommand
{
    /// <summary>
    ///     Gets or sets whether or not to kill a already running watcher.
    /// </summary>
    [Option('k', "kill", SetName = "functions", HelpText = "Kill the current running watcher for the project")]
    public bool Kill { get; set; }

    /// <summary>
    ///     Gets or sets whether or not to start a watcher.
    /// </summary>
    [Option('s', "start", SetName = "functions", HelpText = "Start a watcher for the current project")]
    public bool Start { get; set; }

    /// <summary>
    ///     Gets or sets the output directory to place the generated source files.
    /// </summary>
    [Option('o', "output", HelpText = "The output directory for the generated source to be placed.")]
    public string? OutputDirectory { get; set; }

    /// <summary>
    ///     Gets or sets the project name/namespace of generated files.
    /// </summary>
    [Option('n', "project-name", HelpText = "The name of the generated project and namespace of generated files.")]
    public string GeneratedProjectName { get; set; } = "EdgeDB.Generated";

    /// <inheritdoc/>
    public Task ExecuteAsync(ILogger logger)
    {
        // get project root
        var root = ProjectUtils.GetProjectRoot();
        var watcher = ProjectUtils.GetWatcherProcess(root);
        logger.Debug("Watcher?: {@Watcher} Project root: {@Root}", watcher is not null, root);
        try
        {
            if (!Kill && !Start)
            {
                // display information about the current watcher
                if (watcher is null)
                {
                    logger.Information("No file watcher is running for {@Dir}", root);
                    return Task.CompletedTask;
                }

                logger.Information("File watcher is watching {@Dir}", Path.Combine(root, "*.edgeql"));
                logger.Information("Process ID: {@Id}", watcher.Id);

                return Task.CompletedTask;
            }

            if (Kill)
            {
                if (watcher is null)
                {
                    logger.Error("No watcher is running for {@Dir}", root);
                    return Task.CompletedTask;
                }

                watcher.Kill();
                logger.Information("Watcher process {@PID} kiled", watcher.Id);

                return Task.CompletedTask;
            }

            if (Start)
            {
                if (watcher is not null)
                {
                    logger.Error("Watcher already running! Process ID: {@PID}", watcher.Id);
                    return Task.CompletedTask;
                }

                var connection = GetConnection();

                OutputDirectory ??= Path.Combine(Environment.CurrentDirectory, GeneratedProjectName);

                var pid = ProjectUtils.StartWatchProcess(connection, root, OutputDirectory, GeneratedProjectName);

                logger.Information("Watcher process started, PID: {@PID}", pid);
            }

            return Task.CompletedTask;
        }
        catch (Exception x)
        {
            logger.Error(x, "Failed to run watcher command");
            return Task.CompletedTask;
        }
    }
}

[Verb("file-watch-internal", Hidden = true)]
internal class FileWatchInternal : ICommand
{
    [Option("connection")]
    public string? Connection { get; set; }

    [Option("dir")]
    public string? Dir { get; set; }

    [Option("output")]
    public string? Output { get; set; }

    [Option("namespace")]
    public string? Namespace { get; set; }

    private readonly FileSystemWatcher _watcher = new();
    private EdgeDBTcpClient? _client;
    private readonly SemaphoreSlim _mutex = new(1, 1);
    private readonly ConcurrentStack<EdgeQLParser.GenerationTargetInfo> _writeQueue = new();
    private TaskCompletionSource _writeDispatcher = new();

    public async Task ExecuteAsync(ILogger logger)
    {
        if (Connection is null)
            throw new InvalidOperationException("Connection must be specified");

        _client = new(JsonConvert.DeserializeObject<EdgeDBConnection>(Connection)!, new(), ClientPoolHolder.Empty);

        _watcher.Path = Dir!;
        _watcher.Filter = "*.edgeql";
        _watcher.IncludeSubdirectories = true;

        _watcher.Error += _watcher_Error;

        _watcher.Changed += CreatedAndUpdated;
        _watcher.Created += CreatedAndUpdated;
        _watcher.Deleted += _watcher_Deleted;
        _watcher.Renamed += _watcher_Renamed;

        _watcher.NotifyFilter = NotifyFilters.Attributes
                             | NotifyFilters.CreationTime
                             | NotifyFilters.DirectoryName
                             | NotifyFilters.FileName
                             | NotifyFilters.LastAccess
                             | NotifyFilters.LastWrite
                             | NotifyFilters.Security
                             | NotifyFilters.Size;

        _watcher.EnableRaisingEvents = true;

        ProjectUtils.RegisterProcessAsWatcher(Dir!);

        logger.Information("Watcher started for {@Dir}", Output);

        while (true)
        {
            await _writeDispatcher.Task;

            while (_writeQueue.TryPop(out var info))
            {
                await GenerateAsync(info);
            }

            _writeDispatcher = new();
        }
    }

    private bool IsValidFile(string path)
        => !path.StartsWith(Path.Combine(Dir!, "dbschema", "migrations"));

    public async Task GenerateAsync(EdgeQLParser.GenerationTargetInfo info)
    {
        await _mutex.WaitAsync().ConfigureAwait(false);

        try
        {
            // check if the file is a valid file
            if (!IsValidFile(info.EdgeQLFilePath!))
                return;

            await _client!.ConnectAsync();

            try
            {
                var result = await EdgeQLParser.ParseAndGenerateAsync(_client, Namespace!, info);
                File.WriteAllText(info.TargetFilePath!, result.Code);
            }
            catch (EdgeDBErrorException err)
            {
                // error with file
                Console.WriteLine(err.Message);
            }
        }
        catch (Exception x)
        {
            Console.WriteLine(x);
        }
        finally
        {
            _mutex.Release();
        }
    }

    private void CreatedAndUpdated(object sender, FileSystemEventArgs e)
    {
        if (!FileUtils.WaitForHotFile(e.FullPath))
            return;

        // wait an extra second to make sure the file is fully written
        Thread.Sleep(1000);

        var info = EdgeQLParser.GetTargetInfo(e.FullPath, Output!);

        if (info.IsGeneratedTargetExistsAndIsUpToDate())
            return;

        _writeQueue.Push(info);
        _writeDispatcher.TrySetResult();
    }

    private void _watcher_Deleted(object sender, FileSystemEventArgs e)
    {
        if (!IsValidFile(e.FullPath))
            return;

        // get the generated file name
        var path = Path.Combine(Output!, $"{Path.GetFileNameWithoutExtension(e.FullPath)}.g.cs");

        if (File.Exists(path))
            File.Delete(path);
    }

    private void _watcher_Renamed(object sender, RenamedEventArgs e)
    {
        if (!IsValidFile(e.FullPath))
            return;

        var oldPath = Path.Combine(Output!, $"{Path.GetFileNameWithoutExtension(e.OldFullPath)}.g.cs");
        var newPath = Path.Combine(Output!, $"{Path.GetFileNameWithoutExtension(e.FullPath)}.g.cs");

        if (File.Exists(oldPath))
            File.Move(oldPath, newPath);
    }

    private void _watcher_Error(object sender, ErrorEventArgs e)
    {
        Console.Error.WriteLine($"An error occored: {e.GetException()}");
    }
}