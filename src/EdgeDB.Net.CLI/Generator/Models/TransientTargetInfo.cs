using EdgeDB.Binary.Codecs;
using EdgeDB.CLI.Generator.Results;
using System;
namespace EdgeDB.CLI.Generator.Models
{
    /// <summary>
    ///     Represents a target for codegeneration that has been parsed and will be generated.
    /// </summary>
    internal class TransientTargetInfo
    {
        /// <summary>
        ///     Gets the result of the query.
        /// </summary>
        public IQueryResult Result { get; }

        /// <summary>
        ///     Gets the result cardinality of the query.
        /// </summary>
        public Cardinality ResultCardinality { get; }

        /// <summary>
        ///     Gets the required capabilities of the query.
        /// </summary>
        public Capabilities Capabilities { get; }

        /// <summary>
        ///     Gets the argument codec for the query.
        /// </summary>
        public IArgumentCodec Arguments { get; }

        /// <summary>
        ///     Gets the generation target info.
        /// </summary>
        public GenerationTargetInfo Info { get; }

        /// <summary>
        ///     Constructs a new <see cref="TransientTargetInfo"/>.
        /// </summary>
        /// <param name="parseResult">The result of the <see cref="CodeGenerator.ParseAsync(EdgeDBTcpClient, string, GenerationTargetInfo)"/>.</param>
        /// <param name="info">The pre-parse generation target info.</param>
        public TransientTargetInfo(
            (IQueryResult Result, Cardinality Cardinality, Capabilities Capabilities, IArgumentCodec Arg) parseResult,
            GenerationTargetInfo info)
        {
            Result = parseResult.Result;
            ResultCardinality = parseResult.Cardinality;
            Capabilities = parseResult.Capabilities;
            Arguments = parseResult.Arg;
            Info = info;
        }
    }
}

