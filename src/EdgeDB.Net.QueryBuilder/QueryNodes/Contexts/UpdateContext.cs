using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    /// <summary>
    ///     Represents context for a <see cref="UpdateNode"/>.
    /// </summary>
    internal class UpdateContext : NodeContext
    {
        /// <summary>
        ///     Gets the update factory used within the 'SET' statement.
        /// </summary>
        public LambdaExpression? UpdateExpression { get; init; }
        
        /// <inheritdoc/>
        public UpdateContext(Type currentType) : base(currentType)
        {
        }
    }
}
