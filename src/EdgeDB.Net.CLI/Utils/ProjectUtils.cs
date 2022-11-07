using CliWrap;
using EdgeDB.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.CLI.Utils
{
    /// <summary>
    ///     A utility class containing methods realted to edgedb projects.
    /// </summary>
    internal static class ProjectUtils
    {
        public static string EdgeQLNetDataDir
            => Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create),
                "edgeql-net"
            );

        static ProjectUtils()
        {
            Directory.CreateDirectory(EdgeQLNetDataDir);
        }

        /// <summary>
        ///     Gets the edgedb project root from the current directory.
        /// </summary>
        /// <returns>The project root directory.</returns>
        /// <exception cref="FileNotFoundException">The project could not be found.</exception>
        public static string GetProjectRoot()
        {
            var directory = Environment.CurrentDirectory;
            bool foundRoot = false;

            while (!foundRoot)
            {
                if (
                    !(foundRoot = Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly).Any(x => x.EndsWith($"{Path.DirectorySeparatorChar}edgedb.toml"))) && 
                    (directory = Directory.GetParent(directory!)?.FullName) is null)
                    throw new FileNotFoundException("Could not find edgedb.toml in the current and parent directories");
            }

            return directory;
        }

        public static bool TryGetProjectRoot([MaybeNullWhen(false)] out string root)
        {
            root = null;

            try
            {
                root = GetProjectRoot();
                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        /// <summary>
        ///     Starts a watcher process.
        /// </summary>
        /// <param name="connection">The connection info for the watcher process.</param>
        /// <param name="root">The project root directory.</param>
        /// <param name="outputDir">The output directory for files the watcher generates to place.</param>
        /// <param name="namespace">The namespace for generated files.</param>
        /// <returns>The started watcher process id.</returns>
        public static int StartBackgroundWatchProcess(EdgeDBConnection connection, string root, string outputDir, string @namespace)
        {
            var current = Process.GetCurrentProcess();
            var connString = JsonConvert.SerializeObject(connection).Replace("\"", "\\\"");

            return Process.Start(new ProcessStartInfo
            {
                FileName = current.MainModule!.FileName,
                Arguments = $"watch --raw-connection \"{connString}\" -t \"{root}\" -o \"{outputDir}\" -n \"{@namespace}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WindowStyle = ProcessWindowStyle.Hidden
            })!.Id;
        }

        /// <summary>
        ///     Gets the watcher process for the provided root directory and/or its parents.
        /// </summary>
        /// <param name="root">The project root.</param>
        /// <returns>
        ///     The watcher <see cref="Process"/> or <see langword="null"/> if not found.
        /// </returns>
        public static Process? GetWatcherProcess(string root)
        {
            string? path = root;
            Process? proc = null;
            while(path is not null)
            {
                var target = GetWatcherProcessTargetPath(path);
                if (File.Exists(target))
                {
                    var rawPid = File.ReadAllText(target);

                    if (int.TryParse(rawPid, out var pid) && (proc = Process.GetProcesses().FirstOrDefault(x => x.Id == pid)) is not null)
                        return proc;
                }

                path = Directory.GetParent(path)?.FullName;
            }

            return null;
        }

        /// <summary>
        ///     Registers the current process as the watcher project for the given project root.
        /// </summary>
        /// <param name="root">The project root.</param>
        public static void RegisterProcessAsWatcher(string root)
        {
            var id = Environment.ProcessId;

            if(GetWatcherProcess(root) is not null)
                throw new ArgumentException("Watch directory already has a watcher", nameof(root));

            File.WriteAllText(GetWatcherProcessTargetPath(root), id.ToString());
        }

        private static string GetWatcherProcessTargetPath(string root)
        {
            using var hash = SHA256.Create();

            var name = HexConverter.ToHex(hash.ComputeHash(Encoding.UTF8.GetBytes(root)));

            return Path.Combine(EdgeQLNetDataDir, name);
        }

        /// <summary>
        ///     Creates a dotnet project.
        /// </summary>
        /// <param name="root">The target directory.</param>
        /// <param name="name">The name of the project</param>
        /// <exception cref="IOException">The project failed to be created.</exception>
        public static async Task CreateGeneratedProjectAsync(string root, string name)
        {
            var result = await Cli.Wrap("dotnet")
                .WithArguments($"new classlib --framework \"net6.0\" -n {name}")
                .WithWorkingDirectory(root)
                .WithStandardErrorPipe(PipeTarget.ToStream(Console.OpenStandardError()))
                .WithStandardOutputPipe(PipeTarget.ToStream(Console.OpenStandardOutput()))
                .ExecuteAsync();

            if (result.ExitCode != 0)
                throw new IOException($"Failed to create new project");

            result = await Cli.Wrap("dotnet")
                .WithArguments("add package EdgeDB.Net.Driver")
                .WithWorkingDirectory(Path.Combine(root, name))
                .WithStandardErrorPipe(PipeTarget.ToStream(Console.OpenStandardError()))
                .WithStandardOutputPipe(PipeTarget.ToStream(Console.OpenStandardOutput()))
                .ExecuteAsync();

            if (result.ExitCode != 0)
                throw new IOException($"Failed to create new project");

            // remove default file
            File.Delete(Path.Combine(root, name, "Class1.cs"));
        }

        /// <summary>
        ///     Gets a list of edgeql file paths for the provided root directory.
        /// </summary>
        /// <remarks>
        ///     migration files are ignored.
        /// </remarks>
        /// <param name="root">The root directory to scan for edgeql files.</param>
        /// <returns>
        ///     An <see cref="IEnumerable{T}"/> that enumerates a collection of files ending in .edgeql.
        /// </returns>
        public static IEnumerable<string> GetTargetEdgeQLFiles(string root)
            => Directory.GetFiles(root, "*.edgeql", SearchOption.AllDirectories).Where(x => !x.Contains(Path.Combine("dbschema", "migrations")));
    }
}
