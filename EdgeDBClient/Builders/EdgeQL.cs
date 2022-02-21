using EdgeDB.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public sealed class EdgeQL
    {
        [EquivalentOperator(typeof(Len))]
        public static long Length(string value) { return 0; }

        [EquivalentOperator(typeof(Len))]
        public static long Length<TType>(IEnumerable<TType> value) { return 0; }

        [EquivalentOperator(typeof(Contains))]
        public static bool Contains<TType>(IEnumerable<TType> collection, TType value) { return true; }

        [EquivalentOperator(typeof(Find))]
        public static long Find<TType>(IEnumerable<TType> collection, TType value) { return 0; }

        [EquivalentOperator(typeof(Find))]
        public static long IndexOf<TType>(IEnumerable<TType> collection, TType value) { return 0; }

        [EquivalentOperator(typeof(Operators.Index))]
        public static TType Index<TType>(IEnumerable<TType> collection, long index) { return default!; }

        [EquivalentOperator(typeof(Operators.Index))]
        public static TType ElementAt<TType>(IEnumerable<TType> collection, long index) { return default!; }
    }
}
