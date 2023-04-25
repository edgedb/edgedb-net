using EdgeDB.TestGenerator.ValueProviders;
using EdgeDB.TestGenerator.ValueProviders.Impl;
using System.Text;
using YamlDotNet.Serialization;

namespace EdgeDB.TestGenerator
{
    public sealed class GenerationRuleSet
    {
        public static Dictionary<Type, Range> DefaultProviderRanges = new Dictionary<Type, Range>
        {
            { typeof(ArrayValueProvider),             5..20 },
            { typeof(BigIntValueProvider),            4..15 },
            { typeof(BytesValueProvider),            10..20 },
            { typeof(DateDurationValueProvider),       ..15 },
            { typeof(DecimalValueProvider),            ..100 },
            { typeof(DurationValueProvider),           ..500 },
            { typeof(Float32ValueProvider),            ..100 },
            { typeof(Float64ValueProvider),            ..100 },
            { typeof(Int16ValueProvider),              ..short.MaxValue },
            { typeof(Int32ValueProvider),              ..int.MaxValue },
            { typeof(RelativeDurationValueProvider),   ..100 },
            { typeof(StringValueProvider),           15..15 }
        };

        public string? Name { get; set; }

        public Range DefaultRange { get; set; } = 1..10;

        public Dictionary<Type, Range> ProviderRanges { get; set; } = new(DefaultProviderRanges);

        public List<Type> RootExcluded { get; set; } = new();

        public int MaxDepth { get; set; } = 3;

        public List<Type> Excluded { get; set; } = new();

        public Dictionary<Type, List<Type>> ExcludedChildren { get; set; } = new();

        public bool RollChildProviders { get; set; } = false;

        public bool ApplyRangeRulesToSetGeneration { get; set; } = false;

        public int Seed { get; set; }

        public Random Random { get; set; }

        public GenerationRuleSet()
        {
            Random ??= new Random(Seed);
        }

        public Random RefreshRandom() => Random = new Random(Seed);

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"MaxDepth: [blue]{MaxDepth}[/]");
            sb.AppendLine($"Rolling Children: [cyan]{RollChildProviders}[/]");
            sb.AppendLine($"Excluded: {(Excluded.Any() ? $"[silver][[\n  [/]{string.Join("[grey],[/]\n  ", Excluded.Select(x => $"[green]{x.Name}[/]"))}[silver]\n]][/]" : "[[]]")}");
            sb.AppendLine($"Root Excluded: {(RootExcluded.Any() ? $"[silver][[\n  [/]{string.Join("[grey],[/]\n  ", RootExcluded.Select(x => $"[green]{x.Name}[/]"))}[silver]\n]][/]" : "")}");

            if (ExcludedChildren.Any())
            {
                sb.AppendLine("Excluded Children:");
                foreach (var c in ExcludedChildren)
                {
                    sb.AppendLine($"[silver] -[/] [green]{c.Key.Name}[/]: [silver][[[/]\n     {string.Join("[silver],[/]\n     ", c.Value.Select(x => $"[green]{x.Name}[/]"))}[silver]\n   ]][/]");
                }
            }

            if (DefaultProviderRanges.Any())
            {
                sb.AppendLine("Range Overrides:");

                foreach (var range in DefaultProviderRanges)
                {
                    sb.AppendLine($"[silver] -[/] [green]{range.Key.Name}[/]: [cyan]{range.Value}[/]");
                }
            }

            sb.AppendLine($"Default Range: [cyan]{DefaultRange}[/]");
            sb.Append($"Apply range-rule to sets: [cyan]{ApplyRangeRulesToSetGeneration}[/]");

            return sb.ToString();
        }

        public Range GetRange<T>()
            where T : IValueProvider
            => ProviderRanges.TryGetValue(typeof(T), out var v) ? v : DefaultRange;

        public Range GetRange(Type type)
            => ProviderRanges.TryGetValue(type, out var v) ? v : DefaultRange;

        public GenerationRuleSet Clone()
            => (GenerationRuleSet)MemberwiseClone();
    }
}
