using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Generator
{
    internal record GeneratorTargetInfo(
        string EdgeQL,
        string? FileName,
        string? Path,
        string Hash
    );
}
