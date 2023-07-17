using EdgeDB.Binary.Codecs;
using EdgeDB.Binary.Protocol;
using EdgeDB.CLI;
using EdgeDB.CLI.Utils;
using EdgeDB.Generator.TypeGenerators;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EdgeDB.Generator
{
    internal class CodeGenerator
    {
        private readonly EdgeDBClient _client;
        private readonly ILogger _logger;

        public CodeGenerator(EdgeDBConnection connection)
        {
            _logger = Serilog.Log.ForContext<CodeGenerator>();

            _client = new EdgeDBClient(connection);
        }

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

        public async Task GenerateAsync(IEnumerable<string> targets, GeneratorContext context, CancellationToken token)
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

            var protocolVersion = await GetClientProtocolVersionAsync(token);

            ITypeGenerator typeGenerator = protocolVersion switch
            {
                (1, >= 0) => new V1TypeGenerator(),
                (2, >= 0) => new V2TypeGenerator(),
                _ => throw new NotSupportedException($"Cannot determine the generator to use for the protocol version {protocolVersion}")
            };

            var typeGenerationContext = typeGenerator.CreateContext(context);

            foreach (var target in targetInfos)
            {
                _logger.Debug("Parsing {@Target}...", target);

                ParseResult parsed;

                try
                {
                    parsed = await ParseAsync(target, token);
                }
                catch(Exception x)
                {
                    _logger.Error(x, "Failed to parse {@Target}", target);
                    continue;
                }


                var resultTypeDef = await typeGenerator.GetTypeAsync(parsed.OutCodecInfo.Codec, target, typeGenerationContext);

                var code = await GenerateCSharpFileAsync(resultTypeDef, parsed, target, context, typeGenerator, typeGenerationContext);

                await File.WriteAllTextAsync(Path.Combine(context.OutputDirectory, $"{target.FileName}.g.cs"), code, token);
            }

            foreach(var task in typeGenerationContext.FileGenerationTasks)
            {
                _logger.Debug("Invoking type generator task {@Task}", task);
                await task;
            }
        }

        private async Task<string> GenerateCSharpFileAsync(
            string typeRef, ParseResult parseResult, GeneratorTargetInfo target,
            GeneratorContext context, ITypeGenerator typeGenerator, ITypeGeneratorContext typeGeneratorContext)
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

                for (int i = 0; i != objCodec.InnerCodecs.Length; i++)
                {
                    var codec = objCodec.InnerCodecs[i];
                    var name = objCodec.PropertyNames[i];

                    var type = await typeGenerator.GetTypeAsync(codec, target, typeGeneratorContext);

                    methodParametersArray[i] = $"{type} {TextUtils.ToCamelCase(name)}";
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

namespace {{context.GenerationNamespace}};

public static class {{target.FileName}}
{
    /// <summary>
    ///     The string containing the query defined in <c>{{target.Path}}</c>.
    /// </summary>
    public const string QUERY =
"""
{{TextUtils.EscapeToSourceCode(target.EdgeQL, true)}}
""";

    /// <summary>
    ///     Executes the {{target.FileName}} query, defined as:
    ///     <code>
    ///         {{TextUtils.EscapeToXMLComment(Regex.Replace(target.EdgeQL!, @"(\n)", m => $"{m.Groups[1].Value}{"",4}///         "))}}
    ///     </code>
    /// </summary>
    /// <param name="client">The client to execute the query on.</param>{{paramSummarys}}
    public static Task<{{resultType}}> ExecuteAsync(IEdgeDBQueryable client{{methodParameters}}, CancellationToken token = default)
        => client.{{method}}<{{resultType}}>(
            QUERY{{dictParameters}},
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

        private async Task<ProtocolVersion> GetClientProtocolVersionAsync(CancellationToken token)
        {
            await using var binaryClient = await _client.GetOrCreateClientAsync<EdgeDBBinaryClient>(token);

            await binaryClient.SyncAsync(token);

            return binaryClient.ProtocolProvider.Version;
        }
    }
}
