using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DotnetTool
{
    internal class Constraint
    {
        public string? Value { get; set; }

        public bool IsExpression { get; set; }
    }
}
