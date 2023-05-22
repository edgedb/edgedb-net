using EdgeDB.DataTypes;
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

namespace EdgeDB.StandardLibGenerator
{
    public readonly struct TypeNode
    {
        public readonly string DotnetTypeName
            => DotnetType?.Name?.Replace("`1", "") ?? _dotnetName!;
        
        public readonly string EdgeDBName;
        public readonly Type? DotnetType;
        public readonly bool IsGeneric;
        public readonly TypeNode[] Children;

        public readonly string? TupleElementName;
        public readonly bool IsChildOfNamedTuple;

        public readonly bool RequiresGeneration;
        public readonly bool WasGenerated;
        private readonly string? _dotnetName;

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

            return $"{DotnetTypeName.Replace("`1", "")}<{string.Join(", ", Children)}>";
        }
    }

    public class TypeUtils
    {
        private static readonly Regex GenericRegex = new(@"(.+?)<(.+?)>$");
        private static readonly Regex NamedTupleRegex = new(@"(.*?[^:]):([^:].*?)$");
        internal static readonly Dictionary<string, string> GeneratedTypes = new();

        public static bool TryGetType(string t, [MaybeNullWhen(false)] out TypeNode type)
        {
            if(GeneratedTypes.TryGetValue(t, out var dotnetName))
            {
                type = new(dotnetName, true, t);
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
                // tuple or arry?
                var match = GenericRegex.Match(t);

                if (!match.Success)
                    return false;

                Type? wrapperType = match.Groups[1].Value switch
                {
                    "tuple" => typeof(ValueTuple<>),
                    "array" => typeof(IEnumerable<>),
                    "set" => typeof(IEnumerable<>),
                    "range" => typeof(Range<>),
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

                    return new TypeNode(m.Groups[2].Value, type.DotnetType, m.Groups[1].Value, type.IsGeneric, type.Children);
                });

                if (wrapperType is null || innerTypes.Any(x => !x.HasValue))
                    throw new Exception($"Type {t} not found");

                type = new(t, wrapperType, false, innerTypes.Select(x => x!.Value).ToArray());
            }

            return true;
        }
    }
}
