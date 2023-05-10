using EdgeDB.ContractResolvers;
using EdgeDB.DataTypes;
using EdgeDB.State;
using EdgeDB.Tests.Integration.SharedTests.Json;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using BinaryResult = EdgeDB.EdgeDBBinaryClient.RawExecuteResult;

namespace EdgeDB.Tests.Integration.SharedTests
{
    [TestClass]
    public class SharedTestsRunner
    {
        public static readonly string TestsDirectory = Path.Combine(Environment.CurrentDirectory, "tests");
        private static readonly JsonSerializer Serializer = new()
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            DateParseHandling = DateParseHandling.DateTimeOffset,
            ContractResolver = new EdgeDBContractResolver(),
            FloatParseHandling = FloatParseHandling.Decimal,
        };
        private static readonly EdgeDBClient _client = new(new EdgeDBClientPoolConfig
        {
            PreferSystemTemporalTypes = true,
            ExplicitObjectIds = true,
            MessageTimeout = (uint)TimeSpan.FromHours(1).TotalMilliseconds
        });

        private static T Read<T>(string path)
        {
            Console.WriteLine($"Loading {path}...");
            using var fs = File.OpenRead(path);
            using var sr = new StreamReader(fs);
            using var r = new JsonTextReader(sr);
            return Serializer.Deserialize<T>(r)!;
        }

        private class ExecutionResult
        {
            public required Cardinality Cardinality { get; init; }
            public required BinaryResult Result { get; init; }
        }

        public static async Task RunAsync(string path)
        {
            var testDefinition = Read<Test>(path);

            List<ExecutionResult> results = new();

            // can return 1 or more types, for example to test complex
            // codecs by returning all possible combinations of valid
            // types for the query.
            var resultTypes = ResultTypeBuilder.CreateResultTypes(testDefinition.Result!).Prepend(typeof(object)).ToArray();

            Console.WriteLine($"Created {resultTypes.Count()} different results for '{testDefinition.Name}'");

            var clientHandle = await _client.GetOrCreateClientAsync<EdgeDBBinaryClient>();

            for (int i = 0; i != testDefinition.Queries!.Count; i++)
            {
                var query = testDefinition.Queries[i];

                Console.WriteLine($"{i + 1}/{testDefinition.Queries.Count}: {query.Name}");

                // state
                clientHandle.WithSession(query.Session!);

                var result = await ExecuteQueryAsync(
                    clientHandle,
                    query,
                    resultTypes
                );

                if(result.Data.Length > 0)
                    results.Add(new() { Cardinality = query.Cardinality, Result = result });
            }

            foreach (var executionResult in results)
            {
                foreach (var resultType in resultTypes)
                {
                    var value = BuildResult(clientHandle, resultType, executionResult.Cardinality, executionResult.Result);
                    ResultAsserter.AssertResult(testDefinition.Result, value);
                }
            }

            await clientHandle.DisposeAsync();
        }

        private static object? BuildResult(EdgeDBBinaryClient client, Type type, Cardinality card, BinaryResult result)
        {
            switch (card)
            {
                case Cardinality.Many:
                    {
                        var array = Array.CreateInstance(type, result.Data.Length);

                        for (int i = 0; i != result.Data.Length; i++)
                        {
                            var obj = ObjectBuilder.BuildResult(type, client, result.Deserializer, ref result.Data[i]);
                            array.SetValue(obj, i);
                        }
                        return array;
                    }
                case Cardinality.AtMostOne:
                    {
                        if (result.Data.Length == 0)
                            return null;

                        return ObjectBuilder.BuildResult(type, client, result.Deserializer, ref result.Data[0]);
                    }
                case Cardinality.One:
                    {
                        if (result.Data.Length != 1)
                            throw new ArgumentOutOfRangeException(nameof(result), "Missing data for result");

                        return ObjectBuilder.BuildResult(type, client, result.Deserializer, ref result.Data[0]);
                    }
                default:
                    if (result.Data.Length > 0)
                        throw new ArgumentException("Unknown cardinality path for remaining data", nameof(card));

                    return null;
            }
        }

        private static Task<BinaryResult> ExecuteQueryAsync(EdgeDBBinaryClient client, Test.QueryArgs query, IEnumerable<Type> resultTypes)
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

            return QueryRawAsync(client, query.Value!, args, query.Capabilities, query.Cardinality);
        }

        private static Task<BinaryResult> QueryRawAsync(
            EdgeDBBinaryClient client,
            string query,
            IDictionary<string, object?>? args,
            Capabilities capabilities, Cardinality cardinality)
        {
            return client.ExecuteInternalAsync(
                query,
                args,
                cardinality switch
                {
                    Cardinality.One or Cardinality.AtMostOne => Cardinality.AtMostOne,
                    _ => Cardinality.Many
                },
                capabilities,
                cardinality is Cardinality.NoResult ? IOFormat.None : IOFormat.Binary
            );
        }

        private static bool TryGetResult(Task task, out object? result)
        {
            if (!task.GetType().IsGenericType)
                return (result = null) is null;

            result = task.GetType()
                .GetProperty(nameof(Task<object>.Result), BindingFlags.Public | BindingFlags.Instance)!
                .GetValue(task);

            return true;
        }

        private static MethodInfo GetQueryMethod(Type result, Cardinality cardinality)
        {
            if (cardinality is Cardinality.NoResult)
                return typeof(BaseEdgeDBClient).GetMethod("ExecuteAsync", BindingFlags.Public | BindingFlags.Instance)!;

            return typeof(BaseEdgeDBClient).GetMethod(
                cardinality switch
                {
                    Cardinality.One => nameof(BaseEdgeDBClient.QueryRequiredSingleAsync),
                    Cardinality.AtMostOne => nameof(BaseEdgeDBClient.QuerySingleAsync),
                    Cardinality.Many => nameof(BaseEdgeDBClient.QueryAsync),
                    _ => throw new InvalidOperationException($"Unsupported cardinality {cardinality}")
                }, BindingFlags.Public | BindingFlags.Instance
           )!.MakeGenericMethod(result);
        }

        private static object ToObject(IResultNode node)
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
            public List<QueryArgs>? Queries { get; set; }

            [Newtonsoft.Json.JsonConverter(typeof(ResultNodeConverter))]
            public object? Result { get; set; }

            public class QueryArgs
            {
                public string? Name { get; set; }
                public Cardinality Cardinality { get; set; }
                public string? Value { get; set; }
                public List<QueryArgument>? Arguments { get; set; }
                public EdgeDB.Capabilities Capabilities { get; set; }
                public Session? Session { get; set; }

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

