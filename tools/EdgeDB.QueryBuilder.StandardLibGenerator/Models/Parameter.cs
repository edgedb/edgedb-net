using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.StandardLibGenerator.Models
{
    public enum ParameterKind
    {
        VariadicParam,
        NamedOnlyParam,
        PositionalParam
    }

    [EdgeDBType(ModuleName = "schema")]
    internal class Parameter
    {
        public string? Name { get; set; }
        public string? Default { get; set; }
        public string[]? ComputedFields { get; set; }
        public ParameterKind Kind { get; set; }
        public Type? Type { get; set; }

        [EdgeDBProperty("num")]
        public long Index { get; set; }

        [EdgeDBProperty("typemod")]
        public TypeModifier TypeModifier { get; set; }

        [EdgeDBProperty("builtin")]
        public bool BuiltIn { get; set; }
    }
}
