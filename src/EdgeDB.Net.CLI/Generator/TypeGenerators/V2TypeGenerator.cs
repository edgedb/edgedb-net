using EdgeDB.Binary.Codecs;
using EdgeDB.CLI.Utils;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Generator.TypeGenerators
{
    internal abstract class ObjectGenerationInfo
    {
        public abstract string GeneratedFilePath { get; }

        public string CleanTypeName
            => TypeName.Contains("::") ? TypeName.Split("::")[1] : TypeName;

        public ObjectGenerationInfo? Parent { get; set; }

        public string TypeName { get; }

        public Dictionary<string, string> PropertyTypeMap { get; }

        public string? FilePath { get; set; }

        public ObjectGenerationInfo(string typename, Dictionary<string, string> propertyMap, ObjectGenerationInfo? parent = null)
        {
            TypeName = typename;
            PropertyTypeMap = propertyMap;
            Parent = parent;
        }
    }

    internal sealed class ClassGenerationInfo : ObjectGenerationInfo
    {
        public override string GeneratedFilePath
            => Path.Combine("Results", $"{CleanTypeName}.g.cs");

        public string? EdgeQLSourceFile { get; }
        public ObjectCodec Codec { get; }

        public ClassGenerationInfo(
            string typename, ObjectCodec codec, Dictionary<string, string> propertyTypeMap,
            string? edgeqlSourceFile,
            ObjectGenerationInfo? parent = null)
            : base(typename, propertyTypeMap, parent)
        {
            EdgeQLSourceFile = edgeqlSourceFile;
            Codec = codec;
        }

        public override string ToString() => $"class {CleanTypeName} ({TypeName})";
    }

    internal sealed class InterfaceGenerationInfo : ObjectGenerationInfo
    {
        public override string GeneratedFilePath
            => Path.Combine("Interfaces", $"{TypeName}.g.cs");

        public InterfaceGenerationInfo(
            string typename,
            Dictionary<string, string> propertyMap,
            ObjectGenerationInfo? parent = null)
        : base(typename, propertyMap, parent)
        {
        }

        public override string ToString() => $"interface {CleanTypeName}";
    }

    internal sealed class V2TypeGenerator : ITypeGenerator
    {
        private readonly Dictionary<Guid, List<ClassGenerationInfo>> _resultShapes;
        private readonly List<ObjectGenerationInfo> _generatedTypes;

        private int _count;

        public V2TypeGenerator()
        {
            _resultShapes = new();
            _generatedTypes = new();
        }

        public IEnumerable<GeneratedFileInfo> GetGeneratedFiles()
            => _generatedTypes.Select(x =>
            {
                return x switch
                {
                    ClassGenerationInfo cls => new GeneratedFileInfo(cls.FilePath!, cls.EdgeQLSourceFile),
                    InterfaceGenerationInfo iface => new GeneratedFileInfo(iface.FilePath!, null),
                    _ => throw new InvalidOperationException($"Unknown target info {x}")
                };
            });

        public void RemoveGeneratedReferences(IEnumerable<GeneratedFileInfo> references)
        {
            foreach (var item in references)
            {
                _generatedTypes.RemoveAll(x => x.FilePath == item.GeneratedPath);
            }
        }

        private string GetSubtypeName(string? name = null)
        {
            return name is not null
                ? $"{name}SubType{_count++}"
                : $"GenericSubType{_count++}";
        }

        public ValueTask<string> GetTypeAsync(ICodec codec, GeneratorTargetInfo? target, GeneratorContext context)
        {
            var rootName = target is null ? $"{codec.GetHashCode()}Result" : target.FileName;

            return GetTypeAsync(codec, rootName, target?.Path, context);
        }

        private async ValueTask<string> GetTypeAsync(ICodec codec, string? name, string? workingFile, GeneratorContext context)
        {
            switch (codec)
            {
                case SparceObjectCodec:
                    throw new InvalidOperationException("Cannot parse sparce object codec");
                case ObjectCodec obj:
                    return await GenerateResultType(obj, name, workingFile, context);
                case TupleCodec tuple:
                    var elements = new string[tuple.InnerCodecs.Length];
                    for (var i = 0; i != elements.Length; i++)
                        elements[i] = await GetTypeAsync(codec, $"{name ?? GetSubtypeName()}TupleElement{i}", workingFile, context);
                    return $"({string.Join(", ", elements)})";
                case CompilableWrappingCodec:
                case IWrappingCodec when codec.GetType().IsGenericType:
                    var inner = codec is CompilableWrappingCodec c ? c.InnerCodec : codec is IWrappingCodec w ? w.InnerCodec : throw new Exception("This isn't possible");
                    if (codec.GetType().GetGenericTypeDefinition() == typeof(ArrayCodec<>))
                        return $"{await GetTypeAsync(inner, name, workingFile, context)}[]";
                    else if (codec.GetType().GetGenericTypeDefinition() == typeof(RangeCodec<>))
                        return $"Range<{await GetTypeAsync(inner, name, workingFile, context)}>";
                    else if (codec.GetType().GetGenericTypeDefinition() == typeof(SetCodec<>))
                        return $"List<{await GetTypeAsync(inner, name, workingFile, context)}>";
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

        private async ValueTask<string> GenerateResultType(ObjectCodec obj, string? name, string? workingFile, GeneratorContext context)
        {
            if (name is not null)
                name = $"{name}{TextUtils.NameWithoutModule(obj.Metadata?.SchemaName) ?? "Result"}";
            else
                name = $"{GetSubtypeName(name)}{TextUtils.CleanTypeName(obj.Metadata?.SchemaName) ?? "Result"}";

            var map = new Dictionary<string, string>();

            for(var i = 0; i != obj.Properties.Length; i++)
            {
                var propType = await GetTypeAsync(obj.InnerCodecs[i], name: null, workingFile, context);
                map.Add(obj.Properties[i].Name, propType);
            }

            var info = new ClassGenerationInfo(name, obj, map, workingFile);
            _generatedTypes.Add(info);

            info.FilePath = Path.Combine(context.OutputDirectory, info.GeneratedFilePath);

            _resultShapes.TryAdd(obj.Id, new());
            _resultShapes[obj.Id].Add(info);


            return info.CleanTypeName;
        }

        public async ValueTask PostProcessAsync(GeneratorContext context)
        {
            var groups = _resultShapes.Values
                .SelectMany(x => x)
                .GroupBy(x => x.Codec.Metadata is null ? x.Codec.Id.ToString() : x.Codec.Metadata.SchemaName ?? GetSubtypeName(x.TypeName));

            foreach(var group in groups)
            {
                var iname = group.Key.Contains("::") ? group.Key.Split("::")[1] : group.Key;

                var map = await GenerateInterfaceAsync(
                    group.Key,
                    iname,
                    group,
                    context,
                    group.Select(x => x.CleanTypeName),
                    prop => group.Where(x => x.PropertyTypeMap.ContainsKey(prop)).DistinctBy(x => x.CleanTypeName).Select(x => x.CleanTypeName)
                );

                foreach (var type in group)
                {
                    var propsMap = type.PropertyTypeMap.Select(x =>
                    {
                        var property = type.Codec.Properties.First(y => y.Name == x.Key);

                        return $"[EdgeDBProperty(\"{property.Name}\")]\n    public {CodeGenerator.ApplyCardinality(x.Value, property.Cardinality)} {TextUtils.ToPascalCase(x.Key)} {{ get; set; }}";
                    });

                    var ifaceStrMap = string.Empty;

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

                    ifaceStrMap = ifaceMap.Any()
                        ? $"// I{iname}\n    {string.Join("\n    ", ifaceMap)}"
                        : string.Empty;

                    var path = Path.Combine(context.OutputDirectory, "Results", $"{type.CleanTypeName}.g.cs");

                    type.FilePath = path;

                    var code =
$$"""
// source: {{(type.EdgeQLSourceFile is not null ? Path.GetRelativePath(path, type.EdgeQLSourceFile) : "unknown")}}
// parent: {{Path.Combine("..", "..", "Interfaces", $"I{iname}.g.cs")}}
using EdgeDB;
using EdgeDB.DataTypes;
using System;

namespace {{context.GenerationNamespace}};

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
                    
                    await File.WriteAllTextAsync(path, code);
                }
            }
        }

        private async Task<Dictionary<string, ((string Type, ObjectProperty Value), bool)>> GenerateInterfaceAsync(
            string schemaName, string name,
            IEnumerable<ClassGenerationInfo> subtypes, GeneratorContext context,
            IEnumerable<string> decendants, Func<string, IEnumerable<string>> getAvailability)
        {
            var props = subtypes.Select(x =>
                x.PropertyTypeMap.Select(y =>
                    (TypeName: y.Value, Property: x.Codec.Properties.First(z => z.Name == y.Key))
                ).ToArray())
                .SelectMany(x => x)
                .DistinctBy(x => x.Property.Name)
                .ToDictionary(x => x, x => subtypes.All(y => y.Codec.Properties.Contains(x.Property, PropertyComparator.Instance)));

            var interfaceInfo = new InterfaceGenerationInfo($"I{name}", props.ToDictionary(x => x.Key.Property.Name, x =>
            {
                var type = CodeGenerator.ApplyCardinality(x.Key.TypeName, x.Key.Property.Cardinality);

                if (!x.Value)
                    type = $"Optional<{type}>";

                return type;
            }));

            foreach(var subtype in subtypes)
            {
                subtype.Parent = interfaceInfo;
            }

            _generatedTypes.Add(interfaceInfo);

            var propStr =
                string.Join("\n    ", props.Select(x =>
                {
                    var type = CodeGenerator.ApplyCardinality(x.Key.TypeName, x.Key.Property.Cardinality);

                    if (!x.Value)
                        type = $"Optional<{type}>";

                    var summary = $"/// <summary>\n    ///     The <c>{x.Key.Property.Name}</c> property available on ";

                    if (x.Value)
                    {
                        summary += "all descendants of this interface.\n";
                    }
                    else
                    {
                        var availableOn = getAvailability(x.Key.Property.Name);
                        summary += $"the following types:<br/>\n    ///     {string.Join("<br/>\n    ///         ", availableOn.Select(x => $"<see cref=\"{x}\"/>"))}\n";
                    }

                    summary += "    /// </summary>\n    ";

                    return $"{summary}{type} {TextUtils.ToPascalCase(x.Key.Property.Name)} {{ get; }}\n";
                }));

            var code =
$$"""
// descendants: {{string.Join("|", subtypes.Select(x => Path.Combine("..", "..", "Results", $"{x.CleanTypeName}.g.cs")))}}
// {{schemaName}} abstraction
using EdgeDB;
using EdgeDB.DataTypes;
using System;

#nullable enable
#pragma warning disable CS8618 // nullablility is controlled by EdgeDB

namespace {{context.GenerationNamespace}};

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
            Directory.CreateDirectory(Path.Combine(context.OutputDirectory, "Interfaces"));

            var path = Path.Combine(context.OutputDirectory, "Interfaces", $"I{name}.g.cs");

            interfaceInfo.FilePath = path;

            await File.WriteAllTextAsync(path, code);

            return props.ToDictionary(x => x.Key.Property.Name, x => (x.Key, x.Value));
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
