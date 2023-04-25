using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using Formatter = System.Func<EdgeDB.TestGenerator.VariableIdentifier, object>;

namespace EdgeDB.TestGenerator
{
    public partial class QueryTemplate
    {
        public static readonly Regex VariableIdentifierRegex = GetVariableIdentifierRegex();

        [YamlMember(Alias = "name")]
        public string? Name { get; set; }

        [YamlMember(Alias = "query")]
        public string? Query { get; set; }

        [YamlMember(Alias = "arguments")]
        public Dictionary<string, string>? Arguments { get; set; }

        
        public string? FormatName(Formatter formatProvider)
            => Format(Name!, formatProvider);

        public string? FormatQuery(Formatter formatProvider)
            => Format(Query!, formatProvider);

        public Dictionary<string, object?>? FormatArguments(Formatter formatProvider)
        {
            return Arguments?.Select(kvp =>
            {
                string? key = null;
                object? value = null;

                if (Identifiers.TryGetValue(kvp.Key, out var token))
                {
                    key = formatProvider(token!.Value).ToString()!;
                }

                if (Identifiers.TryGetValue(kvp.Value, out token))
                {
                    value = formatProvider(token!.Value);
                }

                key ??= Format(kvp.Key, formatProvider);
                value ??= Format(kvp.Value, formatProvider);

                return (Key: key, Value: (object?)value);
            }).ToDictionary(x => x.Key!, x => x.Value);
        }

        public static readonly Dictionary<string, VariableIdentifier?> Identifiers = new()
        {
            { "EDB_TYPE_NAME", VariableIdentifier.EdgeDBTypeName },
            { "EDGEQL_VALUE", VariableIdentifier.EdgeQLValue },
            { "VALUE", VariableIdentifier.Value }
        };

        private static string Format(string value, Formatter formatter)
        {
            return VariableIdentifierRegex.Replace(value, (v) =>
            {
                return formatter(
                    Identifiers
                        .GetValueOrDefault(v.Groups[2].Value, null)
                        ?? throw new FormatException($"Unknown identifier \"{v.Groups[2].Value}\"")
                ).ToString()!;
            });
        }

        [GeneratedRegex("(<\\$(.+?)>)")]
        private static partial Regex GetVariableIdentifierRegex();
    }
}

