namespace EdgeDB;

public abstract class GroupContext<TUsing, TContext> : IQueryContextUsing<TUsing> where TContext : IQueryContext
{
    public abstract TUsing Using { get; }
    public abstract TContext Context { get; }
}
