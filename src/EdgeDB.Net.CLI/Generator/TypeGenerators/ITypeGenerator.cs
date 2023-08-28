using EdgeDB.Binary.Codecs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Generator.TypeGenerators
{
    internal interface ITypeGenerator
    {
        ValueTask<string> GetTypeAsync(ICodec codec, GeneratorTargetInfo? target, GeneratorContext context);

        ValueTask PostProcessAsync(GeneratorContext context);

        IEnumerable<GeneratedFileInfo> GetGeneratedFiles();

        void RemoveGeneratedReferences(IEnumerable<GeneratedFileInfo> references);
    }

    internal sealed class GeneratedFileInfo
    {
        public string GeneratedPath { get; }
        public List<string> EdgeQLReferences { get; }

        public GeneratedFileInfo(string generatedPath, string? edgeqlPath)
        {
            GeneratedPath = generatedPath;
            EdgeQLReferences = new();

            if (edgeqlPath is not null)
                EdgeQLReferences.Add(edgeqlPath);
        }
    }
}
