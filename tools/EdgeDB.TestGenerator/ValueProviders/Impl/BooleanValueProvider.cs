using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class BooleanValueProvider : IValueProvider<bool>
    {
        public string EdgeDBName => "std::bool";

        public bool GetRandom(GenerationRuleSet rules) => rules.Random.Next() % 2 == 0;

        public string ToEdgeQLFormat(bool value) => value.ToString().ToLower();

        public override string ToString() => EdgeDBName;
    }
}
