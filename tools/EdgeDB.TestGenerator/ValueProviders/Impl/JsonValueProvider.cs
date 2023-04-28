using EdgeDB.DataTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class JsonValueProvider : IValueProvider<Json>
    {
        public string EdgeDBName => "std::json";

        public Json GetRandom(GenerationRuleSet rules)
        {
            // TODO: use value providers to generate scalar-based json objects
            var jsonRules = ValueGenerator.SmallJsonBlob.Clone();
            jsonRules.Seed = rules.Seed;
            jsonRules.RefreshRandom();
            return new Json(JsonConvert.SerializeObject(ValueGenerator.GenerateRandom(jsonRules).Value));
        }

        public string ToEdgeQLFormat(Json value) => $"std::to_json('{value.Value}')";
        public override string ToString() => EdgeDBName;
    }
}
