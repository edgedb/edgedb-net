using EdgeDB.Binary;
using EdgeDB.Binary.Codecs;
using EdgeDB.Binary.Packets;
using EdgeDB.TestGenerator.Mixin;
using EdgeDB.TestGenerator.ValueProviders;
using EdgeDB.TestGenerator.ValueProviders.Impl;
using EdgeDB.Utils;
using Spectre.Console;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.TestGenerator.Generators
{
    internal abstract class TestGenerator
    {
        private static ConcurrentDictionary<string, TestGroup> _testGroups = new();

        protected static TestGroup ArgumentTestGroup
            => new() { Name = "V2 Argument Tests", ProtocolVersion = "1.0", FileName = "v2_arguments.json" };

        protected static TestGroup DeepNestingTestGroup
            => new() { Name = "V2 Nested Query Result", ProtocolVersion = "1.0", FileName = "v2_deep_nested.json" };

        protected static TestGroup QueryTestGroup
            => new() { Name = "V2 Query Results", ProtocolVersion = "1.0", FileName = "v2_queryresults.json" };

        protected record QueryDefinition(
            string Name, string Query, Dictionary<string, object?>? Args = null,
            Cardinality Cardinality = Cardinality.Many, Capabilities Capabilities = Capabilities.ReadOnly,
            Func<BaseEdgeDBClient, BaseEdgeDBClient>? ConfigureState = null
        );

        protected abstract TestGroup GetTestGroup();
        protected abstract GenerationRuleSet GetTestSetRules();
        protected abstract string GetTestName(ValueGenerator.GenerationResult result);
        protected abstract IEnumerable<QueryDefinition> GetQueries(ValueGenerator.GenerationResult result);

        protected TestGroup GetOrAddTestGroup(GroupDefinition groupDefinition)
        {
            return _testGroups.GetOrAdd(groupDefinition.Id!, _ => new TestGroup()
            {
                FileName = groupDefinition.Id,
                Name = groupDefinition.Name,
                ProtocolVersion = groupDefinition.Protocol
            });
        }

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

                        using var formatScope = FormatterUtils.Scoped(value);
                        var queries = GetQueries(value);

                        bool isResultSet = false;
                        var testManifest = new Test
                        {
                            Name = GetTestName(value),
                            Queries = new()
                        };

                        QueryDefinition? latestDefinition = null;

                        try
                        {
                            var handle = await client.GetOrCreateClientAsync();

                            foreach (var queryDefinition in queries)
                            {
                                if (queryDefinition.ConfigureState is not null)
                                    handle = queryDefinition.ConfigureState(handle);

                                latestDefinition = queryDefinition;

                                var queryResult = await ExecuteAndGenerateQueryArgs(provider, (TestGeneratorClient)handle, queryDefinition);

                                if (queryResult.Cardinality is not Cardinality.NoResult || queryResult.Result is not null)
                                {
                                    int resultNodeCount = 0;

                                    void update()
                                    {
                                        ctx.Status($"Formatting result ([green]{i}[/]/[red]{f}[/]/[white]{set.Count}[/]) - result nodes [cyan]{resultNodeCount:N0}[/] - test complexity: [yellow]{complexity:N0}[/] - test nodes [yellow]{c}[/] - nodes: [cyan]{totalNodes}[/]");
                                        Interlocked.Increment(ref resultNodeCount);
                                    }

                                    update();

                                    object obj = queryResult.Cardinality is Cardinality.Many && provider.EdgeDBName != "set"
                                        ? await Task.WhenAll(
                                            ((IEnumerable<object>)queryResult.Result!)
                                            .AsParallel()
                                            .Select(x => ValueFormatter.FormatAsync(x!, value.Value, update, provider)))
                                        : await ValueFormatter.FormatAsync(queryResult.Result!, value.Value, update, provider);

                                    if (isResultSet)
                                    {
                                        ((List<object>)testManifest.Result!).Add(obj);
                                    }
                                    else if (testManifest.Result is not null)
                                    {
                                        testManifest.Result = new List<object>() { testManifest.Result, obj };
                                        isResultSet = true;
                                    }
                                    else
                                    {
                                        testManifest.Result = obj;
                                    }
                                }

                                queryResult.Session = handle.Session;

                                testManifest.Queries.Add(queryResult);
                            }

                            i++;

                            group.Tests!.Add(testManifest);

                            await handle.DisposeAsync();
                        }
                        catch(Exception x)
                        {
                            AnsiConsole.WriteException(x);
                            AnsiConsole.WriteLine($"Query type: {value.EdgeDBTypeName}");
                            if(latestDefinition is not null)
                                AnsiConsole.MarkupLine(EdgeQLFormatter.PrettifyAndColor(latestDefinition.Query));

                            Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "fails"));
                            File.WriteAllText(
                                Path.Combine(Environment.CurrentDirectory, "fails", $"fail_{i}{f}.edgeql"),
                                $"{x}{(latestDefinition is not null ? $"\n\n{latestDefinition.Query}\n\n{EdgeQLFormatter.ShittyPrettify(latestDefinition.Query)}" : string.Empty)}\n\n{GenerateTree(provider)}");

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

        private long EstimateComplexity(IValueProvider provider, GenerationRuleSet rules, int depth = 0)
        {
            if (provider is not IWrappingValueProvider wrapping)
                return 1 + depth;

            var rangeAvg = rules.GetRange(wrapping.GetType()).Average();

            var baseComplexity = rangeAvg * wrapping.Children!.Sum(x => EstimateComplexity(x, rules, depth + 1));

            if (wrapping is TupleValueProvider or NamedTupleValueProvider)
            {
                if (depth == 0)
                    baseComplexity *= rangeAvg * wrapping.Children!.Length;
                else if (wrapping.Children!.Any(x => x is SetValueProvider))
                {
                    // multiply all set lengths with eachother, then multiply that into our total, AxBx..
                    var sets = wrapping.Children!
                        .Where(x => x is SetValueProvider)
                        .Cast<IWrappingValueProvider>();

                    var setCount = sets.Count();
                    var averageSetChildren = (long)Math.Ceiling(sets.Average(x => x.Children!.Length));
                    var childrenLength = wrapping.Children!.Length;
                    var cartesianProduct = sets
                        .Select(x => EstimateComplexity(x, rules, depth + 1))
                        .Aggregate((a, b) => a * b);

                    baseComplexity = ((baseComplexity * (setCount * averageSetChildren) * cartesianProduct) / childrenLength) * depth;
                }
            }
            else if (wrapping is SetValueProvider)
            {
                if (wrapping.Children![0] is TupleValueProvider or NamedTupleValueProvider)
                    baseComplexity *= rangeAvg * wrapping.Children!.Length * depth;
            }


            return baseComplexity;
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

        private static async Task<Test.QueryArgs> ExecuteAndGenerateQueryArgs(
            IValueProvider provider,
            TestGeneratorClient handle,
            QueryDefinition queryDefinition)
        {
            try
            {
                var qArgs = new Test.QueryArgs()
                {
                    Name = queryDefinition.Name,
                    Value = queryDefinition.Query,
                    Capabilities = queryDefinition.Capabilities,
                    Cardinality = queryDefinition.Cardinality,
                    Arguments = new List<Test.QueryArgs.QueryArgument>(),
                    Result = queryDefinition.Cardinality switch
                    {
                        Cardinality.One => await handle.QueryRequiredSingleAsync<object>(
                            queryDefinition.Query,
                            queryDefinition.Args,
                            queryDefinition.Capabilities
                        ),
                        Cardinality.Many => await handle.QueryAsync<object>(
                            queryDefinition.Query,
                            queryDefinition.Args,
                            queryDefinition.Capabilities
                        ),
                        Cardinality.AtMostOne => await handle.QuerySingleAsync<object>(
                            queryDefinition.Query,
                            queryDefinition.Args,
                            queryDefinition.Capabilities
                        ),
                        Cardinality.NoResult => await handle.ExecuteAsync(
                            queryDefinition.Query,
                            queryDefinition.Args,
                            queryDefinition.Capabilities
                        ).ContinueWith(r =>
                        {
                            if (r.Exception is not null)
                                throw r.Exception;

                            return (object?)null;
                        }),
                        _ => throw new ArgumentException("Unknown cardinality", queryDefinition.Cardinality.ToString())
                    }
                };

                var inCodec = ResolveInputCodec(handle, queryDefinition);

                if (inCodec is null && queryDefinition.Args is not null && queryDefinition.Args.Any())
                    throw new ArgumentException("Could not find argument codec");

                if (inCodec is not null && queryDefinition.Args is not null && inCodec is ObjectCodec codec)
                {
                    var arr = queryDefinition.Args.ToArray();

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
                            EdgeDBTypeName = CodecNameResolver.GetEdgeDBName(codec.InnerCodecs[i], handle),
                            Name = name,
                            Value = await ValueFormatter.FormatAsync(arr[i].Value!, arr[i].Value!, () => { }, provider)
                        });
                    }
                }

                return qArgs;
            }
            catch
            {
                // reset transaction state
                if (handle.TransactionState is not TransactionState.NotInTransaction)
                    await handle.WithSession(State.Session.Default).ExecuteAsync("rollback", capabilities: Capabilities.Transaction);
                throw;
            }
        }

        private static ICodec? ResolveInputCodec(TestGeneratorClient client, QueryDefinition queryDefinition)
        {
            var key = CodecBuilder.GetCacheHashKey(queryDefinition.Query, queryDefinition.Cardinality switch
            {
                Cardinality.One or Cardinality.AtMostOne => Cardinality.AtMostOne,
                _ => Cardinality.Many
            }, IOFormat.Binary);

            if (CodecBuilder.TryGetCodecs(key, out var inCodecInfo, out _))
                return inCodecInfo.Codec;

            if (client.DataDescription.InputTypeDescriptorId != CodecBuilder.NullCodec && queryDefinition.Args is not null)
            {
                return (ObjectCodec)CodecBuilder.GetCodec(client.DataDescription.InputTypeDescriptorId)!;
            }

            return null;
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
    }
}
