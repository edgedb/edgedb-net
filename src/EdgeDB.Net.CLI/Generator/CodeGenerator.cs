using EdgeDB.Binary.Codecs;
using EdgeDB.CLI;
using EdgeDB.CLI.Generator.Models;
using EdgeDB.CLI.Generator.Results;
using EdgeDB.CLI.Utils;
using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace EdgeDB.CLI.Generator
{
    internal class CodeGenerationHandle : IDisposable
    {
        private readonly List<string> _trackedGenerationFiles;

        public CodeGenerationHandle(string dir)
        {
            _trackedGenerationFiles = new(Directory.GetFiles(dir, "*.g.cs").Concat(Directory.GetFiles(Path.Combine(dir, "Types"), "*.g.cs")));
        }

        public void Track(string file)
        {
            _trackedGenerationFiles.Remove(file);
        }

        public void Dispose()
        {
            foreach(var file in _trackedGenerationFiles)
            {
                try
                {
                    File.Delete(file);
                }
                catch { }
            }
        }
    }

    
    internal class CodeGenerator
    {
        /// <summary>
        ///     The file header regex for generate C# files.
        /// </summary>
        private static readonly Regex _headerHashRegex = new(@"\/\/ edgeql:([0-9a-fA-F]{64})");

        public static CodeGenerationHandle GetGenerationHandle(string outputDir)
        {
            return new CodeGenerationHandle(outputDir);
        }

        /// <summary>
        ///     Parses a given target file and returns the relative information for generation.
        /// </summary>
        /// <param name="client">The EdgeDB client to use to parse the query.</param>
        /// <param name="outputDir">The output directory for the generated code.</param>
        /// <param name="targetInfo">The target info, containing the target files information.</param>
        /// <returns>A tuple containing information about the query.</returns>
        public static async Task<(IQueryResult Result, Cardinality ResultCardinality, Capabilities Capabilities, IArgumentCodec Args)> ParseAsync(EdgeDBTcpClient client, string outputDir, GenerationTargetInfo targetInfo)
        {
            var parseResult = await client.ParseAsync(targetInfo.EdgeQL!, Cardinality.Many, IOFormat.Binary, Capabilities.ReadOnly, false, default);

            var codecInfo = GetTypeInfoFromCodec(parseResult.OutCodec.Codec, $"{targetInfo.EdgeQLFileNameWithoutExtension}Result");

            return (codecInfo.Build(targetInfo.EdgeQLFilePath!), parseResult.Cardinality, parseResult.Capabilities, (IArgumentCodec)parseResult.InCodec.Codec);
        }

        /// <summary>
        ///     Checks whether an autogenerate header matches a hash.
        /// </summary>
        /// <param name="header">The header of the autogenerated file to check against.</param>
        /// <param name="hash">The hash to check.</param>
        /// <returns>
        ///     <see langword="true"/> if the header matches the hash; otherwise <see langword="false"/>.
        /// </returns>
        public static bool TargetFileHashMatches(string header, string hash)
        {
            var match = _headerHashRegex.Match(header);
            if (!match.Success)
                return false;
            return match.Groups[1].Value == hash;
        }

        /// <summary>
        ///     Gets the <see cref="GenerationTargetInfo"/> for the given file and
        ///     generation target directory.
        /// </summary>
        /// <remarks>
        ///     This operation requires the file to be opened. This function will
        ///     throw if the file is being used by a different process.
        /// </remarks>
        /// <param name="edgeqlFilePath">The path of the edgeql file.</param>
        /// <param name="targetDir">The output target directory.</param>
        /// <returns>
        ///     The <see cref="GenerationTargetInfo"/> for the given file.
        /// </returns>
        public static GenerationTargetInfo GetTargetInfo(string edgeqlFilePath, string targetDir, string rootProjectPath)
        {
            string fileContent = File.ReadAllText(edgeqlFilePath);
            var hash = HashUtils.HashEdgeQL(fileContent);
            var fileName = TextUtils.ToPascalCase(Path.GetFileName(edgeqlFilePath).Split('.')[0]);

            return new GenerationTargetInfo
            {
                EdgeQLFileNameWithoutExtension = fileName,
                EdgeQL = fileContent,
                EdgeQLHash = hash,
                EdgeQLFilePath = edgeqlFilePath,
                TargetFilePath = Path.Combine(targetDir, $"{fileName}.g.cs"),
                RootProjectPath = rootProjectPath
            };
        }

        /// <summary>
        ///     Generates a <see cref="GenerationResult"/> from the given <see cref="GenerationTargetInfo"/>
        ///     and <see cref="EdgeDBBinaryClient.ParseResult"/>.
        /// </summary>
        /// <param name="namespace">The namepsace for the generated code to consume.</param>
        /// <param name="targetInfo">The <see cref="GenerationTargetInfo"/> used for generation.</param>
        /// <param name="parseResult">The parse result from edgedb.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static async Task<GenerationResult> GenerateAsync(string outputDir, string @namespace, TransientTargetInfo target)
        {
            List<string> generatedTypeFiles = new();
            
            var visitor = new ResultVisitor();
            target.Result.Visit(visitor);

            foreach (var type in visitor.GenerationTargets)
            {
                generatedTypeFiles.Add(await TypeGenerator.GenerateTypeAsync(outputDir, @namespace, type));
            }

            // create the class writer
            var writer = new CodeWriter();
            writer.AppendLine("// AUTOGENERATED: DO NOT MODIFY");
            writer.AppendLine($"// edgeql:{target.Info.EdgeQLHash}");
            writer.AppendLine($"// Generated on {DateTime.UtcNow:O}");
            writer.AppendLine("#nullable enable");
            writer.AppendLine($"using EdgeDB;");

            writer.AppendLine();
            writer.AppendLine($"namespace {@namespace};");
            writer.AppendLine();

            var relFilePath = target.Info.RootProjectPath is not null ? Path.GetRelativePath(target.Info.RootProjectPath, target.Info.EdgeQLFilePath!) : target.Info.EdgeQLFilePath;

            // create the executor class
            writer.AppendLine("/// <summary>");
            writer.AppendLine($"///     A class representing the query file <c>{relFilePath}</c>, containing both the query string and methods to execute the query.");
            writer.AppendLine("/// </summary>");

            var classScope = writer.BeginScope($"public static class {target.Info.EdgeQLFileNameWithoutExtension}");

            writer.AppendLine("/// <summary>");
            writer.AppendLine($"///     A string containing the query defined in <c>{relFilePath}</c>");
            writer.AppendLine("/// </summary>");
            writer.AppendLine($"public static readonly string Query =");
            writer.Append($"@\"{target.Info.EdgeQL!.Replace("\"", "\\\"")}\";");
            writer.AppendLine();
            writer.AppendLine();
            
            var method = target.ResultCardinality switch
            {
                Cardinality.AtMostOne => "QuerySingleAsync",
                Cardinality.One => "QueryRequiredSingleAsync",
                _ => "QueryAsync"
            };

            var rawResultType = target.Result.ToCSharp();

            var resultType = target.ResultCardinality switch
            {
                Cardinality.AtMostOne => $"{rawResultType}?",
                Cardinality.One => rawResultType,
                _ => $"IReadOnlyCollection<{rawResultType}?>"
            };

            // build args
            IEnumerable<string>? argParameters;
            IEnumerable<string>? methodArgs;

            if (target.Arguments is NullCodec)
            {
                methodArgs = Array.Empty<string>();
                argParameters = Array.Empty<string>();
            }
            else if (target.Arguments is Binary.Codecs.Object argCodec)
            {
                argParameters = argParameters = argCodec.PropertyNames.Select((x, i) =>
                {
                    var codec = argCodec.InnerCodecs[i];
                    var codecInfo = GetTypeInfoFromCodec(codec, x);

                    return $"{codecInfo.Build(target.Info.EdgeQLFilePath!).ToCSharp()} {TextUtils.ToCamelCase(x)}";
                });

                methodArgs = methodArgs = argCodec.PropertyNames.Select((x, i) =>
                {
                    return $"{{ \"{x}\", {TextUtils.ToCamelCase(x)} }}";
                });
            }
            else
                throw new InvalidOperationException("Argument codec is malformed");

            void WriteMethodSummary()
            {
                writer!.AppendLine("/// <summary>");
                writer.AppendLine($"///     Executes the {target.Info.EdgeQLFileNameWithoutExtension} query, defined as:");
                writer.AppendLine($"///     <code>");
                writer.AppendLine($"///         {TextUtils.EscapeToXMLComment(Regex.Replace(target.Info.EdgeQL!, @"(\n)", m => $"{m.Groups[1].Value}{"".PadLeft(writer.IndentLevel)}///         "))}");
                writer.AppendLine($"///     </code>");
                writer.AppendLine("/// </summary>");
                writer.AppendLine("/// <param name=\"client\">The client to execute the query on.</param>");
                foreach (var arg in argParameters!)
                {
                    writer.AppendLine($"/// <param name=\"{TextUtils.ToCamelCase(arg.Split(' ')[1])}\">The {arg.Split(" ")[1]} parameter of the query.</param>");
                }

                writer.AppendLine("/// <param name=\"token\">A cancellation token used to cancel the asyncronous query.</param>");
                writer.AppendLine("/// <returns>A Task representing the asynchronous query operation. The result of the task is the result of the query.</returns>");
            }


            WriteMethodSummary();
            writer.AppendLine($"public static Task<{resultType}> ExecuteAsync(IEdgeDBQueryable client{(argParameters.Any() ? $", {string.Join(", ", argParameters)}" : "")}, CancellationToken token = default)");
            writer.AppendLine($"    => client.{method}<{rawResultType}>(Query{(methodArgs.Any() ? $", new Dictionary<string, object?>() {{ {string.Join(", ", methodArgs)} }}" : "")}, capabilities: (Capabilities){(ulong)target.Capabilities}ul, token: token);");
            writer.AppendLine();

            WriteMethodSummary();
            writer.AppendLine($"public static Task<{resultType}> {target.Info.EdgeQLFileNameWithoutExtension}Async(this IEdgeDBQueryable client{(argParameters.Any() ? $", {string.Join(", ", argParameters)}" : "")}, CancellationToken token = default)");
            writer.AppendLine($"    => ExecuteAsync(client{(argParameters.Any() ? $", {string.Join(", ", argParameters.Select(x => x.Split(' ')[1]))}" : "")}, token: token);");

            classScope.Dispose();

            writer.AppendLine("#nullable restore");

            return new()
            {
                ExecuterClassName = target.Info.EdgeQLFileNameWithoutExtension,
                EdgeQLHash = target.Info.EdgeQLHash,
                ReturnResult = resultType,
                Parameters = argParameters,
                Code = writer.ToString(),
                GeneratedTypeFiles = generatedTypeFiles
            };
        }

        /// <summary>
        ///     Creates a <see cref="CodecTypeInfo"/> from the given <see cref="ICodec"/>.
        /// </summary>
        /// <param name="codec">The codec to get the type info for.</param>
        /// <param name="name">The optional name of the codec.</param>
        /// <param name="parent">The optional parent of the codec.</param>
        /// <returns>
        ///     A <see cref="CodecTypeInfo"/> representing type information about the provided codec.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     No <see cref="CodecTypeInfo"/> could be created from the provided codec.
        /// </exception>
        private static CodecTypeInfo GetTypeInfoFromCodec(ICodec codec, string? name = null, CodecTypeInfo? parent = null)
        {
            CodecTypeInfo info;

            switch (codec)
            {
                case Binary.Codecs.Object obj:
                    {
                        info = new CodecTypeInfo
                        {
                            Type = CodecType.Object,
                            TypeName = TextUtils.ToPascalCase(name!)
                        };
                        info.Children = obj.InnerCodecs
                            .Select((x, i) =>
                                obj.PropertyNames[i] is "__tname__" or "__tid__"
                                    ? null
                                    : GetTypeInfoFromCodec(x, obj.PropertyNames[i], info))
                            .Where(x => x is not null)!;
                    }
                    break;
                case ICodec set when ReflectionUtils.IsSubclassOfRawGeneric(typeof(Set<>), set.GetType()):
                    {
                        var innerType = ((IWrappingCodec)set).InnerCodec;

                        info = new CodecTypeInfo
                        {
                            Type = CodecType.Set,
                        };
                        info.Children = new[]
                        {
                            GetTypeInfoFromCodec(innerType, parent: info)
                        };
                    }
                    break;
                case ICodec array when ReflectionUtils.IsSubclassOfRawGeneric(typeof(Array<>), array.GetType()):
                    {
                        var innerType = ((IWrappingCodec)array).InnerCodec;

                        info = new CodecTypeInfo
                        {
                            Type = CodecType.Array,
                        };
                        info.Children = new[]
                        {
                            GetTypeInfoFromCodec(innerType, parent: info)
                        };
                    }
                    break;
                case Binary.Codecs.Tuple tuple:
                    {
                        info = new CodecTypeInfo
                        {
                            Type = CodecType.Tuple,
                        };
                        info.Children = tuple.InnerCodecs.Select(x => GetTypeInfoFromCodec(x, parent: info));
                    }
                    break;
                case ICodec scalar when ReflectionUtils.IsSubclassOfInterfaceGeneric(typeof(IScalarCodec<>), codec!.GetType()):
                    {
                        var scalarTarget = codec.GetType().GetInterface("IScalarCodec`1")!.GetGenericArguments()[0];
                        info = new CodecTypeInfo
                        {
                            Type = CodecType.Scalar,
                            Namespace = scalarTarget.Namespace,
                            TypeName = $"{scalarTarget.ToFormattedString()}{(scalarTarget.IsValueType ? "" : "?")}",
                        };
                    }
                    break;
                default:
                    throw new InvalidOperationException($"Unknown codec {codec}");
            }

            info.Name = name ?? info.Name;
            info.Parent = parent;

            return info;
        }
    }
}

