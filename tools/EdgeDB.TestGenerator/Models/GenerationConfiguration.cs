using Spectre.Console;
using System;
using System.Diagnostics.CodeAnalysis;
using YamlDotNet.Serialization;

namespace EdgeDB.TestGenerator
{
    public class GenerationConfiguration
    {
        [YamlMember(Alias = "groups")]
        public List<GroupDefinition> Groups { get; set; } = new();

        [YamlMember(Alias = "rulesets")]
        public List<RuleSetConfiguration> RuleSets { get; set; } = new();

        public bool TryGetGroup(RuleSetConfiguration ruleset, [MaybeNullWhen(false)] out GroupDefinition group)
        {
            group = Groups.FirstOrDefault(x => x.Id == ruleset.GroupId);
            return group is not null;
        }
    }
}

