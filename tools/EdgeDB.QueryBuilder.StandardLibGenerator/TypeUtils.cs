using EdgeDB.DataTypes;
using EdgeDB.Models.DataTypes;
using EdgeDB.StandardLibGenerator.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Type = System.Type;

namespace EdgeDB.StandardLibGenerator
{
    public class TypeNode
    {
        public string DotnetTypeName
        {
            get => DotnetType?.Name?.Replace("`1", "") ?? _dotnetName!;
            set => _dotnetName = value;
        }

        public bool IsEnum { get; set; }

        public string EdgeDBName { get; }
        public Type? DotnetType { get; }
        public bool IsGeneric { get; }
        public TypeNode[] Children { get; }

        public string? TupleElementName { get; }
        public bool IsChildOfNamedTuple { get; }

        public bool RequiresGeneration { get; }
        public bool WasGenerated { get; }


        private string? _dotnetName;

        public TypeNode(string name, Type? dotnetType, bool isGeneric, params TypeNode[] children)
        {
            EdgeDBName = name;
            DotnetType = dotnetType;
            IsGeneric = isGeneric;
            Children = children;
            IsChildOfNamedTuple = false;
            TupleElementName = null;
            RequiresGeneration = false;
            _dotnetName = null;
            WasGenerated = false;
        }

        public TypeNode(string name, Type? dotnetType, string tupleName, bool isGeneric, params TypeNode[] children)
        {
            EdgeDBName = name;
            DotnetType = dotnetType;
            IsGeneric = isGeneric;
            Children = children;
            IsChildOfNamedTuple = true;
            TupleElementName = tupleName;
            RequiresGeneration = false;
            _dotnetName = null;
            WasGenerated = false;
        }

        public TypeNode(string name, string? tupleName)
        {
            EdgeDBName = name;
            RequiresGeneration = true;
            DotnetType = null;
            IsGeneric = false;
            Children = Array.Empty<TypeNode>();
            IsChildOfNamedTuple = tupleName is not null;
            TupleElementName = tupleName;
            _dotnetName = null;
            WasGenerated = false;
        }

        public TypeNode(string dotnetName, bool wasGenerated, string edgedbName)
        {
            EdgeDBName = edgedbName;
            RequiresGeneration = true;
            DotnetType = null;
            IsGeneric = false;
            Children = Array.Empty<TypeNode>();
            IsChildOfNamedTuple = false;
            TupleElementName = null;
            _dotnetName = dotnetName;
            WasGenerated = wasGenerated;
        }

