using EdgeDB.TestGenerator.ValueProviders.Impl;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;

namespace EdgeDB.TestGenerator
{
    public delegate bool VariableFormatter(GenerationRuleSet rules, VariableIdentifier identifier, [NotNullWhen(true)] out object? value);

    internal static partial class FormatterUtils
    {
        public delegate void KVPMutator(ref string str);

        public static readonly Regex VariableIdentifierRegex = GetVariableIdentifierRegex();
        public static readonly Regex CompleteVariableIdentifierRegex = GetCompleteVariableIdentifierRegex();
        public static readonly Regex NamedRandomRegex = GetKeyedRandomNameRegex();

        private static readonly Dictionary<int, Dictionary<int, string>> _scopeControl = new();
        private static readonly object _scopeMonitor = new();

        public static readonly Dictionary<string, VariableIdentifier?> Identifiers = new()
        {
            { "EDB_TYPE_NAME", VariableIdentifier.EdgeDBTypeName },
            { "EDGEQL_VALUE", VariableIdentifier.EdgeQLValue },
            { "VALUE", VariableIdentifier.Value },
            { "RAND_NAME", VariableIdentifier.RandomName }
        };

        private class ScopedControl : IDisposable
        {
            private readonly int _key;

            public ScopedControl(int key)
            {
                _key = key;
            }

            public void Dispose() => _scopeControl.Remove(_key);
        }

        public static IDisposable Scoped<T>(T key)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            var hashcode = key is int i ? i : key.GetHashCode();

            lock (_scopeMonitor)
            {
                if (_scopeControl.ContainsKey(hashcode))
                    throw new InvalidOperationException($"The key of type {typeof(T)} is already in scope");

                _scopeControl[hashcode] = new Dictionary<int, string>();
                return new ScopedControl(hashcode);
            } 
        }

        [return: NotNullIfNotNull(nameof(value))]
        public static string? Format<T>(GenerationRuleSet rules, string? value, VariableFormatter formatter, T scopeKey)
        {
            if (scopeKey is null)
                throw new ArgumentNullException(nameof(scopeKey));

            if (string.IsNullOrEmpty(value))
                return null;

            Dictionary<int, string> map;

            lock (_scopeMonitor)
            {
                var key = scopeKey is int i ? i : scopeKey.GetHashCode();

                if (!_scopeControl.TryGetValue(key, out map!))
                    throw new KeyNotFoundException($"Cannot find scope key of type {scopeKey}");
            }

            return Format(rules, NamedRandomRegex.Replace(value, m =>
            {
                var mapKey = int.Parse(m.Groups[2].Value);

                if (map.TryGetValue(mapKey, out var value))
                    return value;

                return map[mapKey] = StringValueProvider.Shared.GetRandom(rules);
            }), formatter);
        }

        [return: NotNullIfNotNull(nameof(value))]
        public static string? Format(GenerationRuleSet rules, string? value, VariableFormatter formatter)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            return VariableIdentifierRegex.Replace(value, (v) =>
            {
                var identifier = Identifiers
                        .GetValueOrDefault(v.Groups[2].Value, null)
                        ?? throw new FormatException($"Unknown identifier \"{v.Groups[2].Value}\"");

                return Format(rules, formatter, identifier).ToString()!;
            });
        }

        public static Dictionary<string, object?>? FormatKVP<T>(
            GenerationRuleSet rules,
            VariableFormatter formatProvider,
            Dictionary<string, string>? kvp,
            T scopeKey)
        {
            if (scopeKey is null)
                throw new ArgumentNullException(nameof(scopeKey));

            if (kvp is null)
                return null;

            Dictionary<int, string> map;

            lock (_scopeMonitor)
            {
                var key = scopeKey is int i ? i : scopeKey.GetHashCode();

                if (!_scopeControl.TryGetValue(key, out map!))
                    throw new KeyNotFoundException($"Cannot find scope key of type {scopeKey}");
            }

            return FormatKVP(rules, formatProvider, kvp, (ref string str) =>
            {
                str = NamedRandomRegex.Replace(str, m =>
                {
                    var mapKey = int.Parse(m.Groups[2].Value);

                    if (map.TryGetValue(mapKey, out var value))
                        return value;

                    return map[mapKey] = StringValueProvider.Shared.GetRandom(rules);
                });
            });
        }

        public static Dictionary<string, object?>? FormatKVP(
            GenerationRuleSet rules,
            VariableFormatter formatProvider,
            Dictionary<string, string>? kvp,
            KVPMutator? mutator = null)
        {
            var mutate = mutator ?? ((ref string v) => { });

            return kvp?.Select(kvp =>
            {
                string? key = null;
                object? value = null;

                if (CompleteVariableIdentifierRegex.TryMatch(kvp.Key, out var match) && Identifiers.TryGetValue(match.Groups[1].Value, out var token))
                {
                    key = Format(rules, formatProvider, token!.Value).ToString()!;
                }

                if (CompleteVariableIdentifierRegex.TryMatch(kvp.Value, out match) && Identifiers.TryGetValue(match.Groups[1].Value, out token))
                {
                    value = Format(rules, formatProvider, token!.Value);
                }

                var formattedKey = kvp.Key;
                mutate(ref formattedKey);

                key ??= Format(rules, formattedKey, formatProvider);

                if (value is null)
                {
                    var formattedValue = kvp.Value;
                    mutate(ref formattedValue);

                    var str = Format(rules, formattedValue, formatProvider);
                    value = str;
                }

                return (Key: key, Value: (object?)value);
            }).ToDictionary(x => x.Key!, x => x.Value);
        }

        private static object Format(GenerationRuleSet rules, VariableFormatter formatter, VariableIdentifier identifier)
        {
            if (!formatter(rules, identifier, out var result) && !DefaultFormatter(rules, identifier, out result))
                throw new FormatException($"Unimplemented identifier \"{identifier}\"");

            return result;
        } 

        private static bool DefaultFormatter(GenerationRuleSet rules, VariableIdentifier identifier, [NotNullWhen(true)]out object? obj)
        {
            switch (identifier)
            {
                case VariableIdentifier.RandomName:
                    obj = StringValueProvider.Shared.GetRandom(rules);
                    return true;
                default:
                    obj = null;
                    return false;
            }
        }

        [GeneratedRegex("(<\\$(.+?)>)")]
        private static partial Regex GetVariableIdentifierRegex();

        [GeneratedRegex("^<\\$(.+)>$")]
        private static partial Regex GetCompleteVariableIdentifierRegex();

        [GeneratedRegex("(<\\$RAND_NAME_(\\d+)>)")]
        private static partial Regex GetKeyedRandomNameRegex();
    }
}
