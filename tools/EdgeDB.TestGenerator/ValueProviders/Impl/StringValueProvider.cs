using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class StringValueProvider : IValueProvider<string>
    {
        public static readonly StringValueProvider Shared = new();

        public string EdgeDBName => "std::str";

        public string GetRandom(GenerationRuleSet rules)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var str =  new string(Enumerable.Repeat(chars, rules.Random.Next(rules.GetRange<StringValueProvider>()))
                .Select(s => s[rules.Random.Next(s.Length)]).ToArray());

            if (EdgeQLFormatter.IsUnallowed(str))
                return GetRandom(rules);

            return str;
        }

        public string ToEdgeQLFormat(string value) => $"'{value}'";
        public override string ToString() => EdgeDBName;
    }
}
