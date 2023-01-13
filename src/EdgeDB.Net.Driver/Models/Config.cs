using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.State
{
    /// <summary>
    ///     Represents a DDL policy.
    /// </summary>
    public enum DDLPolicy
    {
        /// <summary>
        ///     Always allow DDL.
        /// </summary>
        AlwaysAllow,

        /// <summary>
        ///     Never allow DDL.
        /// </summary>
        NeverAllow,
    }

    /// <summary>
    ///     Represents a session-level config.
    /// </summary>
    public sealed class Config
    {
        /// <summary>
        ///     Gets the idle transation timeout duration.
        /// </summary>
        public TimeSpan? IdleTransationTimeout { get; init; }

        /// <summary>
        ///     Gets the query execution timeout duration.
        /// </summary>
        public TimeSpan? QueryExecutionTimeout { get; init; }

        /// <summary>
        ///     Gets whether or not to allow data maniplulations in edgeql functions.
        /// </summary>
        public bool? AllowDMLInFunctions { get; init; }

        /// <summary>
        ///     Gets the data definition policy for this client.
        /// </summary>
        public DDLPolicy? DDLPolicy { get; init; }

        /// <summary>
        ///     Gets whether or not to apply the access policy.
        /// </summary>
        public bool? ApplyAccessPolicies { get; init; }

        internal Config() { }

        internal IDictionary<string, object?> Serialize()
        {
            var dict = new Dictionary<string, object?>();

            if(IdleTransationTimeout.HasValue)
                dict["idle_transaction_timeout"] = new DataTypes.Duration(IdleTransationTimeout.Value);

            if (QueryExecutionTimeout.HasValue)
                dict["query_execution_timeout"] = new DataTypes.Duration(QueryExecutionTimeout.Value);

            if(AllowDMLInFunctions.HasValue)
                dict["allow_dml_in_functions"] = AllowDMLInFunctions.Value;

            if(DDLPolicy.HasValue)
                dict["allow_bare_ddl"] = DDLPolicy.Value.ToString();

            if (ApplyAccessPolicies.HasValue)
                dict["apply_access_policies"] = ApplyAccessPolicies.Value;
            
            return dict;
        }

        internal Config Clone()
            => (Config)MemberwiseClone();

        /// <summary>
        ///     Gets the default config.
        /// </summary>
        public static Config Default
            => new();
    }

    /// <summary>
    ///     Represents properties used to modify a <see cref="Config"/>.
    /// </summary>
    public sealed class ConfigProperties
    {
        /// <summary>
        ///     Gets or sets the idle transation timeout duration.
        /// </summary>
        public Optional<TimeSpan> IdleTransationTimeout { get; set; }

        /// <summary>
        ///     Gets or sets the query execution timeout duration.
        /// </summary>
        public Optional<TimeSpan> QueryExecutionTimeout { get; set; }

        /// <summary>
        ///     Gets or sets whether or not to allow data maniplulations in edgeql functions.
        /// </summary>
        public Optional<bool> AllowDMLInFunctions { get; set; }

        /// <summary>
        ///     Gets or sets the data definition policy for this client.
        /// </summary>
        public Optional<DDLPolicy> DDLPolicy { get; set; }

        /// <summary>
        ///     Gets or sets whether or not to apply the access policy.
        /// </summary>
        public Optional<bool> ApplyAccessPolicies { get; set; }

        internal Config ToConfig(Config old)
        {
            return new Config
            {
                DDLPolicy = DDLPolicy.GetValueOrDefault(old.DDLPolicy),
                AllowDMLInFunctions = AllowDMLInFunctions.GetValueOrDefault(old.AllowDMLInFunctions),
                ApplyAccessPolicies = ApplyAccessPolicies.GetValueOrDefault(old.ApplyAccessPolicies),
                IdleTransationTimeout = IdleTransationTimeout.GetValueOrDefault(old.IdleTransationTimeout),
                QueryExecutionTimeout = QueryExecutionTimeout.GetValueOrDefault(old.QueryExecutionTimeout)
            };
        }
    }
}
