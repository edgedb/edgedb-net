using System;
using System.Collections.Generic;
using System.Linq;
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
        public object? Value { get; init; }

        /// <inheritdoc/>
        public InsertContext(Type currentType) : base(currentType)
        {
        }
    }
}
