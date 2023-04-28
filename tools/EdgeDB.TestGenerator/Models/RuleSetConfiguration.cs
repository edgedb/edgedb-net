using Spectre.Console;
using System;
using YamlDotNet.Serialization;

namespace EdgeDB.TestGenerator
{
    public sealed class RuleSetConfiguration
    {
        [YamlMember(Alias = "group_id")]
        public string? GroupId { get; set; }

        [YamlMember(Alias = "ranges")]
        public Dictionary<string, Range>? YamlRanges { get; set; }

        [YamlMember(Alias = "root_excluded")]
        public List<string>? YamlRootExcluded { get; set; }

        [YamlMember(Alias = "excluded")]
        public List<string>? YamlExcluded { get; set; }

        [YamlMember(Alias = "nested_excluded")]
        public Dictionary<string, List<string>>? YamlNestedExcluded { get; set; }

        [YamlMember(Alias = "test_name_template")]
        public string? TestNameTemplate { get; set; }

        [YamlMember(Alias = "name")]
        public string? Name { get; set; }

        [YamlMember(Alias = "default_range")]
        public Range DefaultRange { get; set; } = 1..10;

        [YamlMember(Alias = "max_depth")]
        public int MaxDepth { get; set; } = 3;

        [YamlMember(Alias = "roll_nested")]
        public bool RollChildProviders { get; set; } = false;

        [YamlMember(Alias = "apply_range_to_sets")]
        public bool ApplyRangeRulesToSetGeneration { get; set; } = false;

        [YamlMember(Alias = "seed")]
        public int Seed { get; set; }

        [YamlMember(Alias = "query_templates")]
        public List<QueryTemplate>? QueryTemplates { get; set; } = new();

        public GenerationRuleSet ToRuleSet()
        {
            return new GenerationRuleSet
            {
                ApplyRangeRulesToSetGeneration = ApplyRangeRulesToSetGeneration,
                DefaultRange = DefaultRange,
                Excluded = YamlExcluded?.SelectMany(x =>
                    ValueGenerator.TryGetValueGenerator(x, out var type)
                        ? type
                        : throw new KeyNotFoundException($"Unable to find value provider for the given name '{x}'")
                ).ToList() ?? new(),
                ExcludedChildren = YamlNestedExcluded?.SelectMany(x =>
                {
                    if (!ValueGenerator.TryGetValueGenerator(x.Key, out var type))
                        throw new KeyNotFoundException($"Unable to find value provider for the given name '{x}'");

                    var values = x.Value.SelectMany(x =>
                        ValueGenerator.TryGetValueGenerator(x, out var type)
                            ? type
                            : throw new KeyNotFoundException($"Unable to find value provider for the given name '{x}'")
                    ).ToList();

                    return type.Select(v => new KeyValuePair<Type, List<Type>>(v, values));
                }).ToDictionary(x => x.Key, x => x.Value) ?? new(),
                MaxDepth = MaxDepth,
                ProviderRanges = YamlRanges?.SelectMany(x =>
                    ValueGenerator.TryGetValueGenerator(x.Key, out var type)
                        ? type.Select(v => new KeyValuePair<Type, Range>(v, x.Value))
                        : throw new KeyNotFoundException($"Unable to find value provider for the given name '{x.Key}'")
                ).ToDictionary(x => x.Key, x => x.Value) ?? GenerationRuleSet.DefaultProviderRanges,
                Name = Name,
                Random = new Random(Seed),
                RollChildProviders = RollChildProviders,
                RootExcluded = YamlRootExcluded?.SelectMany(x =>
                    ValueGenerator.TryGetValueGenerator(x, out var type)
                        ? type
                        : throw new KeyNotFoundException($"Unable to find value provider for the given name '{x}'")
                ).ToList() ?? new(),
                Seed = Seed
            };
        }
    }
}

