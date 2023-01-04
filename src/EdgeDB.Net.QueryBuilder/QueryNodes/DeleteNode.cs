using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    /// <summary>
    ///     Represents a 'DELETE' node
    /// </summary>
    internal class DeleteNode : SelectNode
    {
        /// <inheritdoc/>
        public DeleteNode(NodeBuilder builder) : base(builder)
        {
        }

        /// <inheritdoc/>
        /// <remarks>
        ///     Overrides the default <see cref="SelectNode.FinalizeQuery"/> method and does nothing.
        /// </remarks>
        public override void FinalizeQuery() { }
        
        /// <inheritdoc/>
        public override void Visit()
        {
            Query.Append($"delete {Context.SelectName ?? OperatingType.GetEdgeDBTypeName()}");
        }
    }
}
