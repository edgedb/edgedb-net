using System;
using YamlDotNet.Serialization;

namespace EdgeDB.TestGenerator
{
    public class GroupDefinition
    {
        [YamlMember(Alias = "name")]
        public string? Name { get; set; }

        [YamlMember(Alias = "id")]
        public string? Id { get; set; }

        [YamlMember(Alias = "protocol")]
        public string? Protocol { get; set; }
    }
}

