using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    /// <summary>
    ///     Represents the context for a <see cref="DeleteNode"/>.
    /// </summary>
    internal class DeleteContext : SelectContext
    {
        /// <inheritdoc/>
        public DeleteContext(Type currentType) : base(currentType)
        {
        }
    }
}
