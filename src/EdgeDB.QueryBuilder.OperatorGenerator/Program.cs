// This project generates the edgeql operators as functions within the edgeql class and operators folder

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using EdgeDB.QueryBuilder.OperatorGenerator;
using System.Text.RegularExpressions;

const string OperatorsOutputDir = "../../../../EdgeDB.QueryBuilder/Operators";
const string EdgeQLOutput = "../../../../EdgeDB.QueryBuilder";
const string OperatorDefinitionFile = "../../../operators.yml";
const string ParamaterNames = "abcdefghijklmnopqrstuvwxyz";

var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

var content = File.ReadAllText(OperatorDefinitionFile);

var sections = deserializer!.Deserialize<Dictionary<string, EdgeQLOperator[]>>(content!.ToString());

var writer = new CodeWriter();

writer.AppendLine("#nullable restore");
writer.AppendLine("#pragma warning disable");
writer.AppendLine("using EdgeDB.Operators;");
writer.AppendLine("using EdgeDB.DataTypes;");
writer.AppendLine("using System.Numerics;");
writer.AppendLine();

using (var _ = writer.BeginScope("namespace EdgeDB"))
{
    using (var __ = writer.BeginScope("public sealed partial class EdgeQL"))
    {
        List<(string CSName, string OperatorName)> propertyMap = new();
        List<(string CSName, string OperatorName)> functionMap = new();

        foreach (var section in sections)
        {
            if (section.Key == "enums")
            {
                WriteEnums(section.Value);
                continue;
            }

            writer.AppendLine($"#region {section.Key}");
            writer.AppendLine();
            foreach (var op in section.Value)
            {
                var cleanedName = Regex.Replace(op.Name!, @"(<.*?>)", x => "");
                var operatorName = $"{FirstCharToUpper(section.Key)}{cleanedName}";
                BuildSingleOperator(section.Key, op);

                if (op.FunctionMap.Any())
                {
                    functionMap.AddRange(op.FunctionMap.Select(x => (x, operatorName)));
                }

                if (op.PropertyMap.Any())
                {
                    propertyMap.AddRange(op.PropertyMap.Select(x => (x, operatorName)));
                }

                if (op.Functions != null && op.Functions.Any())
                {
                    writer.AppendLine($"#region {op.Name}");
                    foreach (var rootFunc in op.Functions)
                    {
                        var funcs = GenerateFunctions(rootFunc);
                        if (funcs.Count == 0)
                            funcs.Add(rootFunc);

                        foreach(var func in funcs)
                        {
                            writer.AppendLine($"[EquivalentOperator(typeof(EdgeDB.Operators.{operatorName}))]");
                            foreach(var map in op.ParameterMap)
                            {
                                var split = map.Split(":");

                                writer.AppendLine($"[ParameterMap({split[0]}, \"{split[1]}\")]");
                            }
                            writer.AppendLine($"public static {func.Return ?? op.Return} {func.Name ?? op.Name}({string.Join(", ", func.Parameters.Select((x, i) => $"{x.Split(' ')[0]} {(x.Split(' ').Length > 1 ? string.Join(" ", x.Split(' ').Skip(1)) : ParamaterNames[i])}"))}){(func.Filter != null ? $" {func.Filter}" : "")} {{ return default!; }}");
                        }

                    }
                    writer.AppendLine("#endregion");
                    writer.AppendLine();
                }
            }
            writer.AppendLine($"#endregion {section.Key}");
            writer.AppendLine();
        }

        // write the property and function map
        if (propertyMap.Any())
        {
            using(var ___ = writer.BeginScope("internal static Dictionary<string, IEdgeQLOperator> PropertyOperators = new()"))
            {
                propertyMap.ForEach(x => writer.AppendLine($"{{ \"{x.CSName}\", new {x.OperatorName}()}}"));
            }
            writer.AppendLine(";");
        }

        if (functionMap.Any())
        {
            using (var ___ = writer.BeginScope("internal static Dictionary<string, IEdgeQLOperator> FunctionOperators = new()"))
            {
                functionMap.ForEach(x => writer.AppendLine($"{{ \"{x.CSName}\", new {x.OperatorName}()}},"));
                //writer.AppendLine(string.Join(",\n", functionMap.Select(x => $"{new string(' ', writer.IndentLevel)}{{ \"{x.CSName}\", new {x.OperatorName}()}}")));
            }
            writer.Append(";\n");
        }
    }
}

writer.AppendLine("#nullable restore");
writer.AppendLine("#pragma warning restore");

