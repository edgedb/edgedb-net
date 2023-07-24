using CommandLine;
using EdgeDB.CLI;
using EdgeDB.CLI.Arguments;
using EdgeDB.CLI.Utils;
using EdgeDB.Generator;
using EdgeDB.Generator.TypeGenerators;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
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
    private enum Operation
    {
        Created,
        Updated,
        Deleted,
        Renamed
    }

    private sealed record WatchInstruction(string Source, string? Destination, Operation Operation, int Attempts = 0);

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

    [Option("max-retries", HelpText = "The max number of attempts per failed operation.")]
    public int MaxOperationRetries { get; set; } = 10;

    private readonly FileSystemWatcher _watcher = new();
    private readonly Dictionary<string, string> _latestGenerations = new();
    private readonly SemaphoreSlim _mutex = new(1, 1);
    private readonly ConcurrentStack<WatchInstruction> _instructionQueue = new();

    private TaskCompletionSource _instructionDispatcher = new();
    private string? _watchDirectory;
    private ILogger? _logger;

    public async Task ExecuteAsync(ILogger logger)
    {
        _logger = logger;

        var connection = GetConnection();

        var generator = new CodeGenerator(connection);

        _watchDirectory = Directory ?? (ProjectUtils.TryGetProjectRoot(out var root) ? root : Environment.CurrentDirectory);
        OutputDirectory ??= Path.Combine(Environment.CurrentDirectory, GeneratedProjectName);

        // check if a watcher is already running
        var watcher = ProjectUtils.GetWatcherProcess(_watchDirectory);

        if (Kill)
        {
            if (watcher is null)
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
        else if (watcher is not null)
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

        // init generator will full structure generation
        var context = new GeneratorContext(OutputDirectory, GeneratedProjectName);
        var typeGenerator = await generator.GetTypeGeneratorAsync(default);

        await RunFullGenerationAsync(_watchDirectory, generator, context, typeGenerator);

        CleanFiles(context.OutputDirectory, generator, typeGenerator);

        _watcher.Path = _watchDirectory;
        _watcher.Filter = "*.edgeql";
        _watcher.IncludeSubdirectories = true;

        _watcher.Error += _watcher_Error;

        _watcher.Changed += (o, e) => FileCreatedOrUpdated(o, e, false);
        _watcher.Created += (o, e) => FileCreatedOrUpdated(o, e, true);
        _watcher.Deleted += FileDeleted;
        _watcher.Renamed += FileRenamed;

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
            await _instructionDispatcher.Task;

            while (_instructionQueue.TryPop(out var step))
            {
                _logger.Debug("Popped instruction {@Step}", step);

                try
                {
                    switch (step.Operation)
                    {
                        case Operation.Updated or Operation.Created:
                            {
                                var info = GeneratorTargetInfo.FromFile(step.Source);

                                if (_latestGenerations.TryGetValue(info.Path!, out var hash) && hash == info.Hash)
                                    _logger.Information("Skipping {@file}: already at latest generation ({@hash})", info.Path, info.Hash[..7]);
                                else
                                {
                                    if (string.IsNullOrEmpty(info.EdgeQL))
                                    {
                                        _logger.Information("Skipping {@File}, contents is empty", info.Path);
                                        return;
                                    }

                                    using var tokenSource = new CancellationTokenSource(10000);

                                    await GenerateAndCleanAsync(info, generator, context, typeGenerator, tokenSource.Token);

                                    _logger.Information("Calling post-process generation task...");

                                    await typeGenerator.PostProcessAsync(context);

                                    _latestGenerations[info.Path!] = info.Hash!;

                                    _logger.Debug("Generation complete for {@Target}", info.FileName);

                                }
                            }
                            break;
                        case Operation.Deleted:
                            {
                                if (step.Attempts > 0)
                                {
                                    _logger.Information("Attempt retry backoff...");
                                    await Task.Delay(1000);
                                }

                                // TODO
                            }
                            break;
                        case Operation.Renamed:
                            {
                                if (step.Destination is null)
                                {
                                    _logger.Fatal("Unknown rename destination");
                                    return;
                                }

                                if (step.Attempts > 0)
                                {
                                    _logger.Information("Attempt retry backoff...");
                                    await Task.Delay(1000);
                                }

                                var oldGeneratedFile = Path.Combine(context.OutputDirectory, $"{Path.GetFileNameWithoutExtension(step.Source)}.g.cs");

                                if (!RetryingDelete(oldGeneratedFile, step))
                                    break;

                                // TODO
                               

                                
                            }
                            break;
                    }
                }
                catch(Exception x)
                {
                    _logger.Fatal(x, "Unhandled exception in dispatcher");
                }
            }

            _logger!.Debug("Reseting dispatcher...");
            _instructionDispatcher = new();
        }
    }

    private static async Task RunFullGenerationAsync(string projectRoot, CodeGenerator generator, GeneratorContext context, ITypeGenerator typeGenerator)
    {
        // find edgeql files
        var edgeqlFiles = ProjectUtils.GetTargetEdgeQLFiles(projectRoot).ToArray();

        // error if any are the same name
        var groupFileNames = edgeqlFiles.GroupBy(x => Path.GetFileNameWithoutExtension(x));
        if (groupFileNames.Any(x => x.Count() > 1))
        {
            foreach (var conflict in groupFileNames.Where(x => x.Count() > 1))
            {
                Serilog.Log.ForContext<FileWatch>().Fatal($"{{@Count}} files contain the same name ({string.Join(" - ", conflict.Select(x => x))})", conflict.Count());
            }

            return;
        }

        await generator.GenerateAsync(edgeqlFiles, context, default, true, typeGenerator);
    }

    private bool RetryingDelete(string file, WatchInstruction step)
    {
        try
        {
            if(File.Exists(file))
                File.Delete(file);

            return true;
        }
        catch (Exception x)
        {
            if (step.Attempts > MaxOperationRetries)
            {
                _logger!.Fatal(x, "Failed to delete file {@File} after {@Retries} attempts", file, step.Attempts);
                return false;
            }

            _logger!.Warning(x, "Failed to delete file {@File}, attempt {@Current}/{@Max}, requeuing task", file, step.Attempts, MaxOperationRetries);

            _instructionQueue.Push(step with
            {
                Attempts = step.Attempts + 1
            });

            return false;
        }
    }

    private static bool IsValidFile(string path)
        => !path.Contains(Path.Combine("dbschema", "migrations"));

    internal async Task GenerateAndCleanAsync(
        GeneratorTargetInfo info, CodeGenerator generator, GeneratorContext context,
        ITypeGenerator typeGenerator, CancellationToken token, bool force = false)
    {
        await _mutex.WaitAsync(token).ConfigureAwait(false);

        _logger!.Debug("Entering generation step for {@file}. Hash: {@hash}", info.Path, info.Hash);
        try
        {
            // check if the file is a valid file
            if (!IsValidFile(info.Path!))
            {
                _logger.Information("Skipping {@file}: invalid target", info.Path);
                return;
            }

            _logger.Debug("Generating {@file}...", info.Path);

            try
            {
                await generator.GenerateAsync(info, context, token, typeGenerator, force);

                CleanFiles(context.OutputDirectory, generator, typeGenerator);

                _logger!.Information("Completed {@info}", info.GetGenerationFileName(context));

            }
            catch (EdgeDBErrorException err)
            {
                // error with file
                _logger!.Error(err, "Failed to generate {@file}", info.Path);
            }
        }
        catch (Exception x)
        {
            _logger!.Fatal(x, "Failed to generate {@file}", info.Path);
        }
        finally
        {
            _mutex.Release();
        }
    }

    private void CleanFiles(string outputDir, CodeGenerator generator, ITypeGenerator typeGenerator)
    {
        var files = System.IO.Directory.GetFiles(outputDir, "*.g.cs", new EnumerationOptions { RecurseSubdirectories = true });
        var generatedFiles = typeGenerator.GetGeneratedFiles();

        var targets = files.Where(x => !generatedFiles.Any(y => y.GeneratedPath == x) && !generator.GeneratedFiles.Any(y => y == x));

        foreach(var target in targets)
        {
            _logger!.Debug("Removing stale generated file {@File}", target);
            File.Delete(target);
        }
    }

    private void FileCreatedOrUpdated(object sender, FileSystemEventArgs e, bool created)
    {
        if (!FileUtils.WaitForHotFile(e.FullPath))
            return;

        // wait an extra second to make sure the file is fully written
        //Thread.Sleep(1000);

        var operation = created ? Operation.Created : Operation.Updated;

        _logger!.Information("[I] -> {@Op} {@file}", operation, Path.GetFileNameWithoutExtension(e.FullPath));

        PushInstruction(operation, e.FullPath);
    }

    private void FileDeleted(object sender, FileSystemEventArgs e)
    {
        if (!IsValidFile(e.FullPath))
            return;

        _logger!.Information("[I] -> Deleted {@file}", Path.GetFileNameWithoutExtension(e.FullPath));

        PushInstruction(Operation.Deleted, e.FullPath);
    }

    private void FileRenamed(object sender, RenamedEventArgs e)
    {
        if (!IsValidFile(e.FullPath))
            return;

        _logger!.Information("[I] -> Updated {@file}", Path.GetFileNameWithoutExtension(e.FullPath));

        PushInstruction(Operation.Renamed, e.OldFullPath, e.FullPath);
    }

    private void _watcher_Error(object sender, ErrorEventArgs e)
    {
        _logger!.Error(e.GetException(), "A file watch error occured");
    }

    private void PushInstruction(Operation operation, string source, string? destination = null)
    {
        _instructionQueue.Push(new(source, destination, operation));

        var dispatchResult = _instructionDispatcher.TrySetResult();

        _logger!.Debug("Dispatch result: {@r}", dispatchResult);

        if (!dispatchResult)
        {
            _logger!.Debug("Got unsuccessful dispatch result for {@Op}, ignoring", operation);
        }
    }
}
