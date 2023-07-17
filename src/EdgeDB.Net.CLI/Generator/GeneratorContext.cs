using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Generator
{
    internal sealed record GeneratorContext(
        string OutputDirectory,
        string GenerationNamespace
    );
}
