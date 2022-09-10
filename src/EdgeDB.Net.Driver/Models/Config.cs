using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.State
{
    public enum DDLPolicy
    {
        AlwaysAllow,
        NeverAllow,
    }

    public class Config
    {
        public TimeSpan IdleTransationTimeout { get; init; }
        public TimeSpan QueryExecutionTimeout { get; init; }
        public bool AllowDMLInFunctions { get; init; }
        public DDLPolicy AllowBareDDL { get; init; }
        public bool ApplyAccessPolicies { get; init; }

        internal Config()
        {
            IdleTransationTimeout = TimeSpan.FromSeconds(10);
            QueryExecutionTimeout = TimeSpan.Zero;
            AllowDMLInFunctions = false;
            AllowBareDDL = DDLPolicy.AlwaysAllow;
            ApplyAccessPolicies = true;
        }

        internal IDictionary<string, object?> Serialize()
        {
            var dict = new Dictionary<string, object?>();

            if(IdleTransationTimeout.TotalSeconds != 10)
                dict["idle_transaction_timeout"] = IdleTransationTimeout;

            if (QueryExecutionTimeout != TimeSpan.Zero)
                dict["query_execution_timeout"] = QueryExecutionTimeout;

            if(AllowDMLInFunctions)
                dict["allow_dml_in_functions"] = true;

            if (AllowBareDDL != DDLPolicy.AlwaysAllow)
                dict["allow_bare_ddl"] = AllowBareDDL;

            if (!ApplyAccessPolicies)
                dict["apply_access_policies"] = false;
            
            return dict;
        }

        internal Config Clone()
            => (Config)MemberwiseClone();

        public static Config Default
            => new();
    }

    public sealed class ConfigProperties
    {
        public Optional<TimeSpan> IdleTransationTimeout { get; set; }
        public Optional<TimeSpan> QueryExecutionTimeout { get; set; }
        public Optional<bool> AllowDMLInFunctions { get; set; }
        public Optional<DDLPolicy> AllowBareDDL { get; set; }
        public Optional<bool> ApplyAccessPolicies { get; set; }

        internal Config ToConfig(Config old)
        {
            return new Config
            {
                AllowBareDDL = AllowBareDDL.GetValueOrDefault(old.AllowBareDDL),
                AllowDMLInFunctions = AllowDMLInFunctions.GetValueOrDefault(old.AllowDMLInFunctions),
                ApplyAccessPolicies = ApplyAccessPolicies.GetValueOrDefault(old.ApplyAccessPolicies),
                IdleTransationTimeout = IdleTransationTimeout.GetValueOrDefault(old.IdleTransationTimeout),
                QueryExecutionTimeout = QueryExecutionTimeout.GetValueOrDefault(old.QueryExecutionTimeout)
            };
        }
    }
}
