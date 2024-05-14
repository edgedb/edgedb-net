namespace EdgeDB;

internal sealed class NestedSelectReducer : IReducer
{
    /// <summary>
    ///     Reduces sub-query selects if grammatical rules allow it.<br/>
    ///     An example of this would be the following:
    ///     <c>
    ///     select (select Person)
    ///     </c>
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="writer"></param>
    /// <param name="shouldRunAfter"></param>
    public void Reduce(IQueryBuilder builder, QueryWriter writer, Queue<IReducer> shouldRunAfter)
    {

    }
}
