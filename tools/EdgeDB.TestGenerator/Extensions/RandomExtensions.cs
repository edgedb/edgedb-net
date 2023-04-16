using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.TestGenerator
{
    internal static class RandomExtensions
    {
        public static int Next(this Random random, Range range)
        {
            return random.Next(range.Start.Value, range.End.Value);
        }
    }
}
