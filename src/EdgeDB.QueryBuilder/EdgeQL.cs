using EdgeDB.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public sealed partial class EdgeQL
    {
        [EquivalentOperator(typeof(VariablesReference))]
        public static object? Var(string name) => default;

        [EquivalentOperator(typeof(VariablesReference))]
        public static TType? Var<TType>(string name) => default;
    }
}
