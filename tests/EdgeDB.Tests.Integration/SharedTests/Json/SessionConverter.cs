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
            var config = ReadConfig((JObject)obj["config"]!);
            var globals = new Dictionary<string, object>();

            foreach(var global in (JObject)obj["globals"]!)
            {
                var valueNode = ResultNodeConverter.Instance.ReadJson(global.Value!.CreateReader(), typeof(IResultNode), null, serializer);
                var value = ResultTypeBuilder.ToObject((IResultNode)valueNode!);
                globals.Add(global.Key, value);
            }

            return new Session
            {
                Aliases = aliases!,
                Config = config,
                Module = module!,
                Globals = globals!
            };
        }

        private State.Config ReadConfig(JObject configObject)
        {
            return new Config
            {
                AllowDMLInFunctions = configObject.TryGetValue("allow_dml_in_functions", out var allowDML)
                    ? (bool)allowDML
                    : null,
                ApplyAccessPolicies = configObject.TryGetValue("apply_access_policies", out var applyAccessPolicies)
                    ? (bool)applyAccessPolicies
                    : null,
                DDLPolicy = configObject.TryGetValue("ddl_policy", out var ddlPolicy)
                    ? ddlPolicy.ToObject<DDLPolicy>()
                    : null,
                IdleTransationTimeout = configObject.TryGetValue("idle_transaction_timeout", out var idleTransactionTimeout)
                    ? idleTransactionTimeout.ToObject<TimeSpan>()
                    : null,
                QueryExecutionTimeout = configObject.TryGetValue("query_execution_timeout", out var queryTimeout)
                    ? queryTimeout.ToObject<TimeSpan>()
                    : null
            };
        }

        public override void WriteJson(JsonWriter writer, Session? value, JsonSerializer serializer) => throw new NotImplementedException();
    }
}