        public override string ToString()
        {
            if (Children is null || !Children.Any())
                return DotnetTypeName;

            return $"{DotnetTypeName.Replace("`1", "")}<{string.Join(", ", Children.Select(x => x.ToString()))}>";
        }
    }

    public class TypeUtils
    {
        private static readonly Regex GenericRegex = new(@"(.+?)<(.+?)>$");
        private static readonly Regex NamedTupleRegex = new(@"(.*?[^:]):([^:].*?)$");
        internal static readonly Dictionary<string, TypeNode> GeneratedTypes = new();

        public static async ValueTask<string> BuildType(
            EdgeDBClient client, TypeNode node, TypeModifier modifier,
            string outputPath,
            bool shouldGenerate = true, bool allowGenerics = false, string? genericName = null)
        {
            var name = node.IsGeneric
                ? allowGenerics && genericName is not null ? genericName : "object"
                : node.DotnetType is null && node.RequiresGeneration && shouldGenerate
                    ? await GenerateType(client, node, outputPath)
                    : node.ToString() ?? "object";

            return modifier switch
            {
                TypeModifier.OptionalType => $"{name}?",
                TypeModifier.SingletonType => name,
                TypeModifier.SetOfType => $"IEnumerable<{name}>",
                _ => name
            };
        }

        public static async ValueTask<string> GenerateType(EdgeDBClient client, TypeNode node, string outputPath)
        {
            var edgedbType =
                (await QueryBuilder.Select<Models.Type>().Filter(x => x.Name == node.EdgeDBName).ExecuteAsync(client!))
                .FirstOrDefault();

            if (edgedbType is null)
                throw new Exception($"Failed to find type {node.EdgeDBName}");

            if (TypeUtils.GeneratedTypes.TryGetValue(edgedbType.Name, out var cachedNode))
                return cachedNode.DotnetTypeName;

            var meta = await edgedbType.GetMetaInfoAsync(client!);
            var writer = new CodeWriter();
            string typeName = "";

            using (_ = writer.BeginScope("namespace EdgeDB"))
            {
                switch (meta.Type)
                {
                    case MetaInfoType.Object:
                    {
                    }
                        break;
                    case MetaInfoType.Enum:
                    {
                        node.IsEnum = true;
                        var moduleMatch = Regex.Match(edgedbType.Name, @"(.+)::(.*?)$");
                        writer.AppendLine($"[EdgeDBType(ModuleName = \"{moduleMatch.Groups[1].Value}\")]");
                        typeName = moduleMatch.Groups[2].Value!;
                        node.DotnetTypeName = typeName;
                        using (_ = writer.BeginScope($"public enum {typeName}"))
                        {
                            foreach (var value in meta.EnumValues!)
                            {
                                writer.AppendLine($"{value},");
                            }
                        }
                    }
                        break;
                    case MetaInfoType.Unknown:
                    {
                        // generate an empty struct
                        var moduleMatch = Regex.Match(edgedbType.Name, @"(.+)::(.*?)$");
                        writer.AppendLine($"[EdgeDBType(ModuleName = \"{moduleMatch.Groups[1].Value}\")]");
                        typeName = FunctionGenerator.TextInfo.ToTitleCase(moduleMatch.Groups[2].Value!);
                        node.DotnetTypeName = typeName;
                        using (_ = writer.BeginScope($"public readonly struct {typeName}"))
                        {}
                    }
                        break;
                    default:
                        throw new Exception($"Unknown stdlib builder for type {edgedbType.Id} {edgedbType.Name}");
                }
            }

            File.WriteAllText(Path.Combine(outputPath, $"{typeName}.g.cs"), writer.ToString());
            GeneratedTypes.Add(edgedbType.Name, node);
            return typeName;
        }

        public static bool TryGetType(string t, [MaybeNullWhen(false)] out TypeNode type)
        {
            if (GeneratedTypes.TryGetValue(t, out type))
            {
                return true;
            }

            type = default;

            var dotnetType = t switch
            {
                "std::set" => typeof(IEnumerable),
                "std::Object" => typeof(object),
                "std::bool" => typeof(bool),
                "std::bytes" => typeof(byte[]),
                "std::str" => typeof(string),
                "cal::local_date" => typeof(DateOnly),
                "cal::local_time" => typeof(TimeSpan),
                "cal::local_datetime" => typeof(System.DateTime),
                "cal::relative_duration" => typeof(TimeSpan),
                "cal::date_duration" => typeof(TimeSpan),
                "std::datetime" => typeof(DateTimeOffset),
                "std::duration" => typeof(TimeSpan),
                "std::float32" => typeof(float),
                "std::float64" => typeof(double),
                "std::int8" => typeof(sbyte),
                "std::int16" => typeof(short),
                "std::int32" => typeof(int),
                "std::int64" => typeof(long),
                "std::bigint" => typeof(BigInteger),
                "std::decimal" => typeof(decimal),
                "std::uuid" => typeof(Guid),
                "std::json" => typeof(Json),
                "schema::ScalarType" => typeof(Type),
                _ => null
            };

            if (dotnetType is not null)
                type = new(t, dotnetType, false);
            else if (t.StartsWith("any") || t.StartsWith("std::any"))
                type = new(t, null, true);
            else
            {
                // tuple or array?
                var match = GenericRegex.Match(t);

                if (!match.Success)
                    return false;

                Type? wrapperType = match.Groups[1].Value switch
                {
                    "tuple" => typeof(ValueTuple<>),
                    "array" => typeof(IEnumerable<>),
                    "set" => typeof(IEnumerable<>),
                    "range" => typeof(Range<>),
                    "multirange" => typeof(MultiRange<>),
                    _ => null
                };

                var innerTypes = match.Groups[2].Value.Split(", ").Select(x =>
                {
                    var t = x.Replace("|", "::");
                    var m = NamedTupleRegex.Match(t);
                    if (!m.Success)
                        return TryGetType(t, out var lt) ? lt : (TypeNode?)null;

                    if (!TryGetType(m.Groups[2].Value, out var type))
                        return new(m.Groups[2].Value, m.Groups[1].Value);

                    return new TypeNode(m.Groups[2].Value, type.DotnetType, m.Groups[1].Value, type.IsGeneric,
                        type.Children);
                });

                if (wrapperType is null || innerTypes.Any(x => x is null))
                    throw new Exception($"Type {t} not found");

                type = new(t, wrapperType, false, innerTypes.ToArray()!);
            }

            return true;
        }
    }
}
