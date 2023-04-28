using EdgeDB.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace EdgeDB.TestGenerator
{
    public class SessionConfigConfiguration
    {
        [YamlMember(Alias = "session_idle_transaction_timeout")]
        public TimeSpan? IdleTransationTimeout { get; set; }

        [YamlMember(Alias = "query_execution_timeout")]
        public TimeSpan? QueryExecutionTimeout { get; set; }

        [YamlMember(Alias = "allow_dml_in_functions")]
        public bool? AllowDMLInFunctions { get; set; }

        [YamlMember(Alias = "allow_bare_ddl")]
        public DDLPolicy? DDLPolicy { get; set; }

        [YamlMember(Alias = "apply_access_policies")]
        public bool? ApplyAccessPolicies { get; set; }
    }
}
