using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    internal class GroupContext : NodeContext
    {
        public Expression? PropertyExpression { get; init; }
        public Expression? BuilderExpression { get; init; }
        public GroupContext(Type currentType) : base(currentType)
        {
        }
    }
}
