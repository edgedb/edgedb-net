using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DotnetTool
{
    internal class Type
    {
        public Module? Parent { get; set; }

        public string? Name { get; set; }

        public string? Extending { get; set; }

        public bool IsAbstract { get; set; }

        public bool IsScalar { get; set; }

        public bool IsLink { get; set; }

        public List<Property> Properties { get; set; } = new();

        // used for builder
        public string? BuiltName { get; set; }
    }
}
