using EdgeDB.DataTypes;
using EdgeDB.Tests.Integration.SharedTests.Json;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Integration.SharedTests
{
    [TestClass]
    public class SharedTestsRunner
    {
        public static readonly string TestsDirectory = Path.Combine(Environment.CurrentDirectory, "tests");

        private static T Read<T>(string path)
        {
            using var fs = File.OpenRead(path);
            using var sr = new StreamReader(fs);
            using var r = new JsonTextReader(sr);
            return EdgeDBConfig.JsonSerializer.Deserialize<T>(r)!;
        }

        [TestMethod]
        public async Task RunAsync()
        {
            var client = new EdgeDBClient(new EdgeDBClientPoolConfig
            {
                PreferSystemTemporalTypes = true
            });

            var groups = Directory.GetFiles(TestsDirectory, "*.json");
            foreach (var file in groups)
            {
                var testgroup = Read<TestGroup>(file);

                var testDir = Path.Combine(TestsDirectory, Path.GetFileNameWithoutExtension(file));

                foreach(var testFile in Directory.GetFiles(testDir, "*.json"))
                {
                    var testDefinition = Read<Test>(testFile);

                    List<ExecutionResult> results = new();

                    // can return 1 or more types, for example to test complex
                    // codecs by returning all possible combinations of valid
                    // types for the query.
                    var resultTypes = ResultTypeBuilder.CreateResultTypes(testDefinition.Result!).Prepend(typeof(object));

                    foreach(var query in testDefinition.Queries!)
                    {
                        results.Add(await ExecuteQueryAsync(client, query, resultTypes));
                    }

                    foreach(var result in results)
                    {
                        foreach(var resultType in result.ResultMap)
                        {
                            ResultAsserter.AssertResult(testDefinition.Result, resultType.Value);
                        }
                    }
                }
            }
        }

        private readonly struct ExecutionResult
        {
            public readonly Dictionary<Type, object?> ResultMap;

            public ExecutionResult(Dictionary<Type, object?> map)
            {
                ResultMap = map;
            }
        }
        private async Task<ExecutionResult> ExecuteQueryAsync(EdgeDBClient client, Test.QueryArgs query, IEnumerable<Type> resultTypes)
        {
            Dictionary<string, object?>? args = null;

            if (query.Arguments is not null && query.Arguments.Any())
            {
                args = new Dictionary<string, object?>();

                foreach (var argument in query.Arguments)
                {
                    args.Add(argument.Name!, ToObject(argument.Value!));
                }
            }

            var map = new Dictionary<Type, object?>();

            foreach(var resultType in resultTypes)
            {
                var queryMethod = GetQueryMethod(resultType, query.Cardinality);

                var task = (Task)queryMethod.Invoke(client, new object?[] { query.Value, args, query.Capabilities, CancellationToken.None })!;

                await task;

                if(TryGetResult(task, out var taskResult))
                {
                    map.Add(resultType, taskResult);
                }
            }

            return new ExecutionResult(map);
        }

        
        private bool TryGetResult(Task task, out object? result)
        {
            if (!task.GetType().IsGenericType)
                return (result = null) is null;

            result = task.GetType()
                .GetProperty(nameof(Task<object>.Result), BindingFlags.Public | BindingFlags.Instance)!
                .GetValue(task);

            return true;
        }

        private MethodInfo GetQueryMethod(Type result, Cardinality cardinality)
        {
            if (cardinality is Cardinality.NoResult)
                return typeof(EdgeDBClient).GetMethod("ExecuteAsync", BindingFlags.Public | BindingFlags.Instance)!;

            return typeof(EdgeDBClient).GetMethod(
                cardinality switch
                {
                    Cardinality.One => nameof(EdgeDBClient.QueryRequiredSingleAsync),
                    Cardinality.AtMostOne => nameof(EdgeDBClient.QuerySingleAsync),
                    Cardinality.Many => nameof(EdgeDBClient.QueryAsync),
                    _ => throw new InvalidOperationException($"Unsupported cardinality {cardinality}")
                }, BindingFlags.Public | BindingFlags.Instance
           )!.MakeGenericMethod(result);
        }

        private object ToObject(IResultNode node)
        {
            switch (node)
            {
                case CollectionResultNode collection:
                    var values = (object[])collection.Value!;

                    if (values.Length == 0)
                        return Array.Empty<object>();

                    var type = ResultTypeBuilder.TryGetScalarType(((IResultNode)values[0]).Type, out var t)
                        ? t
                        : ToObject((IResultNode)values[0])!.GetType();

                    var arr = Array.CreateInstance(type, values.Length);

                    for (int i = 0; i != values.Length; i++)
                        arr.SetValue(ToObject((IResultNode)values[i]), i);

                    return arr;
                case ResultNode:
                    switch (node.Type)
                    {
                        case "tuple":
                            {
                                var children = ((IResultNode?[])node.Value!).Select(x => ToObject(x!)).ToArray();
                                return new TransientTuple(children).ToValueTuple();
                            }
                        default:
                            return node.Value!;
                    }
                default:
                    throw new NotSupportedException($"Unsupported node {node.GetType().Name}");
            }
        }

        internal class TestGroup
        {
            public string? ProtocolVersion { get; set; }
            public string? Name { get; set; }
            public IEnumerable<Test>? Tests { get; set; }
        }

        internal class Test
        {
            public string? Name { get; set; }
            public IEnumerable<QueryArgs>? Queries { get; set; }

            [Newtonsoft.Json.JsonConverter(typeof(ResultNodeConverter))]
            public object? Result { get; set; }

            public class QueryArgs
            {
                public string? Name { get; set; }
                public Cardinality Cardinality { get; set; }
                public string? Value { get; set; }
                public List<QueryArgument>? Arguments { get; set; }
                public EdgeDB.Capabilities Capabilities { get; set; }

                public class QueryArgument
                {
                    public string? Name { get; set; }
                    [JsonProperty("edgedb_typename")]
                    public string? EdgeDBTypeName { get; set; }
                    [Newtonsoft.Json.JsonConverter(typeof(ResultNodeConverter))]
                    public IResultNode? Value { get; set; }
                }
            }
        }
    }
}

