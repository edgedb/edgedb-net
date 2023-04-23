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
        protected static TestGroup ArgumentTestGroup
            => new() { Name = "V2 Argument Tests", ProtocolVersion = "1.0", FileName = "v2_arguments.json" };

        protected static TestGroup DeepNestingTestGroup
            => new() { Name = "V2 Nested Query Result", ProtocolVersion = "1.0", FileName = "v2_deep_nested.json" };

        protected static TestGroup QueryTestGroup
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
                .Spinner(Spinner.Known.BouncingBar)
                .StartAsync("Generating providers...", async ctx =>
                {
                    AnsiConsole.MarkupLine(rules.ToString());

                    int i = 0;
                    int f = 0;

                    List<IValueProvider> set;
                    try
                    {
                        set = ValueGenerator.GenerateCompleteSet(rules);
                    }
                    catch(Exception x)
                    {
                        f++;
                        AnsiConsole.MarkupLine("Failed to generate provider set based off of the given rules");
                        AnsiConsole.WriteException(x);
                        return;
                    }

                    int totalNodes = set.Count;

                    AnsiConsole.MarkupLine($"Generated [green]{set.Count}[/] root nodes");


                    foreach (var provider in set)
                    {
                        ctx.Status($"Generating value ([green]{i}[/]/[red]{f}[/]/[white]{set.Count}[/]) - nodes: [cyan]{totalNodes}[/]");

                        ValueGenerator.GenerationResult value;

                        try
                        {
                            value = provider.AsResult(rules);
                        }
                        catch(Exception x)
                        {
                            f++;
                            AnsiConsole.WriteException(x);
                            continue;
                        }

                        var c = CountSetNodes(value.Provider);
                        var complexity = EstimateComplexity(value.Provider, rules);

                        totalNodes += c;

                        ctx.Status($"Generating tests ([green]{i}[/]/[red]{f}[/]/[white]{set.Count}[/]) - test complexity: [yellow]{complexity:N0}[/] - test nodes [yellow]{c}[/] - nodes: [cyan]{totalNodes}[/]");

                        var def = GetQuery(value);

                        try
                        {
                            var test = await GenerateTestManifestAsync(client, def.Query, def.Cardinality, def.Args, def.Capabilities);

                            int resultNodeCount = 0;

                            Action update = () =>
                            {
                                ctx.Status($"Formatting result ([green]{i}[/]/[red]{f}[/]/[white]{set.Count}[/]) - result nodes [cyan]{resultNodeCount:N0}[/] - test complexity: [yellow]{complexity:N0}[/] - test nodes [yellow]{c}[/] - nodes: [cyan]{totalNodes}[/]");
                                Interlocked.Increment(ref resultNodeCount);
                            };

                            update();

                            test.Result = def.Cardinality is Cardinality.Many && provider.EdgeDBName != "set"
                                ? await Task.WhenAll(((IEnumerable<object>)test.Result!).AsParallel().Select(x => ValueFormatter.FormatAsync(x!, value.Value, update, provider)))
                                : await ValueFormatter.FormatAsync(test.Result!, value.Value, update, provider);

                            test.Name = GetTestName(value);

                            if(!group.Tests.Any(x => x.Name == test.Name))
                                group.Tests.Add(test);

                            i++;
                        }
                        catch (Exception x)
                        {
                            AnsiConsole.WriteException(x);
                            AnsiConsole.WriteLine($"Query type: {value.EdgeDBTypeName}");
                            AnsiConsole.MarkupLine(EdgeQLFormatter.PrettifyAndColor(def.Query));

                            Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "fails"));
                            File.WriteAllText(
                                Path.Combine(Environment.CurrentDirectory, "fails", $"fail_{i}{f}.edgeql"),
                                $"{x}\n\n{def.Query}\n\n{EdgeQLFormatter.ShittyPrettify(def.Query)}\n\n{GenerateTree(provider)}");

                            f++;
                        }
                    }

                    AnsiConsole.MarkupLine($"Generated [green]{set.Count}[/] tests, [red]{f}[/] failed. Discovered {totalNodes} different value nodes");
                });

            return group;
        }

        private int CountSetNodes(IValueProvider provider)
        {
            return 1 + (provider is IWrappingValueProvider wrapping ? wrapping.Children!.Sum(x => CountSetNodes(x)) : 0);
        }

        private long EstimateComplexity(IValueProvider provider, ValueGenerator.GenerationRuleSet rules)
        {
            var nodeCount = CountSetNodes(provider);

            return nodeCount + FlattenProviders(provider)
                .Select(x => x is IWrappingValueProvider wrapper
                    ? rules.GetRange(wrapper.GetType()).End.Value * wrapper.Children!.Select(x => EstimateComplexity(x, rules)).Sum()
                    : 1
                ).Sum();
        }

        private static string GenerateTree(IValueProvider provider)
        {
            var tree = new Tree(provider.EdgeDBName);

            void addNode(TreeNode parent, IValueProvider node)
            {
                var treeNode = parent.AddNode(node.EdgeDBName);

                if (node is IWrappingValueProvider wrapping)
                {
                    foreach (var child in wrapping.Children!)
                    {
                        addNode(treeNode, child);
                    }
                }
            };

            if(provider is IWrappingValueProvider wrapping)
            {
                foreach(var child in wrapping.Children!)
                {
                    var node = tree.AddNode(child.EdgeDBName);

                    if(child is IWrappingValueProvider childWrapping)
                    {
                        foreach(var child2 in childWrapping.Children!)
                        {
                            addNode(node, child2);
                        }
                    }
                }
            }

            return string.Join("\n", tree.GetSegments(AnsiConsole.Console).Select(x => x.Text));
        }

        private IEnumerable<IValueProvider> FlattenProviders(IValueProvider provider)
        {
            if(provider is IWrappingValueProvider wrapping)
            {
                foreach(var child in wrapping.Children!)
                {
                    foreach (var subProvider in FlattenProviders(child))
                        yield return subProvider;
                }
            }

            yield return provider;
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
