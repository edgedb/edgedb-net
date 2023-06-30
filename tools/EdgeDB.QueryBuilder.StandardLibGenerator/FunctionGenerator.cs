using EdgeDB.DataTypes;
using EdgeDB.StandardLibGenerator.Models;
using System;
using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace EdgeDB.StandardLibGenerator
{
    internal class RequiredMethodTranslator
    {
        public string? TargetName { get; init; }
        public string? Group { get; init; }
        public string? Expression { get; init; }
        public Parameter[]? Parameters { get; init; }
        public Models.Type? Result { get; init; }
        public TypeModifier Modifier { get; init; }
    }

    internal class FunctionGenerator
    {
        public const string STDLIB_PATH = @"C:\Users\lynch\source\repos\EdgeDB\src\EdgeDB.Net.QueryBuilder\stdlib";
        public const string OUTPUT_PATH = @"C:\Users\lynch\source\repos\EdgeDB\src\EdgeDB.Net.QueryBuilder\Translators\Methods\Generated";
        private static readonly TextInfo _textInfo = new CultureInfo("en-US").TextInfo;
        private static readonly Regex _groupRegex = new(@"(.+?)<.+?>");
        private static readonly List<string> _generatedPublicFuncs = new();
        private static CodeWriter? _edgeqlClassWriter;
        private static EdgeDBClient? _client;
        private static readonly Dictionary<string, string> _keywords = new()
        {
            {"base", "@base" },
            {"default", "@default" },
            {"new", "@new" },
            {"string", "str" }
        };

        public static async ValueTask GenerateAsync(CodeWriter eqlWriter, EdgeDBClient client, IReadOnlyCollection<Function> functions, List<RequiredMethodTranslator> translators)
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

                var generatedFuncs = new List<string>();

                foreach (var item in grouped)
                {
                    await ProcessGroup(item.Key!, item, generatedFuncs, translators.Where(x => x.Group == item.Key));
                }
            }
            catch(Exception x)
            {
                Console.Error.WriteLine(x);
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
        private static async ValueTask ProcessGroup(string groupType, IEnumerable<Function> funcs, List<string> generatedFuncs, IEnumerable<RequiredMethodTranslator> translators)
        {
            var writer = new CodeWriter();

            var edgedbType = funcs.FirstOrDefault(x => x.ReturnType!.Name! == groupType)?.ReturnType!;
            var translatorType = TypeUtils.TryGetType(groupType, out var tInfo) ? await TypeUtils.BuildType(_client!, tInfo, TypeModifier.SingletonType, STDLIB_PATH, true) : groupType switch
            {
                "tuple" => typeof(ITuple).Name,
                "array" => typeof(Array).Name,
                "set" => typeof(IEnumerable).Name,
                "range" => "IRange",
                _  => groupType.Contains("::") ? await TypeUtils.BuildType(_client!, new(groupType, null), TypeModifier.SingletonType, STDLIB_PATH, true) : throw new Exception($"Failed to find matching type for {groupType}")
            };

            writer.AppendLine("#nullable restore");
            writer.AppendLine("using EdgeDB;");
            writer.AppendLine("using EdgeDB.DataTypes;");
            writer.AppendLine("using System.Runtime.CompilerServices;");
            writer.AppendLine();

            using (var namespaceScope = writer.BeginScope("namespace EdgeDB.Translators"))
            using (var classScope = writer.BeginScope($"internal partial class {_textInfo.ToTitleCase(groupType.Replace("::", " ")).Replace(" ", "")}MethodTranslator : MethodTranslator<EdgeQL>"))
            {
                foreach (var func in funcs)
                {
                    try
                    {
                        var funcName = _textInfo.ToTitleCase(func.Name!.Split("::")[1].Replace("_", " ")).Replace(" ", "");
                        
                        if (!TypeUtils.TryGetType(func.ReturnType!.Name!, out var returnTypeInfo))
                            throw new Exception($"Faield to get type {groupType}");

                        var dotnetReturnType = await ParseParameter("result", returnTypeInfo, func.ReturnType, func.ReturnTypeModifier);

                        var parameters = func.Parameters!.Select<Parameter, (Parameter Parameter, TypeNode? Node)>(x =>
                        {
                            if (!TypeUtils.TryGetType(x.Type!.Name!, out var info))
                                return (x, null);

                            return (x, info);
                        }).ToArray();

                        for (int i = 0; i != parameters.Length; i++)
                        {
                            var info = parameters[i].Node;

                            if (info is null)
                            {
                                var node = new TypeNode(parameters[i].Parameter.Type!.Name, null);
                                info = new TypeNode(await TypeUtils.BuildType(_client!, node, TypeModifier.SingletonType, STDLIB_PATH, true), true, parameters[i].Parameter.Type!.Name);
                                info.IsEnum = node.IsEnum;

                                parameters[i] = (parameters[i].Parameter, info);
                            }
                        }

                        ParsedParameter[] parsedParameters = new ParsedParameter[parameters.Count()];

                        for(int i = 0; i != parsedParameters.Length; i++)
                        {
                            var x = parameters[i];
                            var name = x.Parameter.Name;
                            var info = x.Node!;

                            parsedParameters[i] = await ParseParameter(name, info, x.Parameter.Type!, x.Parameter.TypeModifier, i);
                            if (!string.IsNullOrEmpty(x.Parameter.Default) && x.Parameter.Default != "{}")
                                parsedParameters[i].DefaultValue = await ParseDefaultAsync(x.Parameter.Default, info);
                        }

                        var parameterGenerics = parsedParameters.Where(x => x.Generics.Any()).SelectMany(x => x.Generics);
                    
                        var strongMappedParameters = string.Join(", ", parsedParameters.Select((x, i) =>
                        {
                            var t = $"{x.Type} {(_keywords.TryGetValue(x.Name!, out var p) ? p : x.Name)}";
                            var param = parameters.ElementAt(i);
                            if (!string.IsNullOrEmpty(x.DefaultValue) && x.DefaultValue != "{}")
                            {
                                var defaultVal = x.DefaultValue;

                                t += " = " + defaultVal switch
                                {
                                    "{}" => x.Generics.Any() || (param.Node!.DotnetType?.IsValueType ?? false) ? "default" : "null",
                                    _ => defaultVal
                                };
                            }

                            return t;
                        }));
                        
                        var parsedMappedParameters = string.Join(", ", parameters.Select(x => $"string? {x!.Parameter.Name}Param"));

                        if(!generatedFuncs.Contains(funcName))
                        {
                            writer.AppendLine($"[MethodName(nameof(EdgeQL.{funcName}))]");
                            writer.AppendLine($"public string {funcName}Translator({parsedMappedParameters})");

                            using (var methodScope = writer.BeginScope())
                            {
                                var methodBody = $"return $\"{func.Name}(";

                                string[] parsedParams = new string[func.Parameters!.Length];

                                for (int i = 0; i != parsedParams.Length; i++)
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

                            generatedFuncs.Add(funcName);
                        }
                        
                        var formattedGenerics = string.Join(", ", dotnetReturnType.Generics.Concat(parsedParameters.Where(x => x.Generics!.Any()).SelectMany(x => x.Generics!)).Distinct());

                        var genKey = $"{(dotnetReturnType.Generics.Any() ? "`1" : dotnetReturnType.Type)}{funcName}{(formattedGenerics.Any() ? $"<`{formattedGenerics.Count()}>" : "")}({string.Join(", ", parsedParameters.Select(x => x.Generics.Any() ? "`1" : x.Type))})";
                        if(!_generatedPublicFuncs.Contains(genKey))
                        {
                            var desc = func.Annotations!.FirstOrDefault(x => x.Name == "std::description");
                            if(desc is not null)
                            {
                                _edgeqlClassWriter!.AppendLine("/// <summary>");
                                _edgeqlClassWriter.AppendLine($"///     {Regex.Replace(desc.Value!.Replace("\n", "").Normalize().Trim(), " {2,}", " ")}");
                                _edgeqlClassWriter.AppendLine("/// </summary>");
                            } 

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

                foreach(var translator in translators)
                {
                    writer.AppendLine($"[MethodName(nameof(EdgeQL.{translator.TargetName}))]");
                    writer.AppendLine($"public string {translator.TargetName}({string.Join(", ", translator.Parameters!.Select(x => $"string? {x.Name}Param"))})");

                    using(_ = writer.BeginScope())
                    {
                        writer.AppendLine($"return $\"{translator.Expression}\";");
                    }

                    if (!TypeUtils.TryGetType(translator.Result!.Name!, out var returnTypeInfo))
                        throw new Exception($"Faield to get type {groupType}");

                    var dotnetReturnType = await ParseParameter("result", returnTypeInfo, translator.Result, translator.Modifier);

                    var parameters = translator.Parameters!.Select<Parameter, (Parameter Parameter, TypeNode? Node)>(x =>
                    {
                        if (!TypeUtils.TryGetType(x.Type!.Name!, out var info))
                            return (x, null);

                        return (x, info);
                    }).ToArray();

                    for (int i = 0; i != parameters.Length; i++)
                    {
                        var info = parameters[i].Node;

                        if (info is null)
                        {
                            var node = new TypeNode(parameters[i].Parameter.Type!.Name, null);
                            info = new TypeNode(await TypeUtils.BuildType(_client!, node, TypeModifier.SingletonType, STDLIB_PATH, true), true, parameters[i].Parameter.Type!.Name);
                            info.IsEnum = node.IsEnum;

                            parameters[i] = (parameters[i].Parameter, info);
                        }
                    }

                    ParsedParameter[] parsedParameters = new ParsedParameter[parameters.Count()];

                    for (int i = 0; i != parsedParameters.Length; i++)
                    {
                        var x = parameters[i];
                        var name = x.Parameter.Name;
                        var info = x.Node!;

                        parsedParameters[i] = await ParseParameter(name, info, x.Parameter.Type!, x.Parameter.TypeModifier, i);
                        if (!string.IsNullOrEmpty(x.Parameter.Default) && x.Parameter.Default != "{}")
                            parsedParameters[i].DefaultValue = await ParseDefaultAsync(x.Parameter.Default, info);
                    }

                    var strongMappedParameters = string.Join(", ", parsedParameters.Select((x, i) =>
                    {
                        var t = $"{x.Type} {(_keywords.TryGetValue(x.Name!, out var p) ? p : x.Name)}";
                        var param = parameters.ElementAt(i);
                        if (!string.IsNullOrEmpty(x.DefaultValue))
                        {
                            var defaultVal = x.DefaultValue;

                            t += " = " + defaultVal switch
                            {
                                "{}" => x.Generics.Any() || (param.Node!.DotnetType?.IsValueType ?? false) ? "default" : "null",
                                _ => defaultVal
                            };
                        }

                        return t;
                    }));

                    var parameterGenerics = parsedParameters.Where(x => x.Generics.Any()).SelectMany(x => x.Generics);

                    var formattedGenerics = string.Join(", ", dotnetReturnType.Generics.Concat(parsedParameters.Where(x => x.Generics!.Any()).SelectMany(x => x.Generics!)).Distinct());

                    var genKey = $"{(dotnetReturnType.Generics.Any() ? "`1" : dotnetReturnType.Type)}{translator.TargetName}{(formattedGenerics.Any() ? $"<`{formattedGenerics.Count()}>" : "")}({string.Join(", ", parsedParameters.Select(x => x.Generics.Any() ? "`1" : x.Type))})";
                    if (!_generatedPublicFuncs.Contains(genKey))
                    {
                        _edgeqlClassWriter!.AppendLine($"public static {dotnetReturnType.Type} {translator.TargetName}{(formattedGenerics.Any() ? $"<{formattedGenerics}>" : "")}({strongMappedParameters})");
                        foreach (var c in parsedParameters.Where(x => x.GenericConditions.Any()).SelectMany(x => x.GenericConditions).Concat(dotnetReturnType.GenericConditions).Distinct())
                        {
                            _edgeqlClassWriter.AppendLine($"    {c}");
                        }
                        _edgeqlClassWriter.AppendLine("    => default!;");
                        _generatedPublicFuncs.Add(genKey);
                    }
                }
            }

            try
            {
                File.WriteAllText(Path.Combine(OUTPUT_PATH, $"{_textInfo.ToTitleCase(groupType).Replace(":", "")}.g.cs"), writer.ToString());
            }
            catch(Exception x)
            {
                Console.Error.WriteLine(x);
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

            var typeName = node.DotnetTypeName ?? await TypeUtils.GenerateType(_client!, node, STDLIB_PATH);
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

        private static readonly Regex _typeCastOne = new(@"(<[^<]*?>)");
        private static async Task<string> ParseDefaultAsync(string @default, TypeNode node)
        {
            if(node.IsEnum)
            {
                return @default.Split("::")[1];
            }

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
