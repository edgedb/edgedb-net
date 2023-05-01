using EdgeDB.Binary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace EdgeDB.TestGenerator
{
    internal class Test
    {
        public string? Name { get; set; }
        public List<QueryArgs> Queries { get; set; } = new();

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
                public object? Value { get; set; }
            }

            [JsonIgnore]
            public object? Result { get; set; }
        }
    }
}
