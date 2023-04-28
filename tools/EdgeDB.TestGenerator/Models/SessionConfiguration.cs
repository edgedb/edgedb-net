using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace EdgeDB.TestGenerator
{
    public class SessionConfiguration
    {
        [YamlMember(Alias = "module")]
        public string? Module { get; set; }

        [YamlMember(Alias = "aliases")]
        public Dictionary<string, string>? Aliases { get; set; }

        [YamlMember(Alias = "globals")]
        public Dictionary<string, string>? Globals { get; set; }

        [YamlMember(Alias = "config")]
        public SessionConfigConfiguration? Config { get; set; }
    }
}
