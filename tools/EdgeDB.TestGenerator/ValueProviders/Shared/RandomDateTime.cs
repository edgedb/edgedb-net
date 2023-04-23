using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.TestGenerator.ValueProviders.Shared
{
    internal static class RandomDateTime
    {
        private static readonly double _max = (new DateTime(9999, 12, 31) - new DateTime(1, 1, 1)).TotalMilliseconds;

        public static DateTime Next(Random random)
        {
            return new DateTime(1, 1, 1).AddMilliseconds(_max * random.NextDouble());
        }
    }
}
