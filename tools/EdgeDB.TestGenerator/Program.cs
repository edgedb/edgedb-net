using EdgeDB;
using EdgeDB.Binary;
using EdgeDB.Binary.Codecs;
using EdgeDB.Binary.Packets;
using EdgeDB.ContractResolvers;
using EdgeDB.TestGenerator;
using EdgeDB.TestGenerator.Generators;
using EdgeDB.TestGenerator.Mixin;
using EdgeDB.TestGenerator.ValueProviders;
using EdgeDB.TestGenerator.ValueProviders.Impl;
using Newtonsoft.Json;
using Spectre.Console;
using System.Reflection;
using YamlDotNet.Serialization;

const string TestManifestFile = "test_manifest.yaml";

var client = new EdgeDBClient(new EdgeDBClientPoolConfig
{
    ClientType = EdgeDBClientType.Custom,
    ClientFactory = (i, con, conf) => ValueTask.FromResult((BaseEdgeDBClient)new TestGeneratorClient(con, conf, null!, i)),
    PreferSystemTemporalTypes = true,
    DefaultPoolSize = 100,
    ConnectionTimeout = 30000,
    MessageTimeout = 30000,
    ExplicitObjectIds = true,
    ImplicitTypeIds = false,
});

AnsiConsole.Write(new FigletText("Test Generator").Centered().Color(Color.Blue));

var manifestPath = Path.Combine(Environment.CurrentDirectory, TestManifestFile);

if(!File.Exists(manifestPath))
{
    AnsiConsole.WriteLine("Unable to find test manifest file");
    return;
}

var deserializer = new DeserializerBuilder()
    .Build();

var manifest = deserializer.Deserialize<ValueGenerator.GenerationRuleSet[]>(File.ReadAllText(manifestPath));



var tests = new List<TestGroup>();

var generators = Assembly.GetExecutingAssembly().GetTypes()
    .Where(x => x.IsAssignableTo(typeof(TestGenerator)) && !x.IsAbstract)
    .Select(x => (TestGenerator)Activator.CreateInstance(x)!);

foreach(var generator in generators)
{
    AnsiConsole.Write(new Rule($"{generator.GetType().Name}"));
    var group = await generator.GenerateAsync(client);
    if (!tests.Contains(group))
        tests.Add(group);
}

AnsiConsole.Write(new Rule("Finishing up"));

foreach(var group in tests)
{
    await AnsiConsole.Status()
        .Spinner(Spinner.Known.BouncingBar)
        .StartAsync("Encoding to json...", async ctx =>
        {
            var path = Path.Combine(Environment.CurrentDirectory, "tests");

            Directory.CreateDirectory(path);

            path = Path.Combine(path, group.FileName!);

            if (File.Exists(path))
                File.Delete(path);

            using var fs = File.OpenWrite(path);
            using var writer = new StreamWriter(fs);
            using var jsonWriter = new JsonTextWriter(writer);

            var serializer = new JsonSerializer
            {
                ContractResolver = new EdgeDBContractResolver()
                {
                    NamingStrategy = new Newtonsoft.Json.Serialization.SnakeCaseNamingStrategy()   
                }
            };
            serializer.Serialize(jsonWriter, group);

            ctx.Status("Writing to disc...");
            await jsonWriter.FlushAsync();
        });
}

AnsiConsole.MarkupLine("Test generation [green]complete![/]");