using EdgeDB.DataTypes;
using EdgeDB.StandardLibGenerator.Models;
using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace EdgeDB.StandardLibGenerator
{
    internal class FunctionGenerator
    {
        private const string STDLIB_PATH = @"C:\Users\lynch\source\repos\EdgeDB\src\EdgeDB.Net.QueryBuilder\stdlib";
        private const string OUTPUT_PATH = @"C:\Users\lynch\source\repos\EdgeDB\src\EdgeDB.Net.QueryBuilder\Translators\Methods\Generated";
        private static readonly TextInfo _textInfo = new CultureInfo("en-US").TextInfo;
        private static readonly Regex _groupRegex = new(@"(.+?)<.+?>");
        private static readonly List<string> _generatedPublicFuncs = new();
        private static CodeWriter? _edgeqlClassWriter;
        private static EdgeDBClient? _client;
        private static readonly Dictionary<string, string> _keywords = new()
        {
            {"base", "@base" },
            {"default", "@default" },
            {"new", "@new" }
        };

        public static async ValueTask GenerateAsync(CodeWriter eqlWriter, EdgeDBClient client, IReadOnlyCollection<Function> functions)
        {
            _client = client;
            _edgeqlClassWriter = eqlWriter;
            if (!Directory.Exists(OUTPUT_PATH))
                Directory.CreateDirectory(OUTPUT_PATH);
            
            if (!Directory.Exists(STDLIB_PATH))
                Directory.CreateDirectory(STDLIB_PATH);

            try
            {
                var grouped = functions.GroupBy(x =>
                {
                    var m = _groupRegex.Match(x.ReturnType!.Name!);
                    return m.Success ? m.Groups[1].Value : x.ReturnType.Name;

                });
                foreach (var item in grouped)
                {
                    await ProcessGroup(item.Key!, item);
                }
            }
            catch(Exception x)
            {

            }
        }

        private class ParsedParameter
        {
            public string? Name { get; init; }
            public string? Type { get; init; }
            public string[] Generics { get; set; } = Array.Empty<string>();
            public List<string> GenericConditions { get; set; } = new();
            public string? DefaultValue { get; set; } = "{}";
        }

        private static async ValueTask ProcessGroup(string groupType, IEnumerable<Function> funcs)
        {
            var writer = new CodeWriter();

            var edgedbType = funcs.FirstOrDefault(x => x.ReturnType!.Name! == groupType)?.ReturnType!;
            var translatorType = TypeUtils.TryGetType(groupType, out var tInfo) ? await BuildType(tInfo, TypeModifier.SingletonType, true) : groupType switch
            {
                "tuple" => typeof(ITuple).Name,
                "array" => typeof(Array).Name,
                "set" => typeof(IEnumerable).Name,
                "range" => "IRange",
                _  => groupType.Contains("::") ? await BuildType(new(groupType, null), TypeModifier.SingletonType, true) : throw new Exception($"Failed to find matching type for {groupType}")
            };

            writer.AppendLine("using EdgeDB;");
            writer.AppendLine("using EdgeDB.DataTypes;");
            writer.AppendLine("using System.Runtime.CompilerServices;");
            writer.AppendLine();

            using (var namespaceScope = writer.BeginScope("namespace EdgeDB.Translators"))
            using (var classScope = writer.BeginScope($"internal partial class {_textInfo.ToTitleCase(groupType.Replace("::", " ")).Replace(" ", "")} : MethodTranslator<{translatorType}>"))
            {
                foreach (var func in funcs)
                {
                    try
                    {
                        var funcName = _textInfo.ToTitleCase(func.Name!.Split("::")[1].Replace("_", " ")).Replace(" ", "");
                        
                        if (!TypeUtils.TryGetType(func.ReturnType!.Name!, out var returnTypeInfo))
                            throw new Exception($"Faield to get type {groupType}");

                        var dotnetReturnType = await ParseParameter("result", returnTypeInfo, func.ReturnType, func.ReturnTypeModifier);

                        var parameters = func.Parameters!.Select<Parameter, (Parameter Parameter, TypeNode Node)?>(x =>
                        {
                            if (!TypeUtils.TryGetType(x.Type!.Name!, out var info))
                                return null;

                            return (x, info);
                        });

                        if (parameters.Any(x => !x.HasValue))
                            throw new Exception("No parameter matches found");

                        ParsedParameter[] parsedParameters = new ParsedParameter[parameters.Count()];

                        for(int i = 0; i != parsedParameters.Length; i++)
                        {
                            var x = parameters.ElementAt(i);
                            var name = x.Value.Parameter.Name;
                            parsedParameters[i] = await ParseParameter(name, x.Value.Node, x.Value.Parameter.Type!, x.Value.Parameter.TypeModifier, i);
                            if (!string.IsNullOrEmpty(x.Value.Parameter.Default) && x.Value.Parameter.Default != "{}")
                                parsedParameters[i].DefaultValue = await ParseDefaultAsync(x.Value.Parameter.Default);
                        }

                        var parameterGenerics = parsedParameters.Where(x => x.Generics.Any()).SelectMany(x => x.Generics);
                    
                        var strongMappedParameters = string.Join(", ", parsedParameters.Select((x, i) =>
                        {
                            var t = $"{x.Type} {(_keywords.TryGetValue(x.Name!, out var p) ? p : x.Name)}";
                            var param = parameters.ElementAt(i);
                            if (!string.IsNullOrEmpty(x.DefaultValue))
                            {
                                t += " = " + x.DefaultValue switch
                                {
                                    "{}" => x.Generics.Any() || (param.Value.Node.DotnetType?.IsValueType ?? false) ? "default" : "null",
                                    _ => x.DefaultValue
                                };
                            }

                            return t;
                        }));
                        
                        var parsedMappedParameters = string.Join(", ", parameters.Select(x => $"string? {x!.Value.Parameter.Name}Param"));

                        writer.AppendLine($"[MethodName(EdgeQL.{funcName})]");
                        writer.AppendLine($"public string {funcName}({parsedMappedParameters})");

                        using(var methodScope = writer.BeginScope())
                        {
                            var methodBody = $"return $\"{func.Name}(";

                            string[] parsedParams = new string[func.Parameters.Length];

                            for(int i = 0; i != parsedParams.Length; i++)
                            {
                                var param = func.Parameters[i];

                                var value = "";
                                if (param.TypeModifier != TypeModifier.OptionalType)
                                    value = $"{{{param.Name}Param}}";
                                else
                                    value = $"{{({param.Name}Param is not null ? \"{param.Name}Param, \" : \"\")}}";

                                if (param.Kind == ParameterKind.NamedOnlyParam)
                                    value = $"{param.Name} := {{{param.Name}Param}}";

                                parsedParams[i] = value;
                            }

                            methodBody += string.Join(", ", parsedParams) + ")\";";

                            writer.AppendLine(methodBody);
                        }
                        writer.AppendLine();

                        var formattedGenerics = string.Join(", ", dotnetReturnType.Generics.Concat(parsedParameters.Where(x => x.Generics!.Any()).SelectMany(x => x.Generics!)).Distinct());

                        var genKey = $"{(dotnetReturnType.Generics.Any() ? "`1" : dotnetReturnType.Type)}{funcName}{(formattedGenerics.Any() ? $"<`{formattedGenerics.Count()}>" : "")}({string.Join(", ", parsedParameters.Select(x => x.Generics.Any() ? "`1" : x.Type))})";
                        if(!_generatedPublicFuncs.Contains(genKey))
                        {
                            _edgeqlClassWriter!.AppendLine($"public static {dotnetReturnType.Type} {funcName}{(formattedGenerics.Any() ? $"<{formattedGenerics}>" : "")}({strongMappedParameters})");
                            foreach(var c in parsedParameters.Where(x => x.GenericConditions.Any()).SelectMany(x => x.GenericConditions).Concat(dotnetReturnType.GenericConditions).Distinct())
                            {
                                _edgeqlClassWriter.AppendLine($"    {c}");
                            }
                            _edgeqlClassWriter.AppendLine("    => default!;");
                            _generatedPublicFuncs.Add(genKey);
                        }
                    }
                    catch(Exception x)
                    {
                        Console.WriteLine(x);
                    }
                }
            }

            try
            {
                File.WriteAllText(Path.Combine(OUTPUT_PATH, $"{_textInfo.ToTitleCase(groupType).Replace(":", "")}.g.cs"), writer.ToString());
            }
            catch(Exception x)
            {

            }
        }

        private static async ValueTask<ParsedParameter> ParseParameter(string? name, TypeNode node, Models.Type type, TypeModifier? modifier, int index = 0, int subIndex = 0)
        {
            if (node.IsGeneric)
            {
                var tname = $"T{_textInfo.ToTitleCase(Regex.Match(node.EdgeDBName, @"(?>.+?::|^)(.*?)$").Groups[1].Value.Replace("any", ""))}";
                var tModified = tname;
                if (modifier.HasValue)
                {
                    switch (modifier.Value)
                    {
                        case TypeModifier.OptionalType:
                            tModified += "?";
                            break;
                        case TypeModifier.SetOfType:
                            tModified = $"IEnumerable<{tname}>";
                            break;
                        default:
                            break;
                    }
                }
                return new ParsedParameter() { Name = name, Generics = new string[] { tname }, Type = tModified };
            }

            var typeName = node.DotnetTypeName ?? await GenerateType(node);
            List<string> generics = new();
            List<string> subTypes = new();
            List<string> constraints = new();

            if(node.Children?.Any() ?? false)
            {
                for (int i = 0; i != node.Children.Length; i++)
                {
                    var child = node.Children[i];
                    var parsed = await ParseParameter(null, child, type, null, index, i);
                    if(parsed.Generics.Any())
                        generics.AddRange(parsed.Generics);
                    if (parsed.Type is not null)
                        subTypes.Add(parsed.Type);

                    if (child.IsGeneric)
                    {
                        switch (node.DotnetTypeName)
                        {
                            case "Range":
                                constraints.Add($"where {parsed.Type} : struct");
                                break;
                        }
                    }
                }
            }
            if (subTypes.Any())
                typeName += $"<{string.Join(", ", subTypes)}>";

            if (modifier.HasValue)
            {
                switch (modifier.Value)
                {
                    case TypeModifier.OptionalType:
                        typeName += "?";
                        break;
                    case TypeModifier.SetOfType:
                        typeName = $"IEnumerable<{typeName}>";
                        break;
                    default:
                        break;
                }
            }

            return new ParsedParameter { GenericConditions = constraints, Name = name, Type = typeName, Generics = generics.ToArray() };
        }

        private static async ValueTask<string> BuildType(TypeNode node, TypeModifier modifier, bool shouldGenerate = true, bool allowGenerics = false, string? genericName = null)
        {
           var name = node.IsGeneric
               ? allowGenerics && genericName is not null ? genericName : "object"
               : node.DotnetType is null && node.RequiresGeneration && shouldGenerate
                   ? await GenerateType(node)
                   : node.ToString() ?? "object";

            return modifier switch
            {
                TypeModifier.OptionalType => $"{name}?",
                TypeModifier.SingletonType => name,
                TypeModifier.SetOfType => $"IEnumerable<{name}>",
                _ => name
            };
        }

        private static async ValueTask<string> GenerateType(TypeNode node)
        {
            var edgedbType = (await QueryBuilder.Select<Models.Type>().Filter(x => x.Name == node.EdgeDBName).ExecuteAsync(_client!)).FirstOrDefault();

            if (edgedbType is null)
                throw new Exception($"Failde to find type {node.EdgeDBName}");

            if (TypeUtils.GeneratedTypes.TryGetValue(edgedbType.Name, out var dotnetType))
                return dotnetType;

            var meta = await edgedbType.GetMetaInfoAsync(_client!);
            var writer = new CodeWriter();
            string typeName = "";

            using(_ = writer.BeginScope("namespace EdgeDB"))
            {
                switch (meta.Type)
                {
                    case MetaInfoType.Object:
                        {

                        }
                        break;
                    case MetaInfoType.Enum:
                        {
                            var moduleMatch = Regex.Match(edgedbType.Name, @"(.+)::(.*?)$");
                            writer.AppendLine($"[EdgeDBType(ModuleName = \"{moduleMatch.Groups[1].Value}\")]");
                            typeName = moduleMatch.Groups[2].Value!;
                            using (_ = writer.BeginScope($"public enum {typeName}"))
                            {
                                foreach (var value in meta.EnumValues!)
                                {
                                    writer.AppendLine($"{value},");
                                }
                            }
                        }
                        break;
                    default:
                        throw new Exception($"Unknown stdlib builder for type {edgedbType.TypeOfSelf}");
                }
            }

            File.WriteAllText(Path.Combine(STDLIB_PATH, $"{typeName}.g.cs"), writer.ToString());
            TypeUtils.GeneratedTypes.Add(edgedbType.Name, typeName);
            return typeName;
        }


        private static readonly Regex _typeCastOne = new(@"(<[^<]*?>)");
        private static async Task<string> ParseDefaultAsync(string @default)
        {
            try
            {
                var result = await _client!.QuerySingleAsync<object>($"select {@default}");
                return result switch
                {
                    bool b => b.ToString().ToLower(),
                    _ => QueryUtils.ParseObject(result),
                };
            }
            catch(Exception x)
            {
                throw;
            }
        }
    }
}
