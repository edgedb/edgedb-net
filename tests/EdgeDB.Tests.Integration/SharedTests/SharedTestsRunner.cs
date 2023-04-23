using EdgeDB.DataTypes;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Integration.SharedTests
{
    [TestClass]
    public class SharedTestsRunner
    {
        public static readonly string TestsDirectory = Path.Combine(Environment.CurrentDirectory, "tests");

        internal static List<TestGroup> Tests = new List<TestGroup>();

        static SharedTestsRunner()
        {
            foreach (var file in Directory.GetFiles(TestsDirectory, "*.json"))
            {
                using var fs = File.OpenRead(file);
                using var sr = new StreamReader(fs);
                using var r = new JsonTextReader(sr);
                var jobj = JObject.Load(r);

                var group = new TestGroup
                {
                    Name = jobj[nameof(TestGroup.Name)].As<string>(),
                    ProtocolVersion = jobj[nameof(TestGroup.ProtocolVersion)].As<string>(),
                    Tests = new List<Test>()
                };

                foreach(var testObj in jobj[nameof(TestGroup.Tests)]!)
                {
                    var test = EdgeDBConfig.JsonSerializer.Deserialize<Test>(testObj.CreateReader())!;
                    test.JsonResult = testObj["Result"]!.ToString();

                    group.Tests.Add(test);
                }

                Tests.Add(group);
            }      
        }

        [TestMethod]
        public async Task RunAsync()
        {
            var client = new EdgeDBClient(new EdgeDBClientPoolConfig
            {
                PreferSystemTemporalTypes = true
            });

            foreach(var testGroup in Tests)
            {
                foreach(var test in testGroup.Tests)
                {
                    // double args get added?
                    var result = await ExecuteTestQueryAsync(client, test);

                    result.Binary.Should().BeEquivalentTo(test.Result);
                    result.Json.Value.Should().BeEquivalentTo(test.JsonResult);
                }
            }
        }

        private static async Task<(object? Binary, Json Json)> ExecuteTestQueryAsync(EdgeDBClient client, Test test)
        {
            var args = new Dictionary<string, object?>();

            if(test.Query!.Arguments != null)
            {
                foreach (var arg in test.Query.Arguments)
                {
                    args.Add(arg.Name!, arg.Value);
                }
            }


            var binary = test.Query.Cardinality switch
            {
                Cardinality.One => await client.QueryRequiredSingleAsync<object>(test.Query.Value!, args, test.Query.Capabilities),
                Cardinality.AtMostOne => await client.QuerySingleAsync<object>(test.Query.Value!, args, test.Query.Capabilities),
                Cardinality.Many => await client.QueryAsync<object>(test.Query.Value!, args, test.Query.Capabilities),
                _ => throw new NotImplementedException()
            };

            var json = await client.QueryJsonAsync(test.Query.Value!, args, test.Query.Capabilities);

            return (binary, json);
        }

        internal class TestGroup
        {
            public string? ProtocolVersion { get; set; }
            public string? Name { get; set; }
            public List<Test> Tests { get; set; } = new();
        }

        internal class Test
        {
            public string? Name { get; set; }
            public QueryArgs? Query { get; set; }

            public Capabilities ActualCapabilities { get; set; }
            public Cardinality ActualCardinality { get; set; }

            public object? Result { get; set; }
            public string? JsonResult { get; set; }

            public class QueryArgs
            {
                public Cardinality Cardinality { get; set; }
                public string? Value { get; set; }
                public List<QueryArgument>? Arguments { get; set; }
                public EdgeDB.Capabilities Capabilities { get; set; }

                public class QueryArgument
                {
                    public string? Name { get; set; }
                    public string? EdgeDBTypeName { get; set; }
                    public Guid? Id { get; set; }
                    public object? Value { get; set; }
                }
            }
        }
    }
}

