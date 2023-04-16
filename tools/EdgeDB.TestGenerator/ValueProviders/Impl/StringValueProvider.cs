using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class StringValueProvider : IValueProvider<string>
    {
        private static readonly Random _random = new Random();

        public string EdgeDBName => "std::str";

        public string GetRandom(GenerationRuleSet rules)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, _random.Next(rules.GetRange<StringValueProvider>()))
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        public string ToEdgeQLFormat(string value) => $"'{value}'";
        public override string ToString() => EdgeDBName;
    }
}
