using CommandLine;
using CommandLine.Text;
using EdgeDB.CLI;
using EdgeDB.CLI.Arguments;
using Serilog;

// intialize our logger
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Console()
    .CreateLogger();

// find all types that extend the 'ICommand' interface.
var commands = typeof(Program).Assembly.GetTypes().Where(x => x.GetInterfaces().Any(x => x == typeof(ICommand)));

// create our command line arg parser with no default help writer.
var parser = new Parser(x =>
{
    x.HelpWriter = null;
});

// parse the 'args'.
ParserResult<object> result;

try
{
    result = parser.ParseArguments(args, commands.ToArray());
}
catch (Exception x)
{
    Log.Logger.Error(x, "Failed to parse args: {@arg}", args);
    return;
}

try
{
    // execute the parsed result if it is a command.
    var commandResult = await result.WithParsedAsync<ICommand>(x =>
    {
        // if the command supports log args, change the log level for our logger.
        if(x is LogArgs logArgs)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(logArgs.LogLevel)
                .WriteTo.Console()
                .CreateLogger();
        }

        // execute the command with the logger.
        return x.ExecuteAsync(Log.Logger);
    });

    // if the result was not parsed to a valid command.
    result.WithNotParsed(err =>
    {
        // build the help text.
        var helpText = HelpText.AutoBuild(commandResult, h =>
        {
            h.AdditionalNewLineAfterOption = true;
            h.Heading = "EdgeDB.Net CLI";
            h.Copyright = "Copyright (c) 2022 EdgeDB";

            return h;
        }, e => e, verbsIndex: true);

        // write out the help text.
        Console.WriteLine(helpText);
    });

}
catch (Exception x)
{
    // log the root exception.
    Log.Logger.Fatal(x, "Critical error");
}