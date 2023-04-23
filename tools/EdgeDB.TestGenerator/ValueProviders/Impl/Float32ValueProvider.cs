using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class Float32ValueProvider : IValueProvider<float>
    {
        public string EdgeDBName => "std::float32";

        public float GetRandom(GenerationRuleSet rules)
        {
            var r = rules.Random.NextSingle() * rules.Random.Next(rules.GetRange<Float32ValueProvider>());

            if (rules.Random.Next() % 2 == 0)
                r *= -1;

            return r;
        }

        public string ToEdgeQLFormat(float value) => $"<float32>{value}n";
        public override string ToString() => EdgeDBName;
    }
}
