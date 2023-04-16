using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class Int16ValueProvider : IValueProvider<short>
    {
        private static readonly Random _random = new Random();

        public string EdgeDBName => "std::int16";

        public short GetRandom(GenerationRuleSet rules) => (short)_random.Next(rules.GetRange<Int16ValueProvider>());
        public string ToEdgeQLFormat(short value) => $"<int16>{value}";
        public override string ToString() => EdgeDBName;
    }
}
