using CommandLine;
using EdgeDB.CLI.Arguments;
using EdgeDB.CLI.Utils;
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
    ///     Gets or sets whether or not to force (re)generate source files.
    /// </summary>
    [Option('f', "force", HelpText = "Force regeneration of files")]
    public bool Force { get; set; }

    /// <summary>
    ///     Gets or sets whether or not to start a watch process post-generate.
    /// </summary>
    [Option("watch", HelpText = "Listens for any changes or new edgeql files and (re)generates them automatically")]
    public bool Watch { get; set; }

    /// <inheritdoc/>
    public async Task ExecuteAsync(ILogger logger)
    {
        // get connection info
        var connection = GetConnection();

        // create the client
        var client = new EdgeDBTcpClient(connection, new(), ClientPoolHolder.Empty);

        logger.Information("Connecting to {@Host}:{@Port}...", connection.Hostname, connection.Port);
        await client.ConnectAsync();

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
        
        logger.Information("Generating {@FileCount} files...", edgeqlFiles.Length);

        for(int i = 0; i != edgeqlFiles.Length; i++)
        {
            var file = edgeqlFiles[i];
            var info = EdgeQLParser.GetTargetInfo(file, OutputDirectory);

            if (!Force && info.IsGeneratedTargetExistsAndIsUpToDate())
            {
                logger.Warning("Skipping {@File}: File already generated and up-to-date.", file);
                continue;
            }

            try
            {
                var result = await EdgeQLParser.ParseAndGenerateAsync(client, GeneratedProjectName, info);
                File.WriteAllText(info.TargetFilePath!, result.Code);
            }
            catch (EdgeDBErrorException error)
            {
                logger.Error("Skipping {@File}: Failed to parse - {@Message} at line {@Line} column {@Column}", 
                    file,
                    error.Message,
                    error.ErrorResponse.Attributes.FirstOrDefault(x => x.Code == 65523).ToString() ?? "??", 
                    error.ErrorResponse.Attributes.FirstOrDefault(x => x.Code == 65524).ToString() ?? "??");
                continue;
            }

            logger.Debug("{@EdgeQL} => {@CSharp}", file, info.TargetFilePath);
        }

        logger.Information("Generation complete!");

        if(Watch)
        {
            var existing = ProjectUtils.GetWatcherProcess(projectRoot);

            if(existing is not null)
            {
                logger.Warning("Watching already running");
                return;
            }

            logger.Information("Starting file watcher...");
            var pid = ProjectUtils.StartWatchProcess(connection, projectRoot, OutputDirectory, GeneratedProjectName);
            logger.Information("File watcher process started, PID: {@PID}", pid);
        }
    }
}