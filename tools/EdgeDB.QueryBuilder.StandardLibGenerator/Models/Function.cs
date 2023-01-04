using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.StandardLibGenerator.Models
{
    [EdgeDBType(ModuleName = "schema")]
    internal class Function
    {
        [EdgeDBProperty("params")]
        public Parameter[]? Parameters { get; set; }
        public Type? ReturnType { get; set; }
        public Annotation[]? Annotations { get; set; }
        public string? Name { get; set; }
        [EdgeDBProperty("builtin")]
        public bool BuiltIn { get; set; }
        public bool Internal { get; set; }
        [EdgeDBProperty("return_typemod")]
        public TypeModifier ReturnTypeModifier { get; set; }
        public Volatility Volatility { get; set; }
        public string[]? ComputedFields { get; set; }
    }
}
