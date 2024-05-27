namespace EdgeDB.Interfaces;

/// <summary>
///     Represents a query with a cardinality of <see cref="Cardinality.AtMostOne" />.
/// </summary>
/// <typeparam name="TType">The result type of the query.</typeparam>
public interface ISingleCardinalityQuery<TType> : IQuery<TType>
{
}
