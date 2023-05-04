using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.TestGenerator
{
    internal class TestGroup
    {
        [JsonIgnore]
        public string? FileName { get; set; }
        public string? ProtocolVersion { get; set; }
        public string? Name { get; set; }
        public List<Test>? Tests { get; set; } = new();
    }
}
