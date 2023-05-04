using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.TestGenerator.OutputWriters
{
    internal interface IOutputWriter
    {
        Task WriteAsync(string root, List<TestGroup> tests);
    }
}
