using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class BytesValueProvider : IValueProvider<byte[]>
    {
        private static readonly string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

        public string EdgeDBName => "std::bytes";

        public byte[] GetRandom(GenerationRuleSet rules)
        {
            return Encoding.ASCII.GetBytes(Enumerable.Repeat(_chars, rules.Random.Next(rules.GetRange<BytesValueProvider>()))
                .Select(s => s[rules.Random.Next(s.Length)]).ToArray());
        }

        public override string ToString() => EdgeDBName;

        public string ToEdgeQLFormat(byte[] value) => $"b'{Encoding.ASCII.GetString(value)}'";
    }
}
