using EdgeDB.Binary.Codecs;
using EdgeDB.CLI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Generator.TypeGenerators
{
    internal sealed class V1TypeGenerator : ITypeGenerator
    {
        private readonly List<GeneratedFileInfo> _generatedFiles;

        private int _count;

        public V1TypeGenerator()
        {
            _generatedFiles = new();
        }

        public IEnumerable<GeneratedFileInfo> GetGeneratedFiles()
            => _generatedFiles;

        public void RemoveGeneratedReferences(IEnumerable<GeneratedFileInfo> references)
        {
            foreach(var item in references)
            {
                _generatedFiles.Remove(item);
            }
        }

        private void RegisterFile(string path, string? workingFile)
        {
            var existing = _generatedFiles.FirstOrDefault(x => x.GeneratedPath == path);

            if (existing is not null && workingFile is not null)
                existing.EdgeQLReferences.Add(workingFile);

            if(existing is null)
            {
                _generatedFiles.Add(new(path, workingFile));
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
            var rootName = target is null ? $"{codec.GetHashCode()}Result" : $"{target.FileName}Result";

            return GetTypeAsync(codec, rootName, target?.Path, context);
        }

        private async ValueTask<string> GetTypeAsync(ICodec codec, string? name, string? workingFile, GeneratorContext context, bool nullable = false)
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
                case IWrappingCodec wrapping when codec.GetType().IsGenericType:
                    if (codec.GetType().GetGenericTypeDefinition() == typeof(ArrayCodec<>))
                        return $"{await GetTypeAsync(wrapping.InnerCodec, name, workingFile, context)}[]";
                    else if (codec.GetType().GetGenericTypeDefinition() == typeof(RangeCodec<>))
                        return $"Range<{await GetTypeAsync(wrapping.InnerCodec, name, workingFile, context)}>";
                    else if (codec.GetType().GetGenericTypeDefinition() == typeof(SetCodec<>))
                        return $"List<{await GetTypeAsync(wrapping.InnerCodec, name, workingFile, context)}>";
                    else
                        throw new EdgeDBException($"Unknown wrapping codec {wrapping}");
                case IScalarCodec scalar:
                    var sc = scalar.ConverterType.Name;
                    if (nullable)
                        sc += "?";

                    return sc;
                case NullCodec:
                    return "object?";
                default:
                    throw new InvalidOperationException($"Unknown type parse path for codec {codec}");
            }
        }

        private async ValueTask<string> GenerateResultType(ObjectCodec obj, string? name, string? workingFile, GeneratorContext context)
        {
            var properties = new (string, string)[obj.Properties.Length];

            for(var i = 0; i != properties.Length; i++)
            {
                var property = obj.Properties[i];
                var codec = obj.InnerCodecs[i];

                var type = await GetTypeAsync(
                    codec,
                    name: null,
                    workingFile,
                    context,
                    property.Cardinality is Cardinality.AtMostOne or Cardinality.Many or Cardinality.NoResult
                );

                properties[i] = (property.Name, $"public {type} {TextUtils.ToPascalCase(property.Name)} {{ get; set; }}");
            }

            var tname = name ?? GetSubtypeName();

            await GenerateTypeCodeAsync(properties, tname, workingFile, context);

            return tname;
        }

        private async Task GenerateTypeCodeAsync((string, string)[] props, string name, string? workingFile, GeneratorContext context)
        {
            Directory.CreateDirectory(Path.Combine(context.OutputDirectory, "Results"));

            var code = 
$$""""
using EdgeDB;
using EdgeDB.DataTypes;
using System;

#nullable enable
#pragma warning disable CS8618 // nullablility is controlled by EdgeDB

namespace {{context.GenerationNamespace}};

[EdgeDBType]
public sealed class {{name}}
{
{{string.Join("\n", props.Select(x => $"    [EdgeDBProperty(\"{x.Item1}\")]\n    {x.Item2}\n"))}}
}

#nullable restore
#pragma warning restore CS8618
"""";
            var path = Path.Combine(context.OutputDirectory, "Results", $"{name}.g.cs");

            RegisterFile(path, workingFile);

            await File.WriteAllTextAsync(path, code);
        }

        public ValueTask PostProcessAsync(GeneratorContext _) => ValueTask.CompletedTask;
    }
}
