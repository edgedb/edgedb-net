namespace EdgeDB;

internal interface IReducer
{
    void Reduce(IQueryBuilder builder, QueryWriter writer);
}
