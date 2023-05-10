using EdgeDB.TestGenerator.ValueProviders.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class LocalDateValueProvider : IValueProvider<DataTypes.LocalDate>
    {
        public string EdgeDBName => "cal::local_date";

        public DataTypes.LocalDate GetRandom(GenerationRuleSet rules)
            => DateOnly.FromDateTime(RandomDateTime.Next(rules.Random));

        public string ToEdgeQLFormat(DataTypes.LocalDate value)
            => $"<cal::local_date>'{value.DateOnly:yyyy-MM-dd}'";

        public override string ToString() => EdgeDBName;
    }
}
