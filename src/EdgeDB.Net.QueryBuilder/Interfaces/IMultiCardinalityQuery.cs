namespace EdgeDB.Interfaces;

/// <summary>
///     Represents a query with a cardinality of <see cref="Cardinality.Many" />.
/// </summary>
/// <typeparam name="TType">The result type of the query.</typeparam>
public interface IMultiCardinalityQuery<TType> : IQuery<TType>
{
}
