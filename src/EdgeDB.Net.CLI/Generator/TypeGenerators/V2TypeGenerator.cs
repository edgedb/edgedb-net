using EdgeDB.Binary.Codecs;
using EdgeDB.CLI.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Generator.TypeGenerators
{
    internal sealed class ObjectGenerationInfo
    {
        public string CleanTypeName
            => TypeName.Contains("::") ? TypeName.Split("::")[1] : TypeName;
        public string TypeName { get; }
        public ObjectCodec Codec { get; }
        public Dictionary<string, string> PropertyTypeMap { get; }

        public ObjectGenerationInfo(string typename, ObjectCodec codec, Dictionary<string, string> propertyTypeMap)
        {
            TypeName = typename;
            Codec = codec;
            PropertyTypeMap = propertyTypeMap;
        }
    }

    internal sealed class V2TypeGeneratorContext : ITypeGeneratorContext
    {
        public required GeneratorContext GeneratorContext { get; init; }

        public string? RootName { get; set; }

        private int _subtypeCount;

        public readonly Dictionary<Guid, List<ObjectGenerationInfo>> ResultShapes;

        public V2TypeGeneratorContext()
        {
            ResultShapes = new();
        }

        public string GetSubtypeName()
            => $"{RootName}SubType{_subtypeCount++}";

    }

    internal sealed class V2TypeGenerator : ITypeGenerator<V2TypeGeneratorContext>
    {
        public V2TypeGeneratorContext CreateContext(GeneratorContext generatorContext)
            => new() { GeneratorContext = generatorContext };

        public ValueTask<string> GetTypeAsync(ICodec codec, GeneratorTargetInfo? target, V2TypeGeneratorContext context)
        {
            context.RootName = target is null ? "Unspecified" : target.FileName;

            return GetTypeAsync(codec, context.RootName, context);
        }

        private async ValueTask<string> GetTypeAsync(ICodec codec, string? name, V2TypeGeneratorContext context)
        {
            switch (codec)
            {
                case SparceObjectCodec:
                    throw new InvalidOperationException("Cannot parse sparce object codec");
                case ObjectCodec obj:
                    return await GenerateResultType(obj, name, context);
                case TupleCodec tuple:
                    var elements = new string[tuple.InnerCodecs.Length];
                    for (var i = 0; i != elements.Length; i++)
                        elements[i] = await GetTypeAsync(codec, $"{name ?? context.GetSubtypeName()}TupleElement{i}", context);
                    return $"({string.Join(", ", elements)})";
                case CompilableWrappingCodec:
                case IWrappingCodec when codec.GetType().IsGenericType:
                    var inner = codec is CompilableWrappingCodec c ? c.InnerCodec : codec is IWrappingCodec w ? w.InnerCodec : throw new Exception("This isn't possible");
                    if (codec.GetType().GetGenericTypeDefinition() == typeof(ArrayCodec<>))
                        return $"{await GetTypeAsync(inner, name, context)}[]";
                    else if (codec.GetType().GetGenericTypeDefinition() == typeof(RangeCodec<>))
                        return $"Range<{await GetTypeAsync(inner, name, context)}>";
                    else if (codec.GetType().GetGenericTypeDefinition() == typeof(SetCodec<>))
                        return $"List<{await GetTypeAsync(inner, name, context)}>";
                    else
                        throw new EdgeDBException($"Unknown wrapping codec {inner}");
                case IScalarCodec scalar:
                    return scalar.ConverterType.Name;
                case NullCodec:
                    return "object";
                default:
                    throw new InvalidOperationException($"Unknown type parse path for codec {codec}");
            }
        }

        private async ValueTask<string> GenerateResultType(ObjectCodec obj, string? name, V2TypeGeneratorContext context)
        {
            if (name is not null)
                name = $"{name}{TextUtils.CleanTypeName(obj.Metadata?.SchemaName) ?? "Result"}";
            else
                name = $"{context.GetSubtypeName()}{TextUtils.CleanTypeName(obj.Metadata?.SchemaName) ?? "Result"}";

            var map = new Dictionary<string, string>();

            for(var i = 0; i != obj.Properties.Length; i++)
            {
                var propType = await GetTypeAsync(obj.InnerCodecs[i], name: null, context);
                map.Add(obj.Properties[i].Name, propType);
            }

            var info = new ObjectGenerationInfo(name, obj, map);
            context.ResultShapes.TryAdd(obj.Id, new());
            context.ResultShapes[obj.Id].Add(info);

            return info.CleanTypeName;
        }

        public async ValueTask PostProcessAsync(V2TypeGeneratorContext context)
        {
            var groups = context.ResultShapes.Values
                .SelectMany(x => x)
                .GroupBy(x => x.Codec.Metadata is null ? x.Codec.Id.ToString() : x.Codec.Metadata.SchemaName ?? context.GetSubtypeName());

            foreach(var group in groups)
            {
                var iname = group.Key.Contains("::") ? group.Key.Split("::")[1] : group.Key;

                var map = await GenerateInterfaceAsync(
                    group.Key,
                    iname,
                    group.Select(x =>
                        x.PropertyTypeMap.Select(y =>
                            (y.Value, x.Codec.Properties.First(z => z.Name == y.Key))
                        ).ToArray()
                    ),
                    context,
                    group.Select(x => x.CleanTypeName),
                    prop => group.Where(x => x.PropertyTypeMap.ContainsKey(prop)).Select(x => x.CleanTypeName)
                );

                foreach(var type in group)
                {
                    var propsMap = type.PropertyTypeMap.Select(x =>
                    {
                        var property = type.Codec.Properties.First(y => y.Name == x.Key);

                        return $"[EdgeDBProperty(\"{property.Name}\")]\n    public {CodeGenerator.ApplyCardinality(x.Value, property.Cardinality)} {TextUtils.ToPascalCase(x.Key)} {{ get; set; }}";
                    });

                    var ifaceMap = map
                        .Where(x => !group.All(y => y.PropertyTypeMap.ContainsKey(x.Key)))
                        .Select(x =>
                        {
                            var isContained = type.PropertyTypeMap.ContainsKey(x.Key);

                            var typename = CodeGenerator.ApplyCardinality(x.Value.Item1.Type, x.Value.Item1.Value.Cardinality);

                            return !isContained
                                ? $"Optional<{typename}> I{iname}.{TextUtils.ToPascalCase(x.Key)} => Optional<{typename}>.Unspecified;"
                                : $"Optional<{typename}> I{iname}.{TextUtils.ToPascalCase(x.Key)} => {TextUtils.ToPascalCase(x.Key)};";
                        });

                    var ifaceStrMap = ifaceMap.Any()
                        ? $"// I{iname}\n    {string.Join("\n    ", ifaceMap)}"
                        : string.Empty;

                    var code =
$$"""
using EdgeDB;

namespace {{context.GeneratorContext.GenerationNamespace}};

#nullable enable
#pragma warning disable CS8618 // nullablility is controlled by EdgeDB

public class {{type.CleanTypeName}} : I{{iname}}
{
    {{string.Join("\n\n    ", propsMap)}}

    {{ifaceStrMap}}
}

#nullable restore
#pragma warning restore CS8618
""";

                    await File.WriteAllTextAsync(Path.Combine(context.GeneratorContext.OutputDirectory, "Results", $"{type.CleanTypeName}.g.cs"), code);
                }
            }
        }

        private async Task<Dictionary<string, ((string Type, ObjectProperty Value), bool)>> GenerateInterfaceAsync(
            string schemaName, string name,
            IEnumerable<(string Type, ObjectProperty Value)[]> properties, V2TypeGeneratorContext context,
            IEnumerable<string> decendants, Func<string, IEnumerable<string>> getAvailability)
        {
            var props = properties
                .SelectMany(x => x)
                .DistinctBy(x => x.Value.Name)
                .ToDictionary(x => x, x => properties.All(y => y.Select(x => x.Value).Contains(x.Value, PropertyComparator.Instance)));

            var propStr =
                string.Join("\n    ", props.Select(x =>
                {
                    var type = CodeGenerator.ApplyCardinality(x.Key.Type, x.Key.Value.Cardinality);

                    if (!x.Value)
                        type = $"Optional<{type}>";

                    var summary = $"/// <summary>\n    ///     The <c>{x.Key.Value.Name}</c> property available on ";

                    if (x.Value)
                    {
                        summary += "all descendants of this interface.\n";
                    }
                    else
                    {
                        var availableOn = getAvailability(x.Key.Value.Name);
                        summary += $"the following types:<br/>\n    ///     {string.Join("<br/>\n    ///         ", availableOn.Select(x => $"<see cref=\"{x}\"/>"))}\n";
                    }

                    summary += "    /// </summary>\n    ";

                    return $"{summary}{type} {TextUtils.ToPascalCase(x.Key.Value.Name)} {{ get; }}\n";
                }));

            var code =
$$"""
// {{schemaName}} abstraction
using EdgeDB;

#nullable enable
#pragma warning disable CS8618 // nullablility is controlled by EdgeDB

namespace {{context.GeneratorContext.GenerationNamespace}};

/// <summary>
///     Represents the schema type <c>{{schemaName}}</c> with properties that are shared across the following types:<br/>
///     {{string.Join("<br/>\n///     ", decendants.Select(x => $"<see cref=\"{x}\"/>"))}}
/// </summary>
public interface I{{name}}
{
    {{propStr}}
}
#nullable restore
#pragma warning restore CS8618

""";
            Directory.CreateDirectory(Path.Combine(context.GeneratorContext.OutputDirectory, "Interfaces"));

            await File.WriteAllTextAsync(Path.Combine(context.GeneratorContext.OutputDirectory, "Interfaces", $"I{name}.g.cs"), code);

            return props.ToDictionary(x => x.Key.Value.Name, x => (x.Key, x.Value));
        }

        private readonly struct PropertyComparator : IEqualityComparer<ObjectProperty>
        {
            public static readonly PropertyComparator Instance = new();

            public bool Equals(ObjectProperty x, ObjectProperty y)
                => x.Name == y.Name && x.Cardinality == y.Cardinality;

            public int GetHashCode([DisallowNull] ObjectProperty obj)
                => obj.GetHashCode();
        }
    }
}
