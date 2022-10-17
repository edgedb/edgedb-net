using CliWrap;
using EdgeDB.DocGenerator;

// take in the path to the edgedb-net folder
var path = args[0];

var driverPath = Path.Combine(path, "src", "EdgeDB.Net.Driver");
var docsTemp = Path.Combine(path, "docs", "tmp");
try
{
    // build the driver with doc files in release mode
    await Cli.Wrap("dotnet")
        .WithArguments($"build {Path.Combine(driverPath, "EdgeDB.Net.Driver.csproj")} -c Release -f net6.0 --force")
        .WithStandardOutputPipe(PipeTarget.ToStream(Console.OpenStandardOutput()))
        .WithStandardErrorPipe(PipeTarget.ToStream(Console.OpenStandardError()))
        .WithValidation(CommandResultValidation.ZeroExitCode)
        .ExecuteAsync();
}
catch(Exception x)
{
    Console.Error.WriteLine("Build failed");
    Console.Error.WriteLine(x.Message);
    return;
}

try
{
    // copy the doc files to a tmp dir in the docs folder
    var driverXMLPath = Path.Combine(docsTemp, "driver.xml");
    Directory.CreateDirectory(docsTemp);
    File.Move(Path.Combine(driverPath, "bin", "Release", "net6.0", "EdgeDB.Net.Driver.xml"), driverXMLPath, true);

    var data = Parser.Load(driverXMLPath);

    var generator = new Generator(Path.Combine(path, "docs"), data);
    generator.Generate();
}
catch(Exception)
{
    Directory.Delete(docsTemp, true);
    throw;
}
