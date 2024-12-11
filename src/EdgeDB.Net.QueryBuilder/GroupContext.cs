namespace EdgeDB;

public abstract class GroupContext<TUsing, TContext> : IQueryContextUsing<TUsing> where TContext : IQueryContext
{
    public abstract TContext Context { get; }
    public abstract TUsing Using { get; }
}
