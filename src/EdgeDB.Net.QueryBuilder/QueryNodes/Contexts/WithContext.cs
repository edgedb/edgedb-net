using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    /// <summary>
    ///     Represents context for a <see cref="WithContext"/>.
    /// </summary>
    internal class WithContext : NodeContext
    {
        /// <summary>
        ///     Gets the global variables that are included in the 'WITH' statement.
        /// </summary>
        public List<QueryGlobal>? Values { get; init; }

        /// <inheritdoc/>
        public WithContext(Type currentType) : base(currentType)
        {
        }
    }
}
