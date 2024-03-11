using EdgeDB.Builders;
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
        public bool IncludeShape { get; set; } = true;
        public LambdaExpression? Selector { get; init; }
        public IShapeBuilder? Shape { get; init; }

        public GroupContext(Type currentType) : base(currentType)
        {

        }
    }
}
