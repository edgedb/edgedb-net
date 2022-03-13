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
        public IEnumerable<KeyValuePair<string, object?>> Parameters { get; set; } = new Dictionary<string, object?>();
    }
}
