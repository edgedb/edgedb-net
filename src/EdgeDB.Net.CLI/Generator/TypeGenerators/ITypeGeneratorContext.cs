using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Generator.TypeGenerators
{
    internal interface ITypeGeneratorContext
    {
        IEnumerable<Task> FileGenerationTasks { get; }
    }
}
