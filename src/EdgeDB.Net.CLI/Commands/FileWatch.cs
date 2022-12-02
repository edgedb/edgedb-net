using CommandLine;
using EdgeDB.CLI;
using EdgeDB.CLI.Arguments;
using EdgeDB.CLI.Generator;
using EdgeDB.CLI.Generator.Models;
using EdgeDB.CLI.Utils;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EdgeDB.CLI;

/// <summary>
///     A class representing the <c>watch</c> command.
/// </summary>
[Verb("watch", HelpText = "Configure the file watcher")]
public class FileWatch : ConnectionArguments, ICommand
{
    /// <summary>
    ///     Gets or sets the directory to watch for edgeql files.
    /// </summary>
    [Option('t', "directory", HelpText = "The directory to watch for .edgeql files.")]
    public string? Directory { get; set; }

    /// <summary>
    ///     Gets or sets whether or not to kill a already running watcher.
    /// </summary>
    [Option('k', "kill", SetName = "functions", HelpText = "Kill the current running watcher for the project")]
    public bool Kill { get; set; }

    [Option('b', "background", SetName = "functions", HelpText = "Runs the watcher in a background process")]
    public bool Background { get; set; }

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

    private readonly FileSystemWatcher _watcher = new();
    private readonly Dictionary<string, string> _latestGenerations = new();
    private readonly SemaphoreSlim _mutex = new(1, 1);
    private readonly ConcurrentStack<GenerationTargetInfo> _writeQueue = new();
    private EdgeDBTcpClient? _client;
    private TaskCompletionSource _writeDispatcher = new();
    private string? _watchDirectory;
    private ILogger? _logger;

    public async Task ExecuteAsync(ILogger logger)
    {
        _logger = logger;

        var connection = GetConnection();

        _client = new(connection, new() { Logger = logger.CreateClientLogger() }, ClientPoolHolder.Empty);

        _watchDirectory = Directory ?? (ProjectUtils.TryGetProjectRoot(out var root) ? root : Environment.CurrentDirectory);
        OutputDirectory ??= Path.Combine(Environment.CurrentDirectory, GeneratedProjectName);

        // check if a watcher is already running
        var watcher = ProjectUtils.GetWatcherProcess(_watchDirectory);

        if (Kill)
        {
            if(watcher is null)
            {
                logger.Error("No watcher process could be found for the target directory {@dir}", _watchDirectory);
                return;
            }
            else
            {
                watcher.Kill();
                logger.Information("Watcher process {@pid} killed", watcher.Id);
                return;
            }
        }
        else if(watcher is not null)
        {
            logger.Error("A watcher is already running for the target directory {@dir}. PID: {@pid}", _watchDirectory, watcher.Id);
            return;
        }



        // is this watcher suppost to run in the background?
        if (Background)
        {
            var pid = ProjectUtils.StartBackgroundWatchProcess(connection, _watchDirectory, OutputDirectory!, GeneratedProjectName);
            logger.Information("Background process started with id {@pid}", pid);
            return;
        }

        _watcher.Path = _watchDirectory;
        _watcher.Filter = "*.edgeql";
        _watcher.IncludeSubdirectories = true;

        _watcher.Error += _watcher_Error;

        _watcher.Changed += (o, e) => CreatedAndUpdated(o, e, false);
        _watcher.Created += (o, e) => CreatedAndUpdated(o, e, true);
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

        ProjectUtils.RegisterProcessAsWatcher(_watchDirectory);

        logger.Information("Watcher started for {@Dir}", _watchDirectory);

        while (true)
        {
            _logger!.Debug("Waiting for dispatch");
            await _writeDispatcher.Task;

            while (_writeQueue.TryPop(out var info))
            {
                if(_latestGenerations.TryGetValue(info.EdgeQLFilePath!, out var hash) && hash == info.EdgeQLHash)
                    _logger.Information("Skipping {@file}: already at latest generation ({@hash})", info.EdgeQLFilePath, info.EdgeQLHash[..7]);
                else
                {
                    await GenerateAsync(info);
                    _latestGenerations[info.EdgeQLFilePath!] = info.EdgeQLHash!;
                }
            }

            _logger!.Debug("Reseting dispatcher...");
            _writeDispatcher = new();
        }
    }

