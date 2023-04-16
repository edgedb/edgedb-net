using EdgeDB.DataTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class JsonValueProvider : IValueProvider<Json>
    {
        public string EdgeDBName => "std::json";

        public Json GetRandom(GenerationRuleSet rules)
        {
            // TODO: use value providers to generate scalar-based json objects
            return new Json(JsonConvert.SerializeObject(GetRandom<ObjectValueProvider>(SmallJsonBlob).Value));
        }

        public string ToEdgeQLFormat(Json value) => $"std::to_json('{value.Value}')";
        public override string ToString() => EdgeDBName;
    }
}
