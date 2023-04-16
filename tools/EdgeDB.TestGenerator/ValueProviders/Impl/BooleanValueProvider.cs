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
        private static readonly Random _random = new Random();

        public string EdgeDBName => "std::bool";

        public bool GetRandom(GenerationRuleSet rules) => _random.Next() % 2 == 0;

        public string ToEdgeQLFormat(bool value) => value.ToString().ToLower();
    }
}