    private bool IsValidFile(string path)
        => !path.Contains(Path.Combine("dbschema", "migrations"));

    internal async Task GenerateAsync(GenerationTargetInfo info)
    {
        await _mutex.WaitAsync().ConfigureAwait(false);

        _logger!.Debug("Entering generation step for {@file}. Hash: {@hash}", info.EdgeQLFilePath, info.EdgeQLHash);
        try
        {
            // check if the file is a valid file
            if (!IsValidFile(info.EdgeQLFilePath!))
            {
                _logger.Information("Skipping {@file}: invalid target", info.EdgeQLFilePath);

                return;
            }

            await _client!.ConnectAsync();

            try
            {
                _logger.Debug("Parsing {@file}...", info.EdgeQLFilePath);
                var result = await CodeGenerator.ParseAndGenerateAsync(_client, OutputDirectory!, GeneratedProjectName!, info);
                _logger.Debug("Completed parse of {@file} -> {@target}", info.EdgeQLFilePath, info.TargetFilePath);
                File.WriteAllText(info.TargetFilePath!, result.Code);
                _logger!.Information($"{(info.WasCreated ? "Created" : "Updated")} {{@info}}", info.TargetFilePath);
            }
            catch (EdgeDBErrorException err)
            {
                // error with file
                _logger!.Error(err, "Failed to generate {@file}", info.EdgeQLFilePath);
            }
        }
        catch (Exception x)
        {
            _logger!.Fatal(x, "Failed to generate {@file}", info.EdgeQLFilePath);
        }
        finally
        {
            _mutex.Release();
        }
    }

    private void CreatedAndUpdated(object sender, FileSystemEventArgs e, bool created)
    {
        if (!FileUtils.WaitForHotFile(e.FullPath))
            return;

        // wait an extra second to make sure the file is fully written
        Thread.Sleep(1000);

        var info = CodeGenerator.GetTargetInfo(e.FullPath, OutputDirectory!);

        if (info.IsGeneratedTargetExistsAndIsUpToDate())
            return;

        info.WasCreated = created;

        _logger!.Debug("Dispatching write task for {@file}", e.FullPath);
        _writeQueue.Push(info);
        var dispatchResult = _writeDispatcher.TrySetResult();
        _logger!.Debug("Dispatch result: {@r}", dispatchResult);

        if (!dispatchResult)
        {
            _logger!.Error("Failed to dispatch file change");
        }
    }

    private void _watcher_Deleted(object sender, FileSystemEventArgs e)
    {
        if (!IsValidFile(e.FullPath))
            return;

        // get the generated file name
        var path = Path.Combine(OutputDirectory!, $"{Path.GetFileNameWithoutExtension(e.FullPath)}.g.cs");

        if (File.Exists(path))
            File.Delete(path);

        _logger!.Information("Deleted {@file}", path);
    }

    private void _watcher_Renamed(object sender, RenamedEventArgs e)
    {
        if (!IsValidFile(e.FullPath))
            return;

        var oldPath = Path.Combine(OutputDirectory!, $"{Path.GetFileNameWithoutExtension(e.OldFullPath)}.g.cs");
        var newPath = Path.Combine(OutputDirectory!, $"{Path.GetFileNameWithoutExtension(e.FullPath)}.g.cs");

        if (File.Exists(oldPath))
            File.Move(oldPath, newPath);

        _logger!.Information("Renamed {@old} to {@new}", oldPath, newPath);
    }

    private void _watcher_Error(object sender, ErrorEventArgs e)
    {
        _logger!.Error(e.GetException(), "A file watch error occured");
    }
}