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
        #region General
        #region Length
        [EquivalentOperator(typeof(Len))]
        public static long Length(string value) { return default!; }

        [EquivalentOperator(typeof(Len))]
        public static long Length<TType>(IEnumerable<TType> value) { return default!; }
        #endregion

        #region Contains
        [EquivalentOperator(typeof(Contains))]
        public static bool Contains<TType>(IEnumerable<TType> collection, TType value) { return default!; }
        #endregion

        #region Find/IndexOf
        [EquivalentOperator(typeof(Find))]
        public static long Find<TType>(IEnumerable<TType> collection, TType value) { return default!; }

        [EquivalentOperator(typeof(Find))]
        public static long IndexOf<TType>(IEnumerable<TType> collection, TType value) { return default!; }
        #endregion

        #region Indexing/ElementAt
        [EquivalentOperator(typeof(Operators.Index))]
        public static TType Index<TType>(IEnumerable<TType> collection, long index) { return default!; }

        [EquivalentOperator(typeof(Operators.Index))]
        public static TType ElementAt<TType>(IEnumerable<TType> collection, long index) { return default!; }
        #endregion

        #region Slice/Substring
        [EquivalentOperator(typeof(Slice))]
        public static IEnumerable<TType> Slice<TType>(IEnumerable<TType> collection, long startIndex) { return default!; }

        [EquivalentOperator(typeof(Slice))]
        public static IEnumerable<TType> Slice<TType>(IEnumerable<TType> collection, long startIndex, long endIndex) { return default!; }

        [EquivalentOperator(typeof(Slice))]
        public static string SubString(string value, long startIndex) { return default!; }

        [EquivalentOperator(typeof(Slice))]
        public static string SubString(string value, long startIndex, long endIndex) { return default!; }
        #endregion

        #region Concat
        [EquivalentOperator(typeof(Concat))]
        public static string Concat(string a, string b) { return default!; }

        [EquivalentOperator(typeof(Concat))]
        public static IEnumerable<TType> Concat<TType>(IEnumerable<TType> a, IEnumerable<TType> b) { return default!; }
        #endregion

        #region Like/ILike
        [EquivalentOperator(typeof(Like))]
        public static bool Like(string a, string b) { return default!; }

        [EquivalentOperator(typeof(ILike))]
        public static bool ILike(string a, string b) { return default!; }
        #endregion

        #endregion General
    }
}
