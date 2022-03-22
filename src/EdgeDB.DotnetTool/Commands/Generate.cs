using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace EdgeDB.DotnetTool.Commands
{
    [Verb("generate", HelpText = "Generates classes based off of a schema")]
    internal class Generate : ICommand
    {
        [Option('c', "dsn", HelpText = "The DSN connection string to connect to the remote instance", Required = false)]
        public string? ConnectionString { get; set; }

        [Option('f', "file", Required = false, HelpText = "The file location of the schema")]
        public string? FilePath { get; set; }

        [Option('n', "namespace", Required = false, HelpText = "The namespace for the generated code files", Default = "EdgeDB.Generated")]
        public string? Namespace { get; set; }

        [Option('o', "output", Required = false, HelpText = "The output directory for the generated files", Default = "./Generated")]
        public string? OutputDir { get; set; }

        [Option('v', "verbose", HelpText = "Enables verbose output")]
        public bool Verbose { get; set; }

        public void Execute()
        {
            // use either file or connection
            string? schema = null;

            if (FilePath != null)
                schema = File.ReadAllText(FilePath!);
            else if (ConnectionString != null)
            {
                Task.Run(async () =>
                {
                    var client = new EdgeDBTcpClient(EdgeDBConnection.FromDSN(ConnectionString), new EdgeDBConfig
                    {
                        // TODO: config?
                    });

                    await client.ConnectAsync();
                    var result =  await client.QueryAsync($"describe schema as sdl");

                }).GetAwaiter().GetResult();
            }

            if(schema == null)
            {
                Console.Error.WriteLine("Please either specify the schema file (-n) or a connection string (-c)");
                return;
            }

            Console.WriteLine("Generating module...");
            List<Module> modules;
            var serializer = new SerializerBuilder()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitEmptyCollections | DefaultValuesHandling.OmitDefaults)
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            try
            {
                modules = new SchemaReader(schema).Read();
                Console.WriteLine("Parsed schema!");
                if(Verbose)
                    Console.WriteLine(serializer.Serialize(modules));
            }
            catch(Exception x)
            {
                Console.Error.WriteLine($"Failed to read schema: {x}");
                return;
            }

            try
            {
                var builder = new ClassBuilder(OutputDir!, Namespace!);
                foreach (var module in modules)
                {
                    Console.WriteLine($"Generating {module.Name}...");
                    builder.Generate(module, GetValidDotnetName);
                }

            }
            catch(Exception x)
            {
                Console.Error.WriteLine($"Failed to build classes from schema: {x}");
                return;
            }

            Console.WriteLine("Generation succeeded");
        }

        private static string GetValidDotnetName(string str)
        {
            Console.WriteLine($"The name \"{str}\" isn't a valid name in DotNet, what would you want to name it instead?");
            Console.Write("> ");

            while (true)
            {
                var newName = Console.ReadLine();

                if (newName == null)
                    continue;

                if (ClassBuilder.IsValidDotnetName(newName))
                {
                    return newName;
                }
            }
        }
    }
}
