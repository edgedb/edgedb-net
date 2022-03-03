using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public struct BuiltQuery
    {
        public string QueryText { get; set; }
        public IDictionary<string, object?> Parameters { get; set; }
    }
}
