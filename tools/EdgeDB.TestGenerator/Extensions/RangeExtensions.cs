using System;
namespace EdgeDB.TestGenerator
{
    public static class RangeExtensions
    {
        public static int Average(this Range range)
        {
            return (range.Start.Value + range.End.Value) / 2;
        }
    }
}

