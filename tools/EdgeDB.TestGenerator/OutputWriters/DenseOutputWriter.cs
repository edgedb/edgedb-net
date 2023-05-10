using EdgeDB.ContractResolvers;
using Newtonsoft.Json;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.TestGenerator.OutputWriters
{
    internal class DenseOutputWriter : IOutputWriter
    {
        private static readonly JsonSerializer _serializer = new()
        {
            Formatting = Formatting.Indented,
            DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFK",
            ContractResolver = new EdgeDBContractResolver()
            {   
                NamingStrategy = new Newtonsoft.Json.Serialization.SnakeCaseNamingStrategy()
            }
        };

        public async Task WriteAsync(string root, List<TestGroup> tests)
        {
            foreach (var group in tests)
            {
                await AnsiConsole.Status()
                    .Spinner(Spinner.Known.BouncingBar)
                    .StartAsync("Encoding to json...", async ctx =>
                    {
                        var path = Path.Combine(root, $"{group.FileName!}.json");

                        if (File.Exists(path))
                            File.Delete(path);

                        using var fs = File.OpenWrite(path);
                        using var writer = new StreamWriter(fs);
                        using var jsonWriter = new JsonTextWriter(writer);

                        _serializer.Serialize(jsonWriter, group);

                        ctx.Status("Writing to disc...");
                        await jsonWriter.FlushAsync();
                    });
            }
        }
    }
}
