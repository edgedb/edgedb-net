using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace EdgeDB.QueryBuilder.SourceGenerator
{
    [Generator]
    public class OperatorSourceGenerator : ISourceGenerator
    {
        private IDeserializer? _deserializer;

        private readonly string _paramaterNames = "abcdefghijklmnopqrstuvwxyz";

        public void Execute(GeneratorExecutionContext context)
        {
            // get the operators yaml file
            var opFile = context.AdditionalFiles.FirstOrDefault(x => x.Path.EndsWith("operators.yml"));

            if (opFile == null)
                throw new Exception("Couldn't find operators.yml");

            var content = opFile.GetText(context.CancellationToken);

            if (content == null)
                throw new Exception("File content was null"); 

            var sections = _deserializer!.Deserialize<Dictionary<string, EdgeQLOperator[]>>(content!.ToString());

            // build the EdgeQL class

            var writer = new CodeWriter();

            writer.AppendLine("using EdgeDB.Operators;");
            writer.AppendLine();
            
            using(var _ = writer.BeginScope("namespace EdgeDB"))
            {
                using(var __ = writer.BeginScope("public sealed partial class EdgeQL"))
                {
                    foreach (var section in sections)
                    {
                        writer.AppendLine($"#region {section.Key}");
                        writer.AppendLine();
                        foreach (var op in section.Value)
                        {
                            BuildSingleOperator(section.Key, op, context);

                            if (op.Functions != null && op.Functions.Any())
                            {
                                writer.AppendLine($"#region {op.Name}");
                                foreach(var func in op.Functions)
                                {
                                    writer.AppendLine($"[EquivalentOperator(typeof({op.Name}))]");
                                    writer.AppendLine($"public static {func.Return ?? op.Return} {func.Name}({string.Join(", ", func.Parameters.Select((x, i) => $"{x.Split(' ')[0]} {(x.Split(' ').Length > 1 ? x.Split(' ')[1] : _paramaterNames[i])}"))}) {{ return default!; }}");
                                }
                                writer.AppendLine("#endregion");
                                writer.AppendLine();
                            }
                        }
                        writer.AppendLine($"#endregion {section.Key}");
                        writer.AppendLine();
                    }
                }
            }

            context.AddSource("edgeql.g.cs", writer.ToString());
        }

        public void BuildSingleOperator(string section, EdgeQLOperator op, GeneratorExecutionContext context)
        {
            var writer = new CodeWriter();
            writer.AppendLine("using System.Linq.Expressions;");
            writer.AppendLine();
            
            using (var _ = writer.BeginScope("namespace EdgeDB.Operators"))
            {
                using(var __  = writer.BeginScope($"internal class {op.Name} : IEdgeQLOperator"))
                {
                    var opValue = "null";

                    if(op.Operator != null)
                    {
                        opValue = $"ExpressionType.{op.Expression}";
                    }

                    writer.AppendLine($"public ExpressionType? Operator => {opValue};");
                    writer.AppendLine($"public string EdgeQLOperator => \"{op.Operator}\";");
                }
            }

            context.AddSource($"{section}-{op.Name}.g.cs", writer.ToString());
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            _deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();
        }
    }
}
