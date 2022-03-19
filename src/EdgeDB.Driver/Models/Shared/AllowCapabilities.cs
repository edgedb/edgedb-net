using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    /// <summary>
    ///     Represents a bitfield of capabilities used when executing commands.
    /// </summary>
    [Flags]
    public enum AllowCapabilities : ulong
    {
        /// <summary>
        ///     The default value for capabilities.
        /// </summary>
        ReadOnly = 1 << 0,

        /// <summary>
        ///     The query is not read only.
        /// </summary>
        Modifications = 1 << 1,

        /// <summary>
        ///     The query contains session config changes.
        /// </summary>
        SessionConfig = 1 << 2,

        /// <summary>
        ///     The query contains transaction manipulations.
        /// </summary>
        Transaction = 1 << 3,

        /// <summary>
        ///     The query contains DDL.
        /// </summary>
        DDL = 1 << 4,

        /// <summary>
        ///     The command changes serve ror database configs.
        /// </summary>
        PersistantConfig = 1 << 5,

        /// <summary>
        ///     Represents all capabilities except <see cref="ReadOnly"/>
        /// </summary>
        All = Modifications | SessionConfig | Transaction | DDL | PersistantConfig
    }
}
