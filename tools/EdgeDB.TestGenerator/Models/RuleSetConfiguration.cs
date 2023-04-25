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

        [YamlMember(Alias = "query_template")]
        public QueryTemplate? QueryTemplate { get; set; }

        public GenerationRuleSet ToRuleSet()
        {
            return new GenerationRuleSet
            {
                ApplyRangeRulesToSetGeneration = ApplyRangeRulesToSetGeneration,
                DefaultRange = DefaultRange,
                Excluded = YamlExcluded?.Select(x =>
                    ValueGenerator.TryGetValueGenerator(x, out var type)
                        ? type
                        : throw new KeyNotFoundException($"Unable to find value provider for the given name '{x}'")
                ).ToList() ?? new(),
                ExcludedChildren = YamlNestedExcluded?.Select(x =>
                {
                    if (!ValueGenerator.TryGetValueGenerator(x.Key, out var type))
                        throw new KeyNotFoundException($"Unable to find value provider for the given name '{x}'");

                    var values = x.Value.Select(x =>
                        ValueGenerator.TryGetValueGenerator(x, out var type)
                            ? type
                            : throw new KeyNotFoundException($"Unable to find value provider for the given name '{x}'")
                    ).ToList();

                    return new KeyValuePair<Type, List<Type>>(type, values);
                }).ToDictionary(x => x.Key, x => x.Value) ?? new(),
                MaxDepth = MaxDepth,
                ProviderRanges = YamlRanges?.Select(x =>
                    ValueGenerator.TryGetValueGenerator(x.Key, out var type)
                        ? (type, x.Value)
                        : throw new KeyNotFoundException($"Unable to find value provider for the given name '{x.Key}'")
                ).ToDictionary(x => x.type, x => x.Value) ?? new(),
                Name = Name,
                Random = new Random(Seed),
                RollChildProviders = RollChildProviders,
                RootExcluded = YamlRootExcluded?.Select(x =>
                    ValueGenerator.TryGetValueGenerator(x, out var type)
                        ? type
                        : throw new KeyNotFoundException($"Unable to find value provider for the given name '{x}'")
                ).ToList() ?? new(),
                Seed = Seed
            };
        }
    }
}

