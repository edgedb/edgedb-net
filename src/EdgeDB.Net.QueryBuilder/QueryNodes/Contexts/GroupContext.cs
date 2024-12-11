using EdgeDB.Builders;
using System.Linq.Expressions;

namespace EdgeDB.QueryNodes;

internal class GroupContext : NodeContext
{
    public GroupContext(Type currentType) : base(currentType)
    {
    }

    public bool IncludeShape { get; set; } = true;
    public LambdaExpression? Selector { get; init; }
    public IShapeBuilder? Shape { get; init; }
}
