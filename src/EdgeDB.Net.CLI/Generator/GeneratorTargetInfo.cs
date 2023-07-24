using EdgeDB.CLI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Generator
{
    internal record GeneratorTargetInfo(
        string EdgeQL,
        string FileName,
        string Path,
        string Hash
    )
    {
        public string GetGenerationFileName(GeneratorContext context)
            => System.IO.Path.Combine(context.OutputDirectory, $"{FileName}.g.cs");

        public static GeneratorTargetInfo FromFile(string target)
        {
            if (!File.Exists(target))
            {
                throw new FileNotFoundException("Failed to find file", target);
            }

            var edgeql = File.ReadAllText(target);
            
            var hash = HashUtils.HashEdgeQL(edgeql);

            return new(edgeql, System.IO.Path.GetFileNameWithoutExtension(target), target, hash);
        }
    }
}
