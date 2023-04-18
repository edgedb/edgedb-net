using EdgeDB.TestGenerator.ValueProviders;
using EdgeDB.TestGenerator.ValueProviders.Impl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.TestGenerator
{
    internal static class ValueGenerator
    {
        public class GenerationRuleSet
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

            public Range DefaultRange { get; set; } = 1..10;
            public Dictionary<Type, Range> ProviderCollectionSizes { get; set; } = new(DefaultProviderRanges);
            public List<Type> RootExclude { get; set; } = new();
            public int MaxDepth { get; set; } = 10;
            public List<Type> Excluded { get; set; } = new();
            public Dictionary<Type, List<Type>> ExcludedChildren { get; set; } = new();
            public bool RollChildProviders { get; set; } = false;
            public bool ApplyRangeRulesToSetGeneration { get; set; } = false;

            public override string ToString()
            {
                var sb = new StringBuilder();

                sb.AppendLine($"MaxDepth: [blue]{MaxDepth}[/]");
                sb.AppendLine($"Rolling Children: [cyan]{RollChildProviders}[/]");
                sb.AppendLine($"Excluded: [silver][[\n  [/]{string.Join("[grey],[/]\n  ", Excluded.Select(x => $"[green]{x.Name}[/]"))}[silver]\n]][/]");
                sb.AppendLine($"Root Excluded: [silver][[\n  [/]{string.Join("[grey],[/]\n  ", RootExclude.Select(x => $"[green]{x.Name}[/]"))}[silver]\n]][/]");

                if (ExcludedChildren.Any())
                {
                    sb.AppendLine("Excluded Children:");
                    foreach(var c in ExcludedChildren)
                    {
                        sb.AppendLine($"[silver] -[/] [green]{c.Key.Name}[/]: [silver][[[/]\n     {string.Join("[silver],[/]\n      ", c.Value.Select(x => $"[green]{x.Name}[/]"))}[silver]\n   ]][/]");
                    } 
                }

                if (DefaultProviderRanges.Any())
                {
                    sb.AppendLine("Range Overrides:");

                    foreach(var range in DefaultProviderRanges)
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
                => ProviderCollectionSizes.TryGetValue(typeof(T), out var v) ? v : DefaultRange;

            public Range GetRange(Type type)
                => ProviderCollectionSizes.TryGetValue(type, out var v) ? v : DefaultRange;
        }

        public class GenerationResult
        {
            public required object Value { get; init; }
            public required string EdgeDBTypeName { get; init; }
            public required IValueProvider Provider { get; init; }

            public string ToEdgeQLFormat()
                => Provider.ToEdgeQLFormat(Value);

        }

        #region Defined rulesets

        public static GenerationRuleSet V2ArugmentRuleSet = new()
        {
            Excluded = new List<Type>
            {
                typeof(SetValueProvider), typeof(ObjectValueProvider),
                typeof(TupleValueProvider), typeof(NamedTupleValueProvider)
            },
            ExcludedChildren = new Dictionary<Type, List<Type>>
            {
                {typeof(ArrayValueProvider), new List<Type>() { typeof(ArrayValueProvider)} }
            }
        };

        public static GenerationRuleSet SmallJsonBlob = new()
        {
            ApplyRangeRulesToSetGeneration = true,
            MaxDepth = 2,
            DefaultRange = 1..2,
            Excluded = new List<Type>
            {
                typeof(JsonValueProvider)
            }
        };

        public static GenerationRuleSet QueryResultRuleSet = new()
        {
            MaxDepth = 2,
            ExcludedChildren = new Dictionary<Type, List<Type>>()
            {
                { typeof(ArrayValueProvider), new List<Type> { typeof(ArrayValueProvider) } }
            }
        };

        public static GenerationRuleSet DeepQueryResultNesting = new()
        {
            MaxDepth = 3,
            Excluded = new List<Type>
            {
                typeof(JsonValueProvider),
                typeof(BigIntValueProvider),
                typeof(BooleanValueProvider),
                typeof(BytesValueProvider),
                typeof(DateDurationValueProvider),
                typeof(DateTimeValueProvider),
                typeof(DecimalValueProvider),
                typeof(DurationValueProvider),
                typeof(Float32ValueProvider),
                typeof(Float64ValueProvider),
                typeof(Int16ValueProvider),
                typeof(Int32ValueProvider),
                typeof(LocalDateTimeValueProvider),
                typeof(LocalDateValueProvider),
                typeof(LocalTimeValueProvider),
                typeof(RelativeDurationValueProvider),
                typeof(UUIDValueProvider)
            },
            RootExclude = new List<Type>
            {
                typeof(StringValueProvider),
                typeof(Int64ValueProvider)
            },
            ExcludedChildren = new Dictionary<Type, List<Type>>()
            {
                { typeof(ArrayValueProvider), new List<Type> { typeof(ArrayValueProvider) } }
            },
            RollChildProviders = false,
            DefaultRange = 2..2,
            ProviderCollectionSizes = new()
            {
                {typeof(StringValueProvider), 15..15 }
            }
        };

        #endregion

        private static readonly Dictionary<Type, List<Type>> _compatableWrappingTypes;
        private static readonly List<Type> _valueGenerators;

        static ValueGenerator()
        {
            _valueGenerators = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.IsAssignableTo(typeof(IValueProvider)) && !x.IsInterface)
                .ToList();

            _compatableWrappingTypes = new Dictionary<Type, List<Type>>()
            {
                {typeof(RangeValueProvider), new List<Type>
                {
                    typeof(Int32ValueProvider),
                    typeof(Int64ValueProvider),
                    typeof(Float32ValueProvider),
                    typeof(Float64ValueProvider),
                    typeof(DecimalValueProvider),
                    typeof(DateTimeValueProvider),
                    typeof(LocalDateTimeValueProvider),
                    typeof(LocalDateValueProvider)
                } }
            };
        }

        #region Set generation

        public static List<IValueProvider> GenerateCompleteSet(GenerationRuleSet? rules = null)
        {
            rules ??= new GenerationRuleSet();

            var allowed = _valueGenerators.Except(rules.Excluded).Except(rules.RootExclude);

            var result = new List<IValueProvider>();

            foreach(var allowedProvider in allowed)
            {
                if (allowedProvider.IsAssignableTo(typeof(IWrappingValueProvider)))
                {
                    result.AddRange(GenerateNestedSetProviders(allowedProvider, rules));
                }
                else
                {
                    result.Add((IValueProvider)Activator.CreateInstance(allowedProvider)!);
                }
            }

            return result;
        }

        private static IEnumerable<IValueProvider> GenerateNestedSetProviders(Type target, GenerationRuleSet rules, int curDepth = 0)
        {
            var allowedChildren = GetAllowedChildren(target, rules);

            if(rules.ExcludedChildren.TryGetValue(target, out var targetExcluded))
            {
                allowedChildren = allowedChildren.Except(targetExcluded);
            }

            var childrenCount = allowedChildren.Count();

            if(childrenCount == 0)
            {
                throw new ArgumentException($"The defined rules restricts the type {target} to contain no children");
            }

            if(rules.ApplyRangeRulesToSetGeneration)
            {
                var range = rules.GetRange(target);

                var count = Random.Shared.Next(range);

                if(count < childrenCount && count > 0)
                {
                    allowedChildren = allowedChildren.OrderRandomly().Take(count);
                }
            }

            if(rules.RollChildProviders)
            {
                return allowedChildren
                    .Roll()
                    .Select(c =>
                    {
                        var initChildren = c.SelectMany(x =>
                        {
                            if (x.IsAssignableTo(typeof(IWrappingValueProvider)))
                            {
                                if (curDepth < rules.MaxDepth)
                                {
                                    return GenerateNestedSetProviders(x, rules, curDepth + 1);
                                }
                                else
                                    return Array.Empty<IValueProvider>();
                            }

                            return new IValueProvider[] { (IValueProvider)Activator.CreateInstance(x)! };
                        });

                        if (!initChildren.Any())
                        {
                            return null;
                        }

                        var inst = (IWrappingValueProvider)Activator.CreateInstance(target)!;
                        inst.Children = initChildren;

                        return inst;
                    })
                    .Where(x => x != null)
                    .Select(x => x!);
            }

            var inst = (IWrappingValueProvider)Activator.CreateInstance(target)!;
            inst.Children = allowedChildren.SelectMany(x =>
            {
                if (x.IsAssignableTo(typeof(IWrappingValueProvider)))
                {
                    if (curDepth < rules.MaxDepth)
                    {
                        return GenerateNestedSetProviders(x, rules, curDepth + 1);
                    }
                    else
                        return Array.Empty<IValueProvider>();
                }

                return new IValueProvider[] { (IValueProvider)Activator.CreateInstance(x)! };
            });

            return new IValueProvider[] { inst };
        }

        #endregion

        #region Random generation

        public static GenerationResult GenerateRandom()
            => GetRandom(new GenerationRuleSet()).AsResult(new GenerationRuleSet());

        public static GenerationResult GenerateRandom(GenerationRuleSet rules)
            => GetRandom(rules).AsResult(rules);

        public static GenerationResult GetRandom<T>()
            where T : IValueProvider
        {
            return (typeof(T).IsAssignableTo(typeof(IWrappingValueProvider))
               ? InitializeWrappingProvider(typeof(T), new GenerationRuleSet())
               : (IValueProvider)Activator.CreateInstance(typeof(T))!).AsResult(new());
        }

        public static GenerationResult GetRandom<T>(GenerationRuleSet rules)
            where T : IValueProvider
        {
            return (typeof(T).IsAssignableTo(typeof(IWrappingValueProvider))
               ? InitializeWrappingProvider(typeof(T), rules)
               : (IValueProvider)Activator.CreateInstance(typeof(T))!).AsResult(rules);
        }


        private static IValueProvider GetRandom(GenerationRuleSet rules)
        {
            var provider = _valueGenerators.Except(rules.Excluded).Random();

            return provider.IsAssignableTo(typeof(IWrappingValueProvider))
                ? InitializeWrappingProvider(provider, rules)
                : (IValueProvider)Activator.CreateInstance(provider)!;
        }


        private static IValueProvider InitializeWrappingProvider(Type wrapping, GenerationRuleSet rules, int depth = 0)
        {
            var allowedChildren = _compatableWrappingTypes.TryGetValue(wrapping, out var value)
                ? value.Except(rules.Excluded)
                : _valueGenerators.Except(rules.Excluded);

            if (rules.ExcludedChildren.TryGetValue(wrapping, out var exChildren))
                allowedChildren = allowedChildren.Except(exChildren);

            if (depth >= rules.MaxDepth)
            {
                allowedChildren = allowedChildren.Where(x => !x.IsAssignableTo(typeof(IWrappingValueProvider)));
            }

            allowedChildren = allowedChildren.RandomSequence();

            if (!allowedChildren.Any())
            {
                //return null;
                throw new InvalidOperationException("No valid children found after exclusion of inputted rule set");
            }

            var inst = (IWrappingValueProvider)Activator.CreateInstance(wrapping)!;

            inst.Children = allowedChildren.Select(x => x.IsAssignableTo(typeof(IWrappingValueProvider))
                ? InitializeWrappingProvider(x, rules, depth + 1)
                : (IValueProvider)Activator.CreateInstance(x)!
            );

            return inst;
        }
        #endregion

        private static IEnumerable<Type> GetAllowedChildren(Type target, GenerationRuleSet rules)
        {
            return _compatableWrappingTypes.TryGetValue(target, out var value)
                ? value.Except(rules.Excluded)
                : _valueGenerators.Except(rules.Excluded);
        }

        public static GenerationResult AsResult(this IValueProvider provider, GenerationRuleSet rules)
        {
            return new GenerationResult
            {
                EdgeDBTypeName = provider is IWrappingValueProvider w
                    ? w.FormatAsGeneric()
                    : provider.EdgeDBName,
                Value = provider.GetRandom(rules),
                Provider = provider
            };
        }
    }
}
