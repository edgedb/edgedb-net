using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    /// <summary>
    ///     Represents context for a <see cref="ForNode"/>.
    /// </summary>
    internal class ForContext : NodeContext
    {
        /// <summary>
        ///     Gets the iteration expression used to build the 'UNION (...)' statement.
        /// </summary>
        public LambdaExpression? Expression { get; init; }

        /// <summary>
        ///     Gets the collection used within the 'FOR' statement.
        /// </summary>
        public IEnumerable? Set { get; init; }

        /// <inheritdoc/>
        public ForContext(Type currentType) : base(currentType)
        {
        }
    }
}
