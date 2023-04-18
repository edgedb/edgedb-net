using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.TestGenerator
{
    internal static class EnumerableExtensions
    {
        private static readonly Random _rand = new();

        public static T Random<T>(this IEnumerable<T> t)
        {
            var arr = t.ToArray();

            var i = _rand.Next(arr.Length);

            return arr[i];
        }

        public static IEnumerable<T> RandomSequence<T>(this IEnumerable<T> t)
        {
            var c = t.Where(_ => _rand.Next() % 2 == 0).OrderBy(x => _rand.Next()).ToArray();
            return c.Length == 0 ? RandomSequence(t) : c;
        }

        public static IEnumerable<IEnumerable<T>> Roll<T>(this IEnumerable<T> collection)
        {
            for(int i = 0; i != collection.Count(); i++)
            {
                if (i == 0)
                {
                    yield return collection;
                    continue;
                }

                yield return collection.Skip(i).Concat(collection.Take(i));
            }
        }

        public static IEnumerable<T> OrderRandomly<T>(this IEnumerable<T> enumerator)
        {
            return enumerator.OrderBy((_) => _rand.Next());
        }
    }
}
