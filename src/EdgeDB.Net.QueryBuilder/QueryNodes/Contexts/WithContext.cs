using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        public List<QueryGlobal>? Values { get; set; }

        public LambdaExpression? ValuesExpression { get; init; }

        /// <inheritdoc/>
        public WithContext(Type currentType) : base(currentType)
        {
        }
    }
}
