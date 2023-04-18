using EdgeDB.Binary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.TestGenerator
{
    internal class Test
    {
        public string? Name { get; set; }
        public QueryArgs? Query { get; set; }

        public Capabilities ActualCapabilities { get; set; }
        public Cardinality ActualCardinality { get; set; }

        [JsonIgnore]
        public List<ITypeDescriptor>? Descriptors { get; set; }

        public object? Result { get; set; }

        [JsonIgnore]
        public List<string>? BinaryResult { get; set; }

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
