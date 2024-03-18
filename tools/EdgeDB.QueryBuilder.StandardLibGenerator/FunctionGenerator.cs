using EdgeDB.DataTypes;
using EdgeDB.Models.DataTypes;
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
        public string? EdgeQLName { get; init; }
        public string? Group { get; init; }
        public string? Expression { get; init; }
        public Parameter[]? Parameters { get; init; }
        public Models.Type? Result { get; init; }
        public TypeModifier Modifier { get; init; }
    }

    internal class FunctionGenerator
    {
        public static string StdLibOutputPath => Path.Combine(Environment.CurrentDirectory, "output", "stdlib");

        public static string OutputPath => Path.Combine(Environment.CurrentDirectory, "output", "funcs");
        //@"C:\Users\lynch\source\repos\EdgeDB\src\EdgeDB.Net.QueryBuilder\Translators\Methods\Generated";

        public static readonly TextInfo TextInfo = new CultureInfo("en-US").TextInfo;
        private static readonly Regex _groupRegex = new(@"(.+?)<.+?>");
        private static readonly List<string> _generatedPublicFuncs = new();
        private static CodeWriter? _edgeqlClassWriter;
        private static EdgeDBClient? _client;

        private static readonly Dictionary<string, string> _keywords = new()
        {
            { "base", "@base" },
            { "default", "@default" },
            { "new", "@new" },
            { "string", "str" },
            { "object", "@object" }
        };

        public static async ValueTask GenerateAsync(CodeWriter eqlWriter, EdgeDBClient client,
            IReadOnlyCollection<Function> functions, List<RequiredMethodTranslator> translators)
        {
            _client = client;
            _edgeqlClassWriter = eqlWriter;
            if (!Directory.Exists(OutputPath))
                Directory.CreateDirectory(OutputPath);

            if (!Directory.Exists(StdLibOutputPath))
                Directory.CreateDirectory(StdLibOutputPath);

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
            catch (Exception x)
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
            public bool Optional { get; set; }
        }

        private static async ValueTask ProcessGroup(string groupType, IEnumerable<Function> funcs,
            List<string> generatedFuncs, IEnumerable<RequiredMethodTranslator> translators)
        {
            var writer = new CodeWriter();

            var edgedbType = funcs.FirstOrDefault(x => x.ReturnType!.Name! == groupType)?.ReturnType!;
            var translatorType = TypeUtils.TryGetType(groupType, out var tInfo)
                ? await TypeUtils.BuildType(_client!, tInfo, TypeModifier.SingletonType, StdLibOutputPath, true)
                : groupType switch
                {
                    "tuple" => typeof(ITuple).Name,
                    "array" => typeof(Array).Name,
                    "set" => typeof(IEnumerable).Name,
                    "range" => "IRange",
                    "multirange" => typeof(MultiRange<>).Name,
                    _ => groupType.Contains("::")
                        ? await TypeUtils.BuildType(_client!, new(groupType, null), TypeModifier.SingletonType,
                            StdLibOutputPath, true)
                        : throw new Exception($"Failed to find matching type for {groupType}")
                };

            writer.AppendLine("#nullable restore");
            writer.AppendLine("using EdgeDB;");
            writer.AppendLine("using EdgeDB.DataTypes;");
            writer.AppendLine("using EdgeDB.Translators.Methods;");
            writer.AppendLine("using System.Runtime.CompilerServices;");
            writer.AppendLine();

            using (var namespaceScope = writer.BeginScope("namespace EdgeDB.Translators"))
            using (var classScope =
                   writer.BeginScope(
                       $"internal partial class {TextInfo.ToTitleCase(groupType.Replace("::", " ")).Replace(" ", "")}MethodTranslator : MethodTranslator<EdgeQL>"))
            {
                foreach (var func in funcs)
                {
                    try
                    {
                        var funcName = TextInfo.ToTitleCase(func.Name!.Split("::").Last().Replace("_", " "))
                            .Replace(" ", "");

                        var funcSpl = func.Name.Split("::");
                        var funcCleanName = funcSpl[^1];
                        var funcModule = string.Join("::", funcSpl[..^1]);

                        if (!TypeUtils.TryGetType(func.ReturnType!.Name!, out var returnTypeInfo))
                            throw new Exception($"Failed to get type {groupType}");

                        var dotnetReturnType = await ParseParameter("result", returnTypeInfo, func.ReturnType,
                            func.ReturnTypeModifier);

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
                                info = new TypeNode(
                                    await TypeUtils.BuildType(_client!, node, TypeModifier.SingletonType,
                                        StdLibOutputPath,
                                        true), true, parameters[i].Parameter.Type!.Name);
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

                            parsedParameters[i] = await ParseParameter(name, info, x.Parameter.Type!,
                                x.Parameter.TypeModifier, i);
                            if (!string.IsNullOrEmpty(x.Parameter.Default) && x.Parameter.Default != "{}")
                                parsedParameters[i].DefaultValue = await ParseDefaultAsync(x.Parameter.Default, info);
                        }

                        var parameterGenerics =
                            parsedParameters.Where(x => x.Generics.Any()).SelectMany(x => x.Generics);

                        var strongMappedParameters = string.Join(", ", parsedParameters.Select((x, i) =>
                        {
                            var t = $"{x.Type} {(_keywords.TryGetValue(x.Name!, out var p) ? p : x.Name)}";
                            var param = parameters.ElementAt(i);
                            if (!string.IsNullOrEmpty(x.DefaultValue))
                            {
                                if (x.DefaultValue is "{}" && !x.Optional)
                                    return t;

                                var defaultVal = x.DefaultValue;

                                t += " = " + defaultVal switch
                                {
                                    "{}" => x.Generics.Any() || (param.Node!.DotnetType?.IsValueType ?? false)
                                        ? "default"
                                        : "null",
                                    _ => defaultVal
                                };
                            }

                            return t;
                        }));

                        var parsedMappedParameters = "QueryWriter writer";

                        if (parameters.Length > 0)
                        {
                            var strParams = parameters.Select(x =>
                            {
                                var p = "TranslatedParameter";

                                if (x.Parameter.TypeModifier is TypeModifier.OptionalType)
                                    p += "?";

                                return $"{p} {x.Parameter.Name}Param";
                                // return p + x.Parameter.Name + "Param";

                                //$"string? {x!.Parameter.Name}Param"
                            });
                            parsedMappedParameters +=
                                $", {string.Join(", ", strParams)}";
                        }

                        if (!generatedFuncs.Contains(funcName))
                        {
                            writer.AppendLine($"[MethodName(nameof(EdgeQL.{funcName}))]");
                            writer.AppendLine($"public void {funcName}Translator({parsedMappedParameters})");

                            using (var methodScope = writer.BeginScope())
                            {
                                var methodBody = $"writer.Function(\"{func.Name}\"";

                                for (var i = 0; i != func.Parameters?.Length; i++)
                                {
                                    methodBody += ", ";
                                    var param = func.Parameters![i];

                                    var value = param.TypeModifier switch
                                    {
                                        TypeModifier.OptionalType => $"OptionalArg({param.Name}Param)",
                                        _ => $"{param.Name}Param"
                                    };

                                    if (param.Kind is ParameterKind.NamedOnlyParam)
                                    {
                                        methodBody += $"new Terms.FunctionArg({value}, \"{param.Name}\")";
                                    }
                                    else
                                    {
                                        methodBody += value;
                                    }
                                }

                                methodBody += ");";

                                writer.AppendLine(methodBody);

                                Console.WriteLine($"Generated {funcName} => {methodBody}");
                            }

                            writer.AppendLine();

                            generatedFuncs.Add(funcName);
                        }

                        var formattedGenerics = string.Join(", ",
                            dotnetReturnType.Generics
                                .Concat(parsedParameters.Where(x => x.Generics!.Any()).SelectMany(x => x.Generics!))
                                .Distinct());

                        var genKey =
                            $"{(dotnetReturnType.Generics.Any() ? "`1" : dotnetReturnType.Type)}{funcName}{(formattedGenerics.Any() ? $"<`{formattedGenerics.Count()}>" : "")}({string.Join(", ", parsedParameters.Select(x => x.Generics.Any() ? "`1" : x.Type))})";
                        if (!_generatedPublicFuncs.Contains(genKey))
                        {
                            var desc = func.Annotations!.FirstOrDefault(x => x.Name == "std::description");
                            if (desc is not null)
                            {
                                _edgeqlClassWriter!.AppendLine("/// <summary>");
                                _edgeqlClassWriter.AppendLine(
                                    $"///     {Regex.Replace(desc.Value!.Replace("\n", "").Normalize().Trim(), " {2,}", " ")}");
                                _edgeqlClassWriter.AppendLine("/// </summary>");
                            }

                            _edgeqlClassWriter!.AppendLine($"[EdgeQLFunction(\"{funcCleanName}\", \"{funcModule}\", \"{func.ReturnType.Name}\", {(func.ReturnTypeModifier is TypeModifier.SetOfType).ToString().ToLower()}, {(func.ReturnTypeModifier is TypeModifier.OptionalType).ToString().ToLower()})]");
                            _edgeqlClassWriter!.AppendLine(
                                $"public static {dotnetReturnType.Type} {funcName}{(formattedGenerics.Any() ? $"<{formattedGenerics}>" : "")}({strongMappedParameters})");
                            foreach (var c in parsedParameters.Where(x => x.GenericConditions.Any())
                                         .SelectMany(x => x.GenericConditions)
                                         .Concat(dotnetReturnType.GenericConditions).Distinct())
                            {
                                _edgeqlClassWriter.AppendLine($"    {c}");
                            }

                            _edgeqlClassWriter.AppendLine("    => default!;");
                            _generatedPublicFuncs.Add(genKey);
                        }
                    }
                    catch (Exception x)
                    {
                        Console.WriteLine(x);
                    }
                }

                foreach (var translator in translators)
                {
                    var translatorParams = translator.Parameters!.Select(x =>
                    {
                        var p = "TranslatedParameter";

                        if (x.TypeModifier is TypeModifier.OptionalType)
                            p += "?";

                        return $"{p} {x.Name}Param";
                    });
                    writer.AppendLine($"[MethodName(nameof(EdgeQL.{translator.TargetName}))]");
                    writer.AppendLine(
                        $"public void {translator.TargetName}(QueryWriter writer, {string.Join(", ", translatorParams)})");

                    using (_ = writer.BeginScope())
                    {
                        writer.AppendLine($"{translator.Expression};");
                    }

                    if (!TypeUtils.TryGetType(translator.Result!.Name!, out var returnTypeInfo))
                        throw new Exception($"Failed to get type {groupType}");

                    var dotnetReturnType =
                        await ParseParameter("result", returnTypeInfo, translator.Result, translator.Modifier);

                    var parameters = translator.Parameters!.Select<Parameter, (Parameter Parameter, TypeNode? Node)>(
                        x =>
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
                            info = new TypeNode(
                                await TypeUtils.BuildType(_client!, node, TypeModifier.SingletonType, StdLibOutputPath,
                                    true), true, parameters[i].Parameter.Type!.Name);
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

                        parsedParameters[i] =
                            await ParseParameter(name, info, x.Parameter.Type!, x.Parameter.TypeModifier, i);
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
                                "{}" => x.Generics.Any() || (param.Node!.DotnetType?.IsValueType ?? false)
                                    ? "default"
                                    : "null",
                                _ => defaultVal
                            };
                        }
                        else if (x.Optional)
                        {
                            t += " = default";
                        }

                        return t;
                    }));

                    var parameterGenerics = parsedParameters.Where(x => x.Generics.Any()).SelectMany(x => x.Generics);

                    var formattedGenerics = string.Join(", ",
                        dotnetReturnType.Generics
                            .Concat(parsedParameters.Where(x => x.Generics!.Any()).SelectMany(x => x.Generics!))
                            .Distinct());

                    var genKey =
                        $"{(dotnetReturnType.Generics.Any() ? "`1" : dotnetReturnType.Type)}{translator.TargetName}{(formattedGenerics.Any() ? $"<`{formattedGenerics.Count()}>" : "")}({string.Join(", ", parsedParameters.Select(x => x.Generics.Any() ? "`1" : x.Type))})";
                    if (!_generatedPublicFuncs.Contains(genKey))
                    {
                        var fSpl = translator.EdgeQLName.Split("::");
                        var funcCleanName = fSpl[^1];
                        var funcModule = string.Join("::", fSpl[..^1]);

                        _edgeqlClassWriter!.AppendLine($"[EdgeQLFunction(\"{funcCleanName}\", \"{funcModule}\", \"{translator.Result.Name}\", {(translator.Modifier is TypeModifier.SetOfType).ToString().ToLower()}, {(translator.Modifier is TypeModifier.OptionalType).ToString().ToLower()})]");
                        _edgeqlClassWriter!.AppendLine(
                            $"public static {dotnetReturnType.Type} {translator.TargetName}{(formattedGenerics.Any() ? $"<{formattedGenerics}>" : "")}({strongMappedParameters})");
                        foreach (var c in parsedParameters.Where(x => x.GenericConditions.Any())
                                     .SelectMany(x => x.GenericConditions).Concat(dotnetReturnType.GenericConditions)
                                     .Distinct())
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
                File.WriteAllText(
                    Path.Combine(OutputPath, $"{TextInfo.ToTitleCase(groupType).Replace(":", "")}.g.cs"),
                    writer.ToString());
            }
            catch (Exception x)
            {
                Console.Error.WriteLine(x);
            }
        }

        private static async ValueTask<ParsedParameter> ParseParameter(string? name, TypeNode node, Models.Type type,
            TypeModifier? modifier, int index = 0, int subIndex = 0)
        {
            if (node.IsGeneric)
            {
                var tname =
                    $"T{TextInfo.ToTitleCase(Regex.Match(node.EdgeDBName, @"(?>.+?::|^)(.*?)$").Groups[1].Value.Replace("any", ""))}";
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

                return new ParsedParameter() { Name = name, Generics = new string[] { tname }, Type = tModified, Optional = modifier is TypeModifier.OptionalType};
            }

            var typeName = node.DotnetTypeName ?? await TypeUtils.GenerateType(_client!, node, StdLibOutputPath);
            List<string> generics = new();
            List<string> subTypes = new();
            List<string> constraints = new();

            if (node.Children?.Any() ?? false)
            {
                for (int i = 0; i != node.Children.Length; i++)
                {
                    var child = node.Children[i];
                    var parsed = await ParseParameter(null, child, type, null, index, i);
                    if (parsed.Generics.Any())
                        generics.AddRange(parsed.Generics);
                    if (parsed.Type is not null)
                        subTypes.Add(parsed.Type);

                    if (child.IsGeneric)
                    {
                        switch (node.DotnetTypeName)
                        {
                            case "Range" or "MultiRange":
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

            return new ParsedParameter
            {
                GenericConditions = constraints, Name = name, Type = typeName, Generics = generics.ToArray(),
                Optional = modifier is TypeModifier.OptionalType
            };
        }

        private static readonly Regex _typeCastOne = new(@"(<[^<]*?>)");

        private static async Task<string> ParseDefaultAsync(string @default, TypeNode node)
        {
            var result = await _client!.QuerySingleAsync<object>($"select {@default}");
            return result switch
            {
                bool b => b.ToString().ToLower(),
                null => "null",
                string str when node.IsEnum => $"{node.DotnetTypeName}.{str}",
                string str => $"\"{str}\"",
                _ => result.ToString()!
            };
        }
    }
}
