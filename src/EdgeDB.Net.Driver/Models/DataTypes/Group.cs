using EdgeDB.DataTypes;
using EdgeDB.Serializer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents a group result returned from the <c>GROUP</c> expression.
    /// </summary>
    /// <typeparam name="TKey">The type of the key used to group the elements.</typeparam>
    /// <typeparam name="TElement">The type of the elements.</typeparam>
    public class Group<TKey, TElement> : IGrouping<TKey, TElement>
    {
        /// <summary>
        ///     Gets the key used to group the set of <see cref="Elements"/>.
        /// </summary>
        public TKey Key { get; }

        /// <summary>
        ///     Gets the name of the property that was grouped by.
        /// </summary>
        public IReadOnlyCollection<string> Grouping { get; }

        /// <summary>
        ///     Gets a collection of elements that have the same key as <see cref="Key"/>.
        /// </summary>
        public IReadOnlyCollection<TElement> Elements { get; }

        /// <summary>
        ///     Constructs a new grouping
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
            if (!raw.TryGetValue("key", out var keyValue) || !raw.TryGetValue("grouping", out var groupingValue) || !raw.TryGetValue("elements", out var elementsValue))
                throw new InvalidOperationException("The result data doesn't contain 'key', 'grouping', and or 'elements'");

            Grouping = ((string[])groupingValue!).ToImmutableArray();
            Key = BuildKey((IDictionary<string, object?>)keyValue!);
            Elements = ((IDictionary<string, object?>[])elementsValue!).Select(x => (TElement)TypeBuilder.BuildObject(typeof(TElement), x)!).ToImmutableArray();
        }

        private static TKey BuildKey(IDictionary<string, object?> value)
        {
            if (typeof(TKey).IsAssignableTo(typeof(ITuple)))
            {
                var types = typeof(TKey).GenericTypeArguments;
                var transientTuple = new TransientTuple(types, value.Values.ToArray());

                switch (typeof(TKey).Name.Split('`')[0])
                {
                    case "ValueTuple":
                        return (TKey)transientTuple.ToValueTuple();
                    case "Tuple":
                        return (TKey)transientTuple.ToReferenceTuple();
                    default:
                        throw new InvalidOperationException($"Cannot build tuple with the type of {typeof(TKey).Name}");
                }
            }
            else if (value.Count == 1)
                return (TKey)value.First().Value!;
            else
                throw new InvalidOperationException($"Cannot build key with the type of {typeof(TKey).Name}");
        }

        /// <inheritdoc/>
        public IEnumerator<TElement> GetEnumerator()
            => Elements.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
            => Elements.GetEnumerator();
    }
}
