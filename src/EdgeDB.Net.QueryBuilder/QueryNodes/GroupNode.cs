using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    internal class GroupNode : QueryNode<GroupContext>
    {
        public GroupNode(NodeBuilder builder) : base(builder)
        {
        }

        public override void FinalizeQuery(QueryStringWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
