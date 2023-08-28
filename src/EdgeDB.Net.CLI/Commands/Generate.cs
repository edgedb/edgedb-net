using CommandLine;
using EdgeDB.Binary;
using EdgeDB.CLI.Arguments;
using EdgeDB.CLI.Utils;
using EdgeDB.Generator;
using Newtonsoft.Json;
using Serilog;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace EdgeDB.CLI;

/// <summary>
///     A class representing the <c>generate</c> command.
/// </summary>
[Verb("generate", HelpText = "Generate or updates csharp classes from .edgeql files.")]
public class Generate : ConnectionArguments, ICommand
{
    /// <summary>
    ///     Gets or sets whether or not a class library should be generated.
    /// </summary>
    [Option('p', "project", HelpText = "Whether or not to create the default class library that will contain the generated source code. Enabled by default.")]
    public bool GenerateProject { get; set; } = true;

    /// <summary>
    ///     Gets or sets the output directory the generated source files will be placed.
    /// </summary>
    [Option('o', "output", HelpText = "The output directory for the generated source to be placed. When generating a project, source files will be placed in that projects directory. Default is the current directory")]
    public string? OutputDirectory { get; set; }

    /// <summary>
    ///     Gets or sets the project name/namespace.
    /// </summary>
    [Option('n', "project-name", HelpText = "The name of the generated project and namespace of generated files.")]
    public string GeneratedProjectName { get; set; } = "EdgeDB.Generated";
    
    /// <summary>
    ///     Gets or sets whether or not to start a watch process post-generate.
    /// </summary>
    [Option('w', "watch", HelpText = "Listens for any changes or new edgeql files and (re)generates them automatically.")]
    public bool Watch { get; set; }

    /// <summary>
    ///     Gets or sets whether or not to force regenerate.
    /// </summary>
    [Option('f', "force", HelpText = "Force regeneration of all query files.")]
    public bool Force { get; set; }

    /// <inheritdoc/>
    public async Task ExecuteAsync(ILogger logger)
    {
        // get connection info
        var connection = GetConnection();

        var projectRoot = ProjectUtils.GetProjectRoot();

        OutputDirectory ??= Environment.CurrentDirectory;

        Directory.CreateDirectory(OutputDirectory);

        if (GenerateProject && !Directory.Exists(Path.Combine(OutputDirectory, GeneratedProjectName)))
        {
            logger.Information("Creating project {@ProjectName}...", GeneratedProjectName);
            await ProjectUtils.CreateGeneratedProjectAsync(OutputDirectory, GeneratedProjectName);
        }
        
        if(GenerateProject)
            OutputDirectory = Path.Combine(OutputDirectory, GeneratedProjectName);

        if (Watch)
        {
            var existing = ProjectUtils.GetWatcherProcess(projectRoot);

            if (existing is not null)
            {
                logger.Warning("Watching already running");
                return;
            }

            logger.Information("Starting file watcher...");
            var pid = ProjectUtils.StartBackgroundWatchProcess(connection, projectRoot, OutputDirectory, GeneratedProjectName);
            logger.Information("File watcher process started, PID: {@PID}", pid);
            return;
        }

        // find edgeql files
        var edgeqlFiles = ProjectUtils.GetTargetEdgeQLFiles(projectRoot).ToArray();

        // error if any are the same name
        var groupFileNames = edgeqlFiles.GroupBy(x => Path.GetFileNameWithoutExtension(x));
        if(groupFileNames.Any(x => x.Count() > 1))
        {
            foreach(var conflict in groupFileNames.Where(x => x.Count() > 1))
            {
                logger.Fatal($"{{@Count}} files contain the same name ({string.Join(" - ", conflict.Select(x => x))})", conflict.Count());
            }

            return;
        }

        var generator = new CodeGenerator(connection);
        var context = new GeneratorContext(OutputDirectory, GeneratedProjectName);

        logger.Information("Generating {@FileCount} files...", edgeqlFiles.Length);

        await generator.GenerateAsync(edgeqlFiles, context, default, Force);

        logger.Information("Generation complete!");
    }
}
