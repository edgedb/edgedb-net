using EdgeDB.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    /// <summary>
    ///     Represents the context for a <see cref="SelectNode"/>.
    /// </summary>
    internal class SelectContext : NodeContext
    {
        /// <summary>
        ///     Gets the shape of the select statement.
        /// </summary>
        public IShapeBuilder? Shape { get; init; }

        /// <summary>
        ///     Gets the expression of the select statement.
        /// </summary>
        public LambdaExpression? Expression { get; init; }

        /// <summary>
        ///     Gets or sets the name that is to be selected.
        /// </summary>
        public string? SelectName { get; set; }

        /// <summary>
        ///     Gets whether or not the select statement is selecting a free object.
        /// </summary>
        public bool IsFreeObject { get; init; }
        
        public bool IncludeShape { get; set; } = true;

        public SelectContext(Type currentType) : base(currentType)
        {
        }
    }
}
