using EdgeDB.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Integration.SharedTests.Json
{
    internal sealed class SessionConverter : JsonConverter<EdgeDB.State.Session>
    {
        public override Session? ReadJson(JsonReader reader, Type objectType, Session? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);

            var module = (string?)obj["module"];
            var aliases = ((JObject)obj["aliases"]!).ToObject<Dictionary<string, string>>();
            var config = ReadConfig((JObject)obj["config"]);
            var globals = 
        }

        private State.Config ReadConfig(JObject configObject)
        {
            return new Config
            {
                AllowDMLInFunctions = (bool)configObject["allow_dml_in_functions"]!,
                ApplyAccessPolicies = (bool)configObject["apply_access_policies"]!,
                DDLPolicy = configObject["ddl_policy"]!.ToObject<DDLPolicy>(),
                IdleTransationTimeout = configObject["idle_transaction_timeout"]!.ToObject<TimeSpan>(),
                QueryExecutionTimeout = configObject["query_execution_timeout"]!.ToObject<TimeSpan>(),
            };
        }

        public override void WriteJson(JsonWriter writer, Session? value, JsonSerializer serializer) => throw new NotImplementedException();
    }
}
