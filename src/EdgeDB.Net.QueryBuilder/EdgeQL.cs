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
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>count(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.SetsCount))]
        public static long Count<TType>(EdgeDB.Interfaces.IMultiCardinalityExecutable<TType> a) { return default!; }


        public static JsonReferenceVariable<T> AsJson<T>(T value)
            => new JsonReferenceVariable<T>(value);

        [EquivalentOperator(typeof(EdgeDB.Operators.LinksAddLink))]
        public static TType[] AddLink<TType>(IQuery<TType> element)
            => default!;

        [EquivalentOperator(typeof(EdgeDB.Operators.LinksAddLink))]
        public static TType[] AddLinkRef<TType>(TType element)
            => default!;

        [EquivalentOperator(typeof(EdgeDB.Operators.LinksAddLink))]
        public static TSource AddLink<TSource, TType>(TType element)
            where TSource : IEnumerable<TType>
            => default!;

        [EquivalentOperator(typeof(EdgeDB.Operators.LinksAddLink))]
        public static TSource AddLinkRef<TSource, TType>(IQuery<TType> element)
            where TSource : IEnumerable<TType>
            => default!;

        [EquivalentOperator(typeof(EdgeDB.Operators.LinksRemoveLink))]
        public static TType[] RemoveLinkRef<TType>(TType element)
            => default!;

        [EquivalentOperator(typeof(EdgeDB.Operators.LinksRemoveLink))]
        public static TType[] RemoveLink<TType>(IQuery<TType> element)
            => default!;

        [EquivalentOperator(typeof(EdgeDB.Operators.LinksRemoveLink))]
        public static TSource RemoveLinkRef<TSource, TType>(TType element)
            where TSource : IEnumerable<TType>
            => default!;

        [EquivalentOperator(typeof(EdgeDB.Operators.LinksRemoveLink))]
        public static TSource RemoveLink<TSource, TType>(IQuery<TType> element)
            where TSource : IEnumerable<TType>
            => default!;
    }
}
