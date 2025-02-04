using CliWrap;
using Gel.DocGenerator;
using System.Reflection;

// take in the path to the gel-net folder
var path = args[0];

#if DEBUG
// if debugging, look for the solution path
path = Directory.GetParent(Assembly.GetExecutingAssembly().Location)!.FullName;
while (true)
{
    bool hasSln = false;
    foreach (string filename in Directory.GetFiles(path))
    {
        if (Path.GetExtension(filename) == ".sln")
        {
            hasSln = true;
            break;
        }
    }
    if (hasSln) { break; }
    path = Directory.GetParent(path)!.FullName;
}
#endif

var driverPath = Path.GetFullPath(Path.Combine(path, "src", "Gel.Net.Driver"));
var docsTemp = Path.GetFullPath(Path.Combine(path, "docs", "tmp"));

try
{
    // build the driver with doc files in release mode
    await Cli.Wrap("dotnet")
        .WithArguments($"build {Path.Combine(driverPath, "Gel.Net.Driver.csproj")} -c Release -f net8.0 --force")
        .WithStandardOutputPipe(PipeTarget.ToStream(Console.OpenStandardOutput()))
        .WithStandardErrorPipe(PipeTarget.ToStream(Console.OpenStandardError()))
        .WithValidation(CommandResultValidation.ZeroExitCode)
        .ExecuteAsync();
}
catch (Exception x)
{
    Console.Error.WriteLine("Build failed");
    Console.Error.WriteLine(x.Message);
    return;
}

try
{
    // copy the doc files to a tmp dir in the docs folder
    var buildXMLPath = Path.Combine(driverPath, "bin", "Release", "net8.0", "Gel.Net.Driver.xml");
    var driverXMLPath = Path.Combine(docsTemp, "driver.xml");

    // file ops don't work in debug
    Directory.CreateDirectory(docsTemp);
    File.Copy(buildXMLPath, driverXMLPath, true);

    var data = Parser.Load(driverXMLPath);

    var generator = new Generator(Path.Combine(path, "docs"), data);
    generator.Generate();
}
catch (Exception)
{
    throw;
}
