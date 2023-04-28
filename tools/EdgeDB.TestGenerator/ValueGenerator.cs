using EdgeDB.TestGenerator.ValueProviders;
using EdgeDB.TestGenerator.ValueProviders.Impl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

namespace EdgeDB.TestGenerator
{
    internal static class ValueGenerator
    {
        public class GenerationResult
        {
            public required object Value { get; init; }
            public required string EdgeDBTypeName { get; init; }
            public required IValueProvider Provider { get; init; }

            public string ToEdgeQLFormat()
                => _edgeqlFormat ??= Provider.ToEdgeQLFormat(Value);

            private string? _edgeqlFormat;

            public bool FormatVariableIdentifier(GenerationRuleSet _, VariableIdentifier identifier, [NotNullWhen(true)] out object? obj)
            {
                switch (identifier)
                {
                    case VariableIdentifier.EdgeDBTypeName:
                        obj = EdgeDBTypeName;
                        return true;
                    case VariableIdentifier.EdgeQLValue:
                        obj = ToEdgeQLFormat();
                        return true;
                    case VariableIdentifier.Value:
                        obj = Value;
                        return true;
                    default:
                        obj = null;
                        return false;
                }
            }
        }

        #region Defined rulesets
        public static GenerationRuleSet SmallJsonBlob = new()
        {
            ApplyRangeRulesToSetGeneration = true,
            MaxDepth = 1,
            DefaultRange = 1..2,
            Excluded = new List<Type>
            {
                typeof(JsonValueProvider),
                typeof(SetValueProvider),
                typeof(ArrayValueProvider),
                typeof(NamedTupleValueProvider),
                typeof(TupleValueProvider),
                typeof(IdenticalNamedTupleValueProvider),
                typeof(ObjectValueProvider),
                typeof(IdenticallyNamedObjectValueProvider)
            }
        };
        #endregion

        private static readonly Dictionary<Type, List<Type>> _compatableWrappingTypes;
        public static readonly List<Type> ValueGenerators;
        public static readonly Dictionary<string, List<Type>> ValueGeneratorsMap;

        static ValueGenerator()
        {
            ValueGenerators = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.IsAssignableTo(typeof(IValueProvider)) && !x.IsInterface)
                .ToList();

            ValueGeneratorsMap = ValueGenerators.Select(x => ((IValueProvider)Activator.CreateInstance(x)!))
                .GroupBy(x => x.EdgeDBName)
                .ToDictionary(x => x.Key, x => x.Select(x => x.GetType()).ToList());

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

        public static bool TryGetValueGenerator(string name, [MaybeNullWhen(false)] out List<Type> type)
        {
            if (ValueGeneratorsMap.TryGetValue(name, out type))
                return true;

            type = ValueGeneratorsMap.FirstOrDefault(x => x.Key.Contains("::") ? x.Key.Split("::")[1] == name : x.Key == name).Value;

            return type != null;
        }

        #region Set generation

        public static List<IValueProvider> GenerateCompleteSet(GenerationRuleSet? rules = null)
        {
            rules ??= new GenerationRuleSet();

            var allowed = ValueGenerators.Except(rules.Excluded).Except(rules.RootExcluded);

            var result = new List<IValueProvider>();

            var depth = rules.MaxDepth;
            for(int i = 0; i != depth; i++)
            {
                var cloned = rules.Clone();
                cloned.MaxDepth = i;

                foreach (var allowedProvider in allowed)
                {
                    if (allowedProvider.IsAssignableTo(typeof(IWrappingValueProvider)))
                    {
                        result.AddRange(GenerateNestedSetProviders(allowedProvider, cloned));
                    }
                    else
                    {
                        result.Add((IValueProvider)Activator.CreateInstance(allowedProvider)!);
                    }
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

                var count = rules.Random.Next(range);

                if(count < childrenCount && count > 0)
                {
                    allowedChildren = allowedChildren.OrderRandomly(rules.Random).Take(count).ToArray();
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
                        inst.Children = initChildren.ToArray();

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
            }).ToArray();

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
            var provider = ValueGenerators.Except(rules.Excluded).Random(rules.Random);

            return provider.IsAssignableTo(typeof(IWrappingValueProvider))
                ? InitializeWrappingProvider(provider, rules)
                : (IValueProvider)Activator.CreateInstance(provider)!;
        }


        private static IValueProvider InitializeWrappingProvider(Type wrapping, GenerationRuleSet rules, int depth = 0)
        {
            var allowedChildren = _compatableWrappingTypes.TryGetValue(wrapping, out var value)
                ? value.Except(rules.Excluded)
                : ValueGenerators.Except(rules.Excluded);

            if (rules.ExcludedChildren.TryGetValue(wrapping, out var exChildren))
                allowedChildren = allowedChildren.Except(exChildren);

            if (depth >= rules.MaxDepth)
            {
                allowedChildren = allowedChildren.Where(x => !x.IsAssignableTo(typeof(IWrappingValueProvider)));
            }

            if (rules.ApplyRangeRulesToSetGeneration)
            {
                var range = rules.GetRange(wrapping);

                var count = rules.Random.Next(range);

                if (count < allowedChildren.Count() && count > 0)
                {
                    allowedChildren = allowedChildren.OrderRandomly(rules.Random).Take(count).ToArray();
                }
            }

            allowedChildren = allowedChildren.RandomSequence(rules.Random);

            if (!allowedChildren.Any())
            {
                //return null;
                throw new InvalidOperationException("No valid children found after exclusion of inputted rule set");
            }

            var inst = (IWrappingValueProvider)Activator.CreateInstance(wrapping)!;

            inst.Children = allowedChildren.Select(x => x.IsAssignableTo(typeof(IWrappingValueProvider))
                ? InitializeWrappingProvider(x, rules, depth + 1)
                : (IValueProvider)Activator.CreateInstance(x)!
            ).ToArray();

            return inst;
        }
        #endregion

        private static IEnumerable<Type> GetAllowedChildren(Type target, GenerationRuleSet rules)
        {
            return _compatableWrappingTypes.TryGetValue(target, out var value)
                ? value.Except(rules.Excluded)
                : ValueGenerators.Except(rules.Excluded);
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
