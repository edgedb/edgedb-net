using EdgeDB.Binary.Codecs;
using EdgeDB.CLI.Generator.Results;
using System;
namespace EdgeDB.CLI.Generator.Models
{
    internal class TransientTargetInfo
    {
        public IQueryResult Result { get; }
        public Cardinality ResultCardinality { get; }
        public Capabilities Capabilities { get; }
        public IArgumentCodec Arguments { get; }
        public GenerationTargetInfo Info { get; }

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

