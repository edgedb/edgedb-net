using EdgeDB.TestGenerator.ValueProviders.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class LocalDateTimeValueProvider : IValueProvider<DataTypes.LocalDateTime>
    {
        public string EdgeDBName => "cal::local_datetime";

        public DataTypes.LocalDateTime GetRandom(GenerationRuleSet rules)
            => new(RandomDateTime.Next(rules.Random));

        public string ToEdgeQLFormat(DataTypes.LocalDateTime value)
            => $"<cal::local_datetime>'{value.DateTime:yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFF}'";

        public override string ToString() => EdgeDBName;
    }
}
