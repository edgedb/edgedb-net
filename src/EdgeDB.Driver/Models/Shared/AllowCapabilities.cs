using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    [Flags]
    public enum AllowCapabilities : ulong // https://github.com/edgedb/rfcs/blob/master/text/1004-transactions-api.rst#edgedb-changes
    {
        ReadOnly = 1 << 0,
        Modifications = 1 << 1,
        SessionConfig = 1 << 2,
        Transaction = 1 << 3,
        DDL = 1 << 4,
        PersistantConfig = 1 << 5,

        All = Modifications | SessionConfig | Transaction | DDL | PersistantConfig
    }
}
