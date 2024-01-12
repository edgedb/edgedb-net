using EdgeDB.StandardLibGenerator.Models;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace EdgeDB.StandardLibGenerator
{
    internal class OperatorGenerator
    {
        private static string OutputPath => Path.Combine(Environment.CurrentDirectory, "output", "operators"); // @"C:\Users\lynch\source\repos\EdgeDB\src\EdgeDB.Net.QueryBuilder\Grammar\Operators";

        public static Dictionary<string, ExpressionType[]> ExpressionMap = new Dictionary<string, ExpressionType[]>
        {
            {"std::+", new ExpressionType[] { ExpressionType.Add, ExpressionType.AddChecked } },
            {"std::AND", new ExpressionType[] { ExpressionType.AndAlso } },
            {"std::IF", new ExpressionType[] { ExpressionType.Conditional, } },
            {"std::-", new ExpressionType[] { ExpressionType.Subtract, ExpressionType.Negate } },
            {"std::/", new ExpressionType[] { ExpressionType.Divide } },
            {"std::>", new ExpressionType[] { ExpressionType.GreaterThan} },
            {"std::<", new ExpressionType[] { ExpressionType.LessThan} },
            {"std::>=", new ExpressionType[] { ExpressionType.GreaterThanOrEqual } },
            {"std::<=", new ExpressionType[] { ExpressionType.LessThanOrEqual } },
            {"std::%", new ExpressionType[] { ExpressionType.Modulo} },
            {"std::*", new ExpressionType[] { ExpressionType.Multiply} },
            {"std::!=", new ExpressionType[] { ExpressionType.NotEqual } },
            {"std::?!=", new ExpressionType[] { ExpressionType.NotEqual } },
            {"std::OR", new ExpressionType[] { ExpressionType.OrElse} },
            {"std::^", new ExpressionType[] { ExpressionType.Power } },
            {"std::=", new ExpressionType[] { ExpressionType.Equal} },
            {"std::?=", new ExpressionType[] { ExpressionType.Equal} },
            {"std::[]", new ExpressionType[] { ExpressionType.ArrayIndex} },
            {"std::??", new ExpressionType[] { ExpressionType.Coalesce} },

        };

        private static Dictionary<string, string> _nameConverters = new Dictionary<string, string>()
        {
            {"Ilike", "ILike" }
        };

        private static Dictionary<string, string> _aliases = new Dictionary<string, string>()
        {
            {"std::++", "std::CONCAT" }
        };

        public static async Task<List<RequiredMethodTranslator>> GenerateAsync(EdgeDBClient client, IReadOnlyCollection<Operator> operators)
        {
            var methodTranslators = new List<RequiredMethodTranslator>();

            var groups = operators.GroupBy(x => x.ReturnType!.Name);

            foreach(var group in groups)
            {
                methodTranslators.AddRange(await ProcessGroupAsync(client, group));
            }

            return methodTranslators;
        }

        private static async Task<List<RequiredMethodTranslator>> ProcessGroupAsync(EdgeDBClient client, IGrouping<string?, Operator?> group)
        {
            var methodTranslators = new List<RequiredMethodTranslator>();

            var writer = new CodeWriter();

            writer.AppendLine("#nullable restore");
            writer.AppendLine("using EdgeDB;");
            writer.AppendLine("using EdgeDB.DataTypes;");
            writer.AppendLine("using System.Runtime.CompilerServices;");
            writer.AppendLine("using System.Linq.Expressions;");
            writer.AppendLine();

            var clsName = INamingStrategy.PascalNamingStrategy.Convert(group.Key!.Replace("::", " "));

            using(_ = writer.BeginScope("namespace EdgeDB"))
            using (_ = writer.BeginScope($"internal partial class Grammar"))
            {
                List<string> generatedOps = new();

                foreach(var op in group)
                {
                    var opName = _aliases.TryGetValue(op!.Name!, out var alias) ? alias : op!.Name!;

                    if (generatedOps.Contains($"{opName}{op.Parameters!.Length}"))
                    {
                        // shortcut for method translators
                        if (Regex.IsMatch(opName!, @".+::(?>\w| )+$"))
                        {
                            methodTranslators.Add(new RequiredMethodTranslator
                            {
                                Expression = BuildExpression(op!),
                                Group = group.Key,
                                Parameters = op.Parameters,
                                Result = op.ReturnType,
                                TargetName = FormatName(INamingStrategy.PascalNamingStrategy.Convert(op.Name!.Split("::")[1])),
                                Modifier = op.ReturnTypeModifier
                            });
                        }

                        continue;
                    }

                    var expression = BuildExpression(op!);

                    if (ExpressionMap.TryGetValue(opName, out var expressions))
                    {
                        writer.AppendLine($"[EquivalentExpression({string.Join(", ", expressions.Select(x => $"ExpressionType.{x}"))})]");
                    }
                    else if (Regex.IsMatch(opName, @".+::(?>\w| )+$"))
                    {
                        methodTranslators.Add(new RequiredMethodTranslator
                        {
                            Expression = expression,
                            Group = group.Key,
                            Parameters = op.Parameters,
                            Result = op.ReturnType,
                            TargetName = FormatName(INamingStrategy.PascalNamingStrategy.Convert(opName.Split("::")[1])),
                            Modifier = op.ReturnTypeModifier
                        });
                    }
                    else
                    {
                        Console.WriteLine($"Skipping op {op.Name}");
                        continue;
                    }

                    writer.AppendLine($"[EdgeQLOp(\"{op!.Name}\")]");

                    var name = Math.Abs(HashCode.Combine(opName, op.Parameters));

                    using (_ = writer.BeginScope($"public static void Op_{name}(QueryStringWriter writer, {string.Join(", ", op.Parameters!.Select(x => $"QueryStringWriter.Proxy {x.Name}Param"))})"))
                    {
                        writer.AppendLine($"{expression};");
                    }

                    generatedOps.Add($"{op.Name}{op.Parameters.Length}");
                }
            }

            var outputPath = Path.Join(OutputPath, $"Grammar.{clsName}.g.cs");
            Directory.CreateDirectory(OutputPath);
            await File.WriteAllTextAsync(outputPath, writer.ToString());

            return methodTranslators;
        }

        private static string FormatName(string name)
        {
            var keys = _nameConverters.Keys.Where(x => name.Contains(x));

            foreach(var key in keys)
            {
                name = name.Replace(key, _nameConverters[key]);
            }

            return name;
        }

        private static string BuildExpression(Operator op)
        {
            var operation = op.Name!.Split("::")[1];

            switch (op.OperatorKind)
            {
                case OperatorKind.Infix:
                    {
                        if (op.Parameters!.Length != 2)
                            throw new ArgumentException("Expected 2 parameters for Infix");

                        if (op.Name == "std::[]")
                        {
                            return $"writer.Append({op.Parameters[0].Name + "Param"}).Wrapped({op.Parameters[1].Name + "Param"}, \"[]\")";
                        }

                        return $"writer.Append({op.Parameters[0].Name + "Param"}).Wrapped(\"{operation}\", \"  \").Append({op.Parameters[1].Name + "Param"})";
                    }
                case OperatorKind.Postfix:
                    {
                        if (op.Parameters!.Length != 1)
                            throw new ArgumentException("Expected 1 parameter for Postfix");

                        return $"writer.Append({op.Parameters[0].Name + "Param"}).Append(\"{operation}\")";
                    }
                case OperatorKind.Prefix:
                    {
                        if (op.Parameters!.Length != 1)
                            throw new ArgumentException("Expected 1 parameter for Prefix");

                        return $"writer.Append(\"{operation}\").Append({op.Parameters[0].Name + "Param"})";
                    }
                case OperatorKind.Ternary:
                    {
                        if (op.Parameters!.Length != 3)
                            throw new ArgumentException("Expected 3 parameters for Ternary");

                        return $"writer.Append({op.Parameters[0].Name + "Param"}).Append(\" IF \").Append({op.Parameters[1].Name + "Param"}).Append(\" ELSE \").Append({op.Parameters[2].Name + "Param"})";
                    }
                default:
                    throw new Exception("Unknown operator kind " + op.OperatorKind);
            }
        }
    }
}
