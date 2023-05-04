using EdgeDB.ContractResolvers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.TestGenerator.OutputWriters
{
    internal class SpreadOutputWriter : IOutputWriter
    {
        private static readonly JsonSerializer _serializer = new()
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
            ContractResolver = new EdgeDBContractResolver()
            {
                NamingStrategy = new Newtonsoft.Json.Serialization.SnakeCaseNamingStrategy()
            }
        };

        public async Task WriteAsync(string root, List<TestGroup> tests)
        {
            foreach(var group in tests)
            {
                await AnsiConsole.Status()
                    .Spinner(Spinner.Known.BouncingBar)
                    .StartAsync("Encoding to json...", async ctx =>
                    {
                        var groupTests = group.Tests!;

                        group.Tests = null;

                        var path = Path.Combine(root, $"{group.FileName!}.json");

                        if (File.Exists(path))
                            File.Delete(path);

                        using (var fs = File.OpenWrite(path))
                        using (var writer = new StreamWriter(fs))
                        using (var jsonWriter = new JsonTextWriter(writer))
                        {
                            _serializer.Serialize(jsonWriter, group);
                            await jsonWriter.FlushAsync();
                        }

                        Directory.CreateDirectory(Path.Combine(root, group.FileName!));

                        for (int i = 0; i != groupTests.Count; i++)
                        {
                            ctx.Status($"Encoding test {i + 1}/{groupTests.Count}...");
                            using var fs = File.OpenWrite(Path.Combine(root, group.FileName!, $"{i}.json"));
                            using var writer = new StreamWriter(fs);
                            using var jsonWriter = new JsonTextWriter(writer);
                            _serializer.Serialize(jsonWriter, groupTests[i]);
                            await jsonWriter.FlushAsync();
                        }

                    });
            }
        }
    }
}
