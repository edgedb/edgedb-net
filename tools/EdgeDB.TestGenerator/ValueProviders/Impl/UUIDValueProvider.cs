using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class UUIDValueProvider : IValueProvider<Guid>
    {
        public string EdgeDBName => "std::uuid";

        public unsafe Guid GetRandom(GenerationRuleSet rules)
        {
            Guid id;

            rules.Random.NextBytes(new Span<byte>(&id, sizeof(Guid)));

            return id;
        }
        public string ToEdgeQLFormat(Guid value) => $"<uuid>'{value}'";
        public override string ToString() => EdgeDBName;
    }
}