void WriteEnums(EdgeQLOperator[] enums)
{
    Directory.CreateDirectory(Path.Combine(OperatorsOutputDir, "Enums"));

    foreach (var en in enums)
    {
        var writer = new CodeWriter();

        using(var _ = writer.BeginScope("namespace EdgeDB"))
        {
            if (en.SerializeMethod != null)
                writer.AppendLine($"[EnumSerializer(SerializationMethod.{en.SerializeMethod})]");
            writer.AppendLine($"public enum {en.Name}");
            using (var __ = writer.BeginScope())
            {
                writer.Append(string.Join(",\n", en.Elements.Select(x => $"{new string(' ', writer.IndentLevel)}{x}")));
                writer.Append("\n");
            }
        }

        File.WriteAllText(Path.Combine(OperatorsOutputDir, "Enums", $"{en.Name}.g.cs"), writer.ToString());
    }
}

void BuildSingleOperator(string section, EdgeQLOperator op)
{
    var writer = new CodeWriter();
    writer.AppendLine("using System.Linq.Expressions;");
    writer.AppendLine();

    var cleanedName = Regex.Replace(op.Name, @"(<.*?>)", x => "");

    using (var _ = writer.BeginScope("namespace EdgeDB.Operators"))
    {
        using (var __ = writer.BeginScope($"internal class {FirstCharToUpper(section)}{cleanedName} : IEdgeQLOperator"))
        {
            var opValue = "null";

            if (!string.IsNullOrEmpty(op.Expression))
            {
                opValue = $"ExpressionType.{op.Expression}";
            }

            writer.AppendLine($"public ExpressionType? Operator => {opValue};");
            writer.AppendLine($"public string EdgeQLOperator => \"{op.Operator}\";");
        }
    }

    Directory.CreateDirectory(Path.Combine(OperatorsOutputDir, $"{FirstCharToUpper(section)}"));
    File.WriteAllText(Path.Combine(OperatorsOutputDir, $"{FirstCharToUpper(section)}", $"{cleanedName}.g.cs"), writer.ToString());
}

string FirstCharToUpper(string input) =>
input switch
{
    null => throw new ArgumentNullException(nameof(input)),
    "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
    _ => input[0].ToString().ToUpper() + input.Substring(1)
};

List<string> Replace(string r, int i, List<string> arr)
{
    var l = new List<string>(arr);

    l[i] = r;

    return l;
}

// evaluates to (n^n/2) / 2 functions

List<EdgeQLFunction> GenerateFunctions(EdgeQLFunction func)
{
    var ret = new List<EdgeQLFunction>();

    for(int i = 0; i != func.Parameters.Count; i++)
    {
        var param = func.Parameters[i];
        if(param == "anyint")
        {
            ret.Add(new EdgeQLFunction { Name = func.Name, Return = func.Return ?? "short", Parameters = Replace("short", i, func.Parameters)});
            ret.Add(new EdgeQLFunction { Name = func.Name, Return = func.Return ?? "int", Parameters = Replace("int", i, func.Parameters)});
            ret.Add(new EdgeQLFunction { Name = func.Name, Return = func.Return ?? "long", Parameters = Replace("long", i, func.Parameters)});

            ret.AddRange(ret.Select(x => GenerateFunctions(x)).SelectMany(x => x));
        }
        if(param == "anyfloat")
        {
            ret.Add(new EdgeQLFunction { Name = func.Name, Return = func.Return ?? "float", Parameters = Replace("float", i, func.Parameters) });
            ret.Add(new EdgeQLFunction { Name = func.Name, Return = func.Return ?? "double", Parameters = Replace("double", i, func.Parameters) });
            ret.AddRange(ret.Select(x => GenerateFunctions(x)).SelectMany(x => x));
        }
        if (param == "anyreal")
        {
            ret.Add(new EdgeQLFunction { Name = func.Name, Return = func.Return ?? "short", Parameters = Replace("short", i, func.Parameters) });
            ret.Add(new EdgeQLFunction { Name = func.Name, Return = func.Return ?? "int", Parameters = Replace("int", i, func.Parameters) });
            ret.Add(new EdgeQLFunction { Name = func.Name, Return = func.Return ?? "long", Parameters = Replace("long", i, func.Parameters) });
            ret.Add(new EdgeQLFunction { Name = func.Name, Return = func.Return ?? "float", Parameters = Replace("float", i, func.Parameters) });
            ret.Add(new EdgeQLFunction { Name = func.Name, Return = func.Return ?? "double", Parameters = Replace("double", i, func.Parameters) });
            ret.Add(new EdgeQLFunction { Name = func.Name, Return = func.Return ?? "decimal", Parameters = Replace("decimal", i, func.Parameters) });
            ret.AddRange(ret.ToArray().Select(x => GenerateFunctions(x)).SelectMany(x => x));
        }
    }

    ret.RemoveAll(x => x.Parameters.Any(x => x == "anyreal" || x == "anyfloat" || x == "anyint"));

    return ret;
}


File.WriteAllText(Path.Combine(EdgeQLOutput, "EdgeQL.g.cs"), writer.ToString());