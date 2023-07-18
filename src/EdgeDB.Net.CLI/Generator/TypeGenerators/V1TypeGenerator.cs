using EdgeDB.Binary.Codecs;
using EdgeDB.CLI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Generator.TypeGenerators
{
    internal sealed class V1TypeGeneratorContext : ITypeGeneratorContext
    {
        public required GeneratorContext GeneratorContext { get; init; }

        public string? RootName { get; set; }

        private int _subtypeCount;

        public string GetSubtypeName()
            => $"{RootName}SubType{_subtypeCount++}";

        public IEnumerable<Task> FileGenerationTasks
            => _work;

        private readonly List<Task> _work = new();

        public void QueueWork(Task task)
            => _work.Add(task);
    }

    internal sealed class V1TypeGenerator : ITypeGenerator<V1TypeGeneratorContext>
    {
        public V1TypeGeneratorContext CreateContext(GeneratorContext generatorContext)
            => new() { GeneratorContext = generatorContext };

        public ValueTask<string> GetTypeAsync(ICodec codec, GeneratorTargetInfo? target, V1TypeGeneratorContext context)
        {
            context.RootName = target is null ? $"{codec.GetHashCode()}Result" : $"{target.FileName}Result";

            return GetTypeAsync(codec, context.RootName, context);
        }

        private async ValueTask<string> GetTypeAsync(ICodec codec, string? name, V1TypeGeneratorContext context, bool nullable = false)
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
                case IWrappingCodec wrapping when codec.GetType().IsGenericType:
                    if (codec.GetType().GetGenericTypeDefinition() == typeof(ArrayCodec<>))
                        return $"{await GetTypeAsync(wrapping.InnerCodec, name, context)}[]";
                    else if (codec.GetType().GetGenericTypeDefinition() == typeof(RangeCodec<>))
                        return $"Range<{await GetTypeAsync(wrapping.InnerCodec, name, context)}>";
                    else if (codec.GetType().GetGenericTypeDefinition() == typeof(SetCodec<>))
                        return $"List<{await GetTypeAsync(wrapping.InnerCodec, name, context)}>";
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

        private async ValueTask<string> GenerateResultType(ObjectCodec obj, string? name, V1TypeGeneratorContext context)
        {
            (string, string)[] properties = new (string, string)[obj.Properties.Length];

            for(var i = 0; i != properties.Length; i++)
            {
                var property = obj.Properties[i];
                var codec = obj.InnerCodecs[i];

                var type = await GetTypeAsync(
                    codec,
                    name: null,
                    context,
                    property.Cardinality is Cardinality.AtMostOne or Cardinality.Many or Cardinality.NoResult
                );

                properties[i] = (property.Name, $"public {type} {TextUtils.ToPascalCase(property.Name)} {{ get; set; }}");

                
            }

            var tname = name ?? context.GetSubtypeName();

            await GenerateTypeCodeAsync(properties, tname, context);

            return tname;
        }

        private async static Task GenerateTypeCodeAsync((string, string)[] props, string name, V1TypeGeneratorContext context)
        {
            Directory.CreateDirectory(Path.Combine(context.GeneratorContext.OutputDirectory, "Results"));

            var code = 
$$""""
using EdgeDB;
using EdgeDB.DataTypes;

#nullable enable
#pragma warning disable CS8618 // nullablility is controlled by EdgeDB

namespace {{context.GeneratorContext.GenerationNamespace}};

[EdgeDBType]
public sealed class {{name}}
{
{{string.Join("\n", props.Select(x => $"    [EdgeDBProperty(\"{x.Item1}\")]\n    {x.Item2}\n"))}}
}

#nullable restore
#pragma warning restore CS8618
"""";
            await File.WriteAllTextAsync(Path.Combine(context.GeneratorContext.OutputDirectory, "Results", $"{name}.g.cs"), code);
        } 
    }
}
