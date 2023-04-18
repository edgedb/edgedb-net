using EdgeDB.Binary;
using EdgeDB.Binary.Codecs;
using EdgeDB.Binary.Packets;
using EdgeDB.TestGenerator.Mixin;
using EdgeDB.TestGenerator.ValueProviders;
using EdgeDB.Utils;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.TestGenerator.Generators
{
    internal abstract class TestGenerator
    {
        protected TestGroup ArgumentTestGroup
            => new() { Name = "V2 Argument Tests", ProtocolVersion = "1.0", FileName = "v2_arguments.json" };

        protected TestGroup QueryTestGroup
            => new() { Name = "V2 Query Results", ProtocolVersion = "1.0", FileName = "v2_queryresults.json" };

        protected record QueryDefinition(
            string Query, Dictionary<string, object?>? Args = null,
            Cardinality Cardinality = Cardinality.Many, Capabilities Capabilities = Capabilities.ReadOnly
        );

        protected abstract TestGroup GetTestGroup();
        protected abstract ValueGenerator.GenerationRuleSet GetTestSetRules();
        protected abstract string GetTestName(ValueGenerator.GenerationResult result);

        protected abstract QueryDefinition GetQuery(ValueGenerator.GenerationResult result);

        public async Task<TestGroup> GenerateAsync(EdgeDBClient client)
        {
            var group = GetTestGroup();
            var rules = GetTestSetRules();

            await AnsiConsole.Status()
                .StartAsync("Generating providers...", async ctx =>
                {
                    AnsiConsole.MarkupLine(rules.ToString());

                    var set = ValueGenerator.GenerateCompleteSet(rules);
                    int totalNodes = set.Count;

                    AnsiConsole.MarkupLine($"Generated [green]{set.Count}[/] root nodes");

                    int i = 0;

                    foreach (var provider in set)
                    {
                        ctx.Status($"Generating value ({i}/{set.Count}) - nodes: {totalNodes}");

                        var value = provider.AsResult(rules);
                        totalNodes += CountSetNodes(value.Provider);

                        ctx.Status($"Generating tests ({i}/{set.Count}) - nodes: {totalNodes}");

                        var def = GetQuery(value);

                        try
                        {
                            var test = await GenerateTestManifestAsync(client, def.Query, def.Cardinality, def.Args, def.Capabilities);

                            test.Name = GetTestName(value);

                            group.Tests.Add(test);
                        }
                        catch (Exception x)
                        {
                            AnsiConsole.WriteException(x);
                            if(x is EdgeDBErrorException err && err.Query is not null)
                            {
                                AnsiConsole.Markup($"Query:\n{EdgeQLFormatter.PrettifyAndColor(err.Query)}");
                            }
                        }

                        i++;
                    }

                    AnsiConsole.MarkupLine($"Generated [green]{set.Count}[/] tests. Discovered {totalNodes} different value nodes");
                });

            return group;
        }

        private int CountSetNodes(IValueProvider provider)
        {
            return 1 + (provider is IWrappingValueProvider wrapping ? wrapping.Children!.Sum(x => CountSetNodes(x)) : 0);
        }

        private async Task<Test> GenerateTestManifestAsync(
            EdgeDBClient client,
            string query,
            Cardinality exp,
            Dictionary<string, object?>? args = null,
            Capabilities capabilities = Capabilities.ReadOnly)
        {
            await using var handle = await client.GetOrCreateClientAsync<TestGeneratorClient>();

            //handle.ClearCaches();

            var result = exp switch
            {
                Cardinality.One => await handle.QueryRequiredSingleAsync<object>(query, args, capabilities),
                Cardinality.Many => await handle.QueryAsync<object>(query, args, capabilities),
                Cardinality.AtMostOne => await handle.QuerySingleAsync<object>(query, args, capabilities),
                _ => throw new ArgumentException("Unknown cardinality", exp.ToString())
            };

            return new Test
            {
                Query = BuildArgs(handle, query, exp, args, capabilities, handle.DataDescription),
                Descriptors = GetDescriptors(handle.DataDescription),
                ActualCapabilities = handle.DataDescription.Capabilities,
                ActualCardinality = handle.DataDescription.Cardinality,
                Result = result,
                BinaryResult = handle.Data.Select(x => HexConverter.ToHex(x.PayloadBuffer)).ToList()
            };
        }

        private List<ITypeDescriptor> GetDescriptors(CommandDataDescription d)
        {
            var descriptors = new List<ITypeDescriptor>();

            var reader = new PacketReader(d.OutputTypeDescriptorBuffer);

            while (!reader.Empty)
            {
                descriptors.Add(ITypeDescriptor.GetDescriptor(ref reader));
            }

            return descriptors;
        }

        private static Test.QueryArgs BuildArgs(
            EdgeDBBinaryClient client,
            string query,
            Cardinality cardinality,
            Dictionary<string, object?>? args,
            Capabilities capabilities,
            CommandDataDescription dataDescriptor)
        {
            var qArgs = new Test.QueryArgs()
            {
                Value = query,
                Capabilities = capabilities,
                Cardinality = cardinality,
                Arguments = new List<Test.QueryArgs.QueryArgument>()
            };

            if (dataDescriptor.InputTypeDescriptorId != CodecBuilder.NullCodec && args is not null)
            {
                var codec = (ObjectCodec)CodecBuilder.GetCodec(dataDescriptor.InputTypeDescriptorId)!;

                var arr = args.ToArray();

                for (int i = 0; i != arr.Length; i++)
                {
                    var name = codec.PropertyNames[i];
                    var cid = CodecBuilder.CodecCache.FirstOrDefault(x => x.Value == codec.InnerCodecs[i]).Key;

                    if (cid == Guid.Empty)
                    {
                        cid = CodecBuilder.InvalidCodec;
                    }

                    qArgs.Arguments.Add(new Test.QueryArgs.QueryArgument
                    {
                        EdgeDBTypeName = CodecNameResolver.GetEdgeDBName(codec.InnerCodecs[i], client),
                        Id = cid,
                        Name = name,
                        Value = arr[i].Value
                    });
                }
            }

            return qArgs;
        }
    }
}
