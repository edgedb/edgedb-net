using CommandLine;
using EdgeDB.Binary.Codecs;
using EdgeDB.Binary.Protocol;
using EdgeDB.CLI;
using EdgeDB.CLI.Utils;
using EdgeDB.Generator.TypeGenerators;
using EdgeDB.Utils;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EdgeDB.Generator
{
    internal class CodeGenerator
    {
        public IEnumerable<string> GeneratedFiles
            => _generatedFilesMap.Values;

        private readonly Dictionary<string, string> _generatedFilesMap = new();

        private readonly EdgeDBClient _client;
        private readonly ILogger _logger;

        public CodeGenerator(EdgeDBConnection connection)
        {
            _logger = Serilog.Log.ForContext<CodeGenerator>();

            _client = new EdgeDBClient(connection, new() { Logger = _logger.CreateClientLogger() });
        }

        public bool TryGetGeneratedFile(string edgeqlPath, [MaybeNullWhen(false)] out string generatedFile)
            => _generatedFilesMap.TryGetValue(edgeqlPath, out generatedFile);

        public async Task<ParseResult> ParseAsync(GeneratorTargetInfo target, CancellationToken token)
        {
            await using var client = await _client.GetOrCreateClientAsync<EdgeDBBinaryClient>(token);

            return await client.ProtocolProvider.ParseQueryAsync(new Binary.Protocol.QueryParameters(
                target.EdgeQL,
                null,
                Capabilities.All,
                Cardinality.Many,
                IOFormat.Binary,
                false
            ), token);
        }

        public async Task GenerateAsync(
            IEnumerable<string> targets, GeneratorContext context, CancellationToken token,
            bool force = false, ITypeGenerator? typeGenerator = null)
        {
            var targetInfos = new List<GeneratorTargetInfo>();

            foreach (var target in targets)
            {
                if (!File.Exists(target))
                {
                    _logger.Warning("Target file {@File} doesn't exist, skipping", target);
                    continue;
                }

                string edgeql;
                try
                {
                    edgeql = File.ReadAllText(target);
                }
                catch (Exception x)
                {
                    _logger.Error(x, "Failed to read target file {@File}", target);
                    continue;
                }

                var hash = HashUtils.HashEdgeQL(edgeql);

                targetInfos.Add(new GeneratorTargetInfo(edgeql, Path.GetFileNameWithoutExtension(target), target, hash));
            }

            typeGenerator ??= await GetTypeGeneratorAsync(token);

            _logger.Information("Chosing type generator {@Generator}", typeGenerator);


            foreach (var target in targetInfos)
            {
                try
                {
                    await GenerateAsync(target, context, token, typeGenerator, force);
                }
                catch(Exception x)
                {
                    _logger.Error(x, "Type generation failed for {@Target}", target);
                }
            }

            _logger.Information("Running post-process...");
            await typeGenerator.PostProcessAsync(context);
        }

        public async Task GenerateAsync(
            GeneratorTargetInfo target, GeneratorContext context,
            CancellationToken token, ITypeGenerator? typeGenerator = null,
            bool force = false)
        {
            var isOwnGenerator = typeGenerator is null;

            typeGenerator ??= await GetTypeGeneratorAsync(token);

            await ValidateOrGenerateAsync(target, context, typeGenerator, force, token);

            if (isOwnGenerator)
                await typeGenerator.PostProcessAsync(context);
        }

        public async Task<ITypeGenerator> GetTypeGeneratorAsync(CancellationToken token)
        {
            var protocolVersion = await GetClientProtocolVersionAsync(token);

            return protocolVersion switch
            {
                (1, >= 0) => new V1TypeGenerator(),
                (2, >= 0) => new V2TypeGenerator(),
                _ => throw new NotSupportedException($"Cannot determine the generator to use for the protocol version {protocolVersion}")
            };
        }

        private async Task ValidateOrGenerateAsync(
            GeneratorTargetInfo target, GeneratorContext context,
            ITypeGenerator typeGenerator, bool force,
            CancellationToken token)
        {
            var outputPath = target.GetGenerationFileName(context);

            if (!force)
            {

                if (File.Exists(outputPath) && await IsUpToDateAsync(target, context))
                {
                    _logger.Information("Skipping {@File}, up to date version exists: {@UpToDate}", target.FileName, outputPath);
                    return;
                }
            }

            _logger.Information("Parsing {@Target}...", target.FileName);


            var parsed = await ParseAsync(target, token);

            var resultTypeDef = await typeGenerator.GetTypeAsync(parsed.OutCodecInfo.Codec, target, context);

            var code = await GenerateCSharpContentAsync(resultTypeDef, parsed, target, context, typeGenerator);

            _logger.Information("Writing {@target} to {@Path}", target.FileName, outputPath);

            _logger.Verbose("Codec structure:\n{@Codec}", CodecFormatter.Format(parsed.OutCodecInfo.Codec).ToString());
            _logger.Verbose("Generated Code:\n{Code}", code);

            await File.WriteAllTextAsync(outputPath, code, token);

            if (!_generatedFilesMap.TryAdd(target.Path, outputPath))
                _generatedFilesMap[target.Path] = outputPath;
        }

        private async Task<string> GenerateCSharpContentAsync(
            string typeRef, ParseResult parseResult, GeneratorTargetInfo target,
            GeneratorContext context, ITypeGenerator typeGenerator)
        {
            var resultType = parseResult.Cardinality switch
            {
                Cardinality.NoResult => null,
                Cardinality.AtMostOne => $"{typeRef}?",
                Cardinality.One => typeRef,
                _ => $"IReadOnlyCollection<{typeRef}?>"
            };

            var method = parseResult.Cardinality switch
            {
                Cardinality.NoResult => "ExecuteAsync",
                Cardinality.AtMostOne => "QuerySingleAsync",
                Cardinality.One => "QueryRequiredSingleAsync",
                _ => "QueryAsync"
            };

            var methodParametersArray = Array.Empty<string>();
            var dictParamsArray = Array.Empty<string>();

            if (parseResult.InCodecInfo.Codec is ObjectCodec objCodec)
            {
                methodParametersArray = new string[objCodec.InnerCodecs.Length];
                dictParamsArray = new string[objCodec.InnerCodecs.Length];

                // order by cardinality
                var oderedProps = objCodec.Properties.OrderByDescending(x => x.Cardinality, CardinalityComparer.Instance).ToArray();

                for (int i = 0; i != oderedProps.Length; i++)
                {
                    var codec = oderedProps[i].Codec;
                    var name = oderedProps[i].Name;

                    var type = await typeGenerator.GetTypeAsync(codec, target, context);

                    methodParametersArray[i] = $"{ApplyCardinality(type, oderedProps[i].Cardinality)} {TextUtils.ToCamelCase(name)}";

                    if (oderedProps[i].Cardinality is Cardinality.AtMostOne)
                        methodParametersArray[i] += " = null";

                    dictParamsArray[i] = $"{{ \"{name}\", {TextUtils.ToCamelCase(name)} }}";
                }
            }

            var methodParameters = methodParametersArray.Length > 0
                ? $", {string.Join(", ", methodParametersArray)}"
                : string.Empty;

            var dictParameters = dictParamsArray.Length > 0
                ? $", new Dictionary<string, object?>() {{ {string.Join(", ", dictParamsArray)} }}"
                : string.Empty;

            var paramSummarys = methodParametersArray.Length > 0
                ? $"\n    /// {string.Join("\n    /// ", methodParametersArray.Select(x => $"<param name=\"{x.Split(' ')[1]}\">The {x.Split(' ')[1]} parameter in the query.</param>"))}"
                : string.Empty;

            return
$$""""
// AUTOGENERATED: DO NOT MODIFY
// edgeql:{{target.Hash}}
// Generated on {{DateTime.UtcNow:O}}
#nullable enable
using EdgeDB;
using EdgeDB.DataTypes;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace {{context.GenerationNamespace}};

public static class {{target.FileName}}
{
    /// <summary>
    ///     The string containing the query defined in <c>{{target.Path}}</c>.
    /// </summary>
    public static readonly string Query;
    private static readonly string _queryHex = "{{HexConverter.ToHex(Encoding.UTF8.GetBytes(target.EdgeQL))}}";


    static {{target.FileName}}()
    {
        Query = string.Join("", Regex.Split(_queryHex, "(?<=\\G..)(?!$)").Select(x => (char)Convert.ToByte(x, 16)));
    }

    /// <summary>
    ///     Executes the {{target.FileName}} query, defined as:
    ///     <code>
    ///         {{TextUtils.EscapeToXMLComment(Regex.Replace(target.EdgeQL!, @"(\n)", m => $"{m.Groups[1].Value}{"",4}///         "))}}
    ///     </code>
    /// </summary>
    /// <param name="client">The client to execute the query on.</param>{{paramSummarys}}
    public static Task<{{resultType}}> ExecuteAsync(IEdgeDBQueryable client{{methodParameters}}, CancellationToken token = default)
        => client.{{method}}<{{typeRef}}>(
            Query{{dictParameters}},
            capabilities: Capabilities.{{parseResult.Capabilities}}, token: token
        );

    /// <summary>
    ///     Executes the {{target.FileName}} query, defined as:
    ///     <code>
    ///         {{TextUtils.EscapeToXMLComment(Regex.Replace(target.EdgeQL!, @"(\n)", m => $"{m.Groups[1].Value}{"",4}///         "))}}
    ///     </code>
    /// </summary>
    /// <param name="client">The client to execute the query on.</param>{{paramSummarys}}
    public static Task<{{resultType}}> {{target.FileName}}Async(this IEdgeDBQueryable client{{methodParameters}}, CancellationToken token = default)
        => ExecuteAsync(client{{(methodParametersArray.Length > 0 ? $", {string.Join(", ", methodParametersArray.Select(x => x.Split(' ')[1]))}" : string.Empty)}}, token: token);
}
#nullable restore
"""";
        }

        internal async Task<bool> IsUpToDateAsync(GeneratorTargetInfo target, GeneratorContext context)
        {
            if (!File.Exists(target.GetGenerationFileName(context)))
                return false;

            using var stream = File.OpenRead(target.GetGenerationFileName(context));
            using var reader = new StreamReader(stream);

            // header
            if (await reader.ReadLineAsync() is null)
                return false;

            var fileHashHeader = await reader.ReadLineAsync();

            if (fileHashHeader is null)
                return false;

            var match = Regex.Match(fileHashHeader, @"^\/\/ edgeql:([0-9a-fA-F]{64})$");

            return match.Success && match.Groups[1].Value == target.Hash;
        }

        private async Task<ProtocolVersion> GetClientProtocolVersionAsync(CancellationToken token)
        {
            await using var binaryClient = await _client.GetOrCreateClientAsync<EdgeDBBinaryClient>(token);

            await binaryClient.SyncAsync(token);

            return binaryClient.ProtocolProvider.Version;
        }

        public static string ApplyCardinality(string type, Cardinality cardinality)
        {
            if (cardinality is Cardinality.AtMostOne or Cardinality.Many or Cardinality.NoResult)
                type += "?";

            return type;
        }

        private readonly struct CardinalityComparer : IComparer<Cardinality>
        {
            public static readonly CardinalityComparer Instance = new();

            private static readonly Dictionary<Cardinality, int> _map = new()
            {
                {Cardinality.NoResult, 0 },
                {Cardinality.AtMostOne, 1 },
                {Cardinality.One, 2 },
                {Cardinality.AtLeastOne, 3 },
                {Cardinality.Many, 4 }
            };

            public int Compare(Cardinality x, Cardinality y)
                => _map[x] - _map[y];
        }
    }
}
