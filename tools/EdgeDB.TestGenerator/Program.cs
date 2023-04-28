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
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

const string TestManifestFile = "test_manifest.yaml";

var client = new EdgeDBClient(new EdgeDBClientPoolConfig
{
    ClientType = EdgeDBClientType.Custom,
    ClientFactory = (i, con, conf) => ValueTask.FromResult((BaseEdgeDBClient)new TestGeneratorClient(con, conf, null!, i)),
    PreferSystemTemporalTypes = true,
    DefaultPoolSize = 100,
    ConnectionTimeout = 30000,
    MessageTimeout = 600000,
    ExplicitObjectIds = true,
    ImplicitTypeIds = false,
});

AnsiConsole.Write(new FigletText("Test Generator").Centered().Color(Color.Blue));

var generators = Assembly.GetExecutingAssembly().GetTypes()
    .Where(x => x.IsAssignableTo(typeof(TestGenerator)) && !x.IsAbstract && x != typeof(ConfigurableTestGenerator))
    .Select(x => (TestGenerator)Activator.CreateInstance(x)!)
    .ToList();

#if DEBUG
var manifestPath = Path.Combine("C:\\Users\\lynch\\source\\repos\\EdgeDB\\tools\\EdgeDB.TestGenerator", TestManifestFile);
#else
var manifestPath = Path.Combine(Environment.CurrentDirectory, TestManifestFile);
#endif

if(!File.Exists(manifestPath))
{
    AnsiConsole.WriteLine("Unable to find test manifest file");
    return;
}

var deserializer = new DeserializerBuilder()
    .WithNamingConvention(UnderscoredNamingConvention.Instance)
    .WithNodeDeserializer(new RangeDeserializer())
    .Build();

var content = File.ReadAllText(manifestPath);

var configuration = deserializer.Deserialize<GenerationConfiguration>(content);

foreach(var ruleset in configuration.RuleSets)
{
    if (!configuration.TryGetGroup(ruleset, out var group))
        throw new ConfigurationException($"No group found with the id '{ruleset.GroupId}'");

    var generator = new ConfigurableTestGenerator(group, ruleset);

    generators.Add(generator);
}

var tests = new List<TestGroup>();

foreach(var generator in generators)
{
    AnsiConsole.Write(new Rule($"{generator}"));
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

            path = Path.Combine(path, $"{group.FileName!}.json");

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

// range deserializer for YAML
class RangeDeserializer : INodeDeserializer
{
    public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
    {
        value = null;

        if (expectedType != typeof(Range))
            return false;

        var scalar = reader.Consume<YamlDotNet.Core.Events.Scalar>();

        var spl = scalar.Value.Split("..");

        value = new Range(int.Parse(spl[0]), int.Parse(spl[1]));

        return true;
    }
}
