using EdgeDB.Binary;
using EdgeDB.DataTypes;
using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace EdgeDB;

/// <summary>
///     Represents a group result returned from the <c>GROUP</c> expression.
/// </summary>
/// <typeparam name="TKey">The type of the key used to group the elements.</typeparam>
/// <typeparam name="TElement">The type of the elements.</typeparam>
public sealed class Group<TKey, TElement> : IGrouping<TKey, TElement>
{
    static Group()
    {
        // precache the deserializer, in case theres issues with it, this will throw before the query is run.
        if(TypeBuilder.IsValidObjectType(typeof(TElement)))
            TypeBuilder.GetDeserializationFactory(typeof(TElement));
    }
    /// <summary>
    ///     Gets the key used to group the set of <see cref="Elements" />.
    /// </summary>
    [EdgeDBIgnore]
    public TKey Key { get; }

    /// <summary>
    ///     Gets a collection of all the names of the parameters used as the key for this particular subset.
    /// </summary>
    [EdgeDBProperty("grouping")]
    public IReadOnlyCollection<string> Grouping { get; }

    /// <summary>
    ///     Gets a collection of elements that have the same key as <see cref="Key" />.
    /// </summary>
    [EdgeDBProperty("elements")]
    public IReadOnlyCollection<TElement> Elements { get; }

    /// <summary>
    ///     Constructs a new grouping.
    /// </summary>
    /// <param name="key">The key that each element share.</param>
    /// <param name="groupedBy">The property used to group the elements.</param>
    /// <param name="elements">The collection of elements that have the specified key.</param>
    public Group(TKey key, IEnumerable<string> groupedBy, IEnumerable<TElement> elements)
    {
        Key = key;
        Grouping = groupedBy.ToImmutableArray();
        Elements = elements.ToImmutableArray();
    }

    [EdgeDBDeserializer]
    internal Group(IDictionary<string, object?> raw)
    {
        if (!raw.TryGetValue("key", out var keyValue) || !raw.TryGetValue("grouping", out var groupingValue) ||
            !raw.TryGetValue("elements", out var elementsValue))
            throw new InvalidOperationException("The result data doesn't contain 'key', 'grouping', and or 'elements'");

        Grouping = ((string[])groupingValue!).ToImmutableArray();
        Key = BuildKey((IDictionary<string, object?>)keyValue!);
        Elements = elementsValue is null
            ? Array.Empty<TElement>()
            : ((object?[])elementsValue).Cast<TElement>().ToImmutableArray();
    }

    /// <inheritdoc />
    public IEnumerator<TElement> GetEnumerator()
        => Elements.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
        => Elements.GetEnumerator();

    private static TKey BuildKey(object? value)
    {
        switch (value)
        {
            case TKey key:
                return key;
            case IDictionary<string, object?> {Count: 2} dict when dict.ContainsKey("id"):
            {
                var keyRaw = dict.FirstOrDefault(x => x.Key != "id");
                if (keyRaw.Value is TKey keyValue)
                    return keyValue;

                throw new InvalidOperationException($"Key of grouping cannot be extracted from {keyRaw.Value?.GetType()}");
            }
            default:
                throw new InvalidOperationException($"Cannot build key with the type of {typeof(TKey).Name}");
        }
    }
}
