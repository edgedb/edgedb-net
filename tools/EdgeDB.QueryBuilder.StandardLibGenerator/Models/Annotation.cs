using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.StandardLibGenerator.Models
{
    [EdgeDBType(ModuleName = "schema")]
    internal class Annotation
    {
        public Annotation[]? Annotations { get; set; }
        public string? Name { get; set; }
        public bool Internal { get; set; }
        public bool Inheritable { get; set; }
        [EdgeDBProperty("builtin")]
        public bool BuiltIn { get; set; }
        [EdgeDBProperty(IsLinkProperty = true)]
        public string? Value { get; set; }
    }
}
