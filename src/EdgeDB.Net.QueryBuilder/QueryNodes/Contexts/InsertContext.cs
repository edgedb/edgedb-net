using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    /// <summary>
    ///     Represents context for a <see cref="InsertNode"/>.
    /// </summary>
    internal class InsertContext : NodeContext
    {
        /// <summary>
        ///     Gets the value that is to be inserted.
        /// </summary>
        public Union<LambdaExpression, InsertNode.InsertValue, IJsonVariable>? Value { get; init; }

        /// <inheritdoc/>
        public InsertContext(Type currentType, object? value) : base(currentType)
        {
            Value = value is not null
                ? Union<LambdaExpression, InsertNode.InsertValue, IJsonVariable>.From(value, () => InsertNode.InsertValue.FromType(value))
                : null;
        }
    }
}
