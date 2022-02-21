using EdgeDB.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public static class EdgeQL
    {
        [EquivalentOperator(typeof(Len))]
        public static long Length(string value) { return 0; }

        [EquivalentOperator(typeof(Len))]
        public static long Length<TType>(IEnumerable<TType> value) { return 0; }

    }
}
