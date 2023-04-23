using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.TestGenerator
{
    internal static class EnumerableExtensions
    {
        public static T Random<T>(this IEnumerable<T> t, Random? rand = null)
        {
            rand ??= System.Random.Shared;

            var arr = t.ToArray();

            var i = rand.Next(arr.Length);

            return arr[i];
        }

        public static IEnumerable<T> RandomSequence<T>(this IEnumerable<T> t, Random? rand = null)
        {
            rand ??= System.Random.Shared;
            var c = t.Where(_ => rand.Next() % 2 == 0).OrderBy(x => rand.Next()).ToArray();
            return c.Length == 0 ? RandomSequence(t) : c;
        }

        public static IEnumerable<IEnumerable<T>> Roll<T>(this IEnumerable<T> collection)
        {
            var col = new List<IEnumerable<T>>();

            for (int i = 0; i != collection.Count(); i++)
            {
                if (i == 0)
                {
                    col.Add(collection);
                    continue;
                }

                col.Add(collection.Skip(i).Concat(collection.Take(i)));
            }

            return col;
        }

        public static IEnumerable<T> OrderRandomly<T>(this IEnumerable<T> enumerator, Random? rand = null)
        {
            rand ??= System.Random.Shared;
            return enumerator.OrderBy((_) => rand.Next());
        }
    }
}
