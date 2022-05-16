using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DotnetTool
{
    internal class Module
    {
        public string? Name { get; set; }

        public List<Type> Types { get; set; } = new();
    }
}
