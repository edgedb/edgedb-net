using CommandLine;
using EdgeDB.DotnetTool;

var commands = typeof(Program).Assembly.GetTypes().Where(x => x.GetInterfaces().Any(x => x == typeof(ICommand)));

Parser.Default.ParseArguments(args, commands.ToArray()).WithParsed<ICommand>(t => t.Execute());
