using EdgeDB.State;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace EdgeDB.TestGenerator
{
    public partial class QueryTemplate
    {
        [YamlMember(Alias = "name")]
        public string? Name { get; set; }

        [YamlMember(Alias = "query")]
        public string? Query { get; set; }

        [YamlMember(Alias = "arguments")]
        public Dictionary<string, string>? Arguments { get; set; }

        [YamlMember(Alias = "capabilities")]
        public Capabilities Capabilities { get; set; } = Capabilities.ReadOnly;

        [YamlMember(Alias = "cardinality")]
        public Cardinality Cardinality { get; set; } = Cardinality.Many;

        [YamlMember(Alias = "session")]
        public SessionConfiguration? Session { get; set; }

        public string? FormatName(GenerationRuleSet rules, VariableFormatter formatProvider)
            => FormatterUtils.Format(rules, Name, formatProvider, formatProvider.Target);

        public string? FormatQuery(GenerationRuleSet rules, VariableFormatter formatProvider)
            => FormatterUtils.Format(rules, Query!, formatProvider, formatProvider.Target);

        public Dictionary<string, object?>? FormatArguments(GenerationRuleSet rules, VariableFormatter formatProvider)
            => FormatterUtils.FormatKVP(rules, formatProvider, Arguments, formatProvider.Target);
    }
}

