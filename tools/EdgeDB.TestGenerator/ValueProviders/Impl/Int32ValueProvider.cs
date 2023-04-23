using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class Int32ValueProvider : IValueProvider<int>
    {
        public string EdgeDBName => "std::int32";

        public int GetRandom(GenerationRuleSet rules) => rules.Random.Next(rules.GetRange<Int32ValueProvider>());
        public string ToEdgeQLFormat(int value) => $"<int32>{value}";
        public override string ToString() => EdgeDBName;
    }
}
