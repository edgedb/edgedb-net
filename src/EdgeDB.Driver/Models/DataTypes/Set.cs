using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DataTypes
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/edgeql/sets#sets">EdgeDB Set</see>.
    /// </summary>
    /// <typeparam name="T">The inner type of the set</typeparam>
    public sealed class Set<T> : IEnumerable<T>, ICollection<T>, ISet
    {
        /// <summary>
        ///     Gets a value at a specific index within the set.
        /// </summary>
        /// <param name="index">The index of the value to retrive.</param>
        /// <returns>The value at the specified index.</returns>
        public T this[int index]
        {
            get => _collection[index];
            set
            {
                if (IsReadOnly)
                    throw new InvalidOperationException("Cannot modify a read onyl set");

                _collection[index] = value;
            }
        }

        /// <inheritdoc cref="ISet.Count"/>
        public int Count => _collection.Count;

        /// <inheritdoc cref="ISet.IsReadOnly"/>
        public bool IsReadOnly { get; private set; }

        internal bool IsSubQuery { get; } = false;
        internal object? QueryBuilder { get; }

        private readonly List<T> _collection;

        /// <summary>
        ///     Creates a new set with no elements.
        /// </summary>
        public Set() 
        {
            IsReadOnly = false;
            _collection = new();
        }

        /// <summary>
        ///     Creates a new set.
        /// </summary>
        /// <param name="collection">The elements to add to the newly created set.</param>
        /// <param name="readOnly">Whether or not this set is read only.</param>
        public Set(IEnumerable<T> collection, bool readOnly) 
        {
            _collection = new(collection);
            IsReadOnly = readOnly;
        }

        /// <summary>
        ///     Creates a new set with the specified inital capacity
        /// </summary>
        /// <param name="capacity">The initial number of items within the set.</param>
        public Set(int capacity)
        {
            _collection = new(capacity);
            IsReadOnly = false;
        }

        internal Set(object? queryBuilder)
            : this()
        {
            QueryBuilder = queryBuilder;
            IsSubQuery = true;
        }

        /// <summary>
        ///     Gets an enumerator for this set.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
            => _collection.GetEnumerator();

        /// <summary>
        ///     Gets an enumerator for this set.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
            => _collection.GetEnumerator();

        /// <summary>
        ///     Adds an item to this set.
        /// </summary>
        /// <param name="item">The item to add to the set.</param>
        /// <exception cref="InvalidOperationException">Cannot add to a read only set</exception>
        public void Add(T item)
        {
            if (IsReadOnly)
                throw new InvalidOperationException("Cannot add to a read only set");

            _collection.Add(item);
        }

        /// <summary>
        ///     Adds a collection of items to this set.
        /// </summary>
        /// <param name="items">The items to add to this set.</param>
        /// <exception cref="InvalidOperationException">Cannot add to a read only set</exception>
        public void AddRange(IEnumerable<T> items)
        {
            if (IsReadOnly)
                throw new InvalidOperationException("Cannot add to a read only set");

            _collection.AddRange(items);
        }

        /// <summary>
        ///     Clears everything within this set.
        /// </summary>
        /// <exception cref="InvalidOperationException">Cannot clear a read only set</exception>
        public void Clear()
        {
            if (IsReadOnly)
                throw new InvalidOperationException("Cannot clear a read only set");

            _collection.Clear();
        }

        /// <summary>
        ///     Checks whether or not the set contains an item.
        /// </summary>
        /// <param name="item">The item to check for.</param>
        /// <returns>
        ///     <see langword="true"/> if the set contains the specified item; otherwise <see langword="false"/>.
        /// </returns>
        public bool Contains(T item)
            => _collection.Contains(item);

        /// <summary>
        ///     Copies this set to an array
        /// </summary>
        /// <param name="array">The destination array to copy to.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(T[] array, int arrayIndex)
            => _collection.CopyTo(array, arrayIndex);

        /// <summary>
        ///     Removes an item from the set.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns><see langword="true"/> if the item was removed; otherwise <see langword="false"/>.</returns>
        /// <exception cref="InvalidOperationException">Cannot remove from a read only set</exception>
        public bool Remove(T item)
        {
            if (IsReadOnly)
                throw new InvalidOperationException("Cannot remove from a read only set");

            return _collection.Remove(item);
        }

        /// <summary>
        ///     Removes a range of items from this set.
        /// </summary>
        /// <param name="items">The items to remove from this set.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Cannot remove from a read only set.</exception>
        public int RemoveRange(IEnumerable<T> items)
        {
            if (IsReadOnly)
                throw new InvalidOperationException("Cannot remove from a read only set");

            return _collection.RemoveAll(x => items.Contains(x));
        }

        // ISet
        object? ISet.QueryBuilder => QueryBuilder;
        bool ISet.IsSubQuery => IsSubQuery;
        Type ISet.GetInnerType() => typeof(T);

        // operators
        public static Set<T> operator +(Set<T> set, T value)
        {
            set.Add(value);
            return set;
        }

        public static Set<T> operator +(Set<T> set, Set<T> value)
        {
            set.AddRange(value);
            return set;
        }

        public static Set<T> operator -(Set<T> set, T value)
        {
            set.Remove(value);
            return set;
        }

        public static Set<T> operator -(Set<T> set, Set<T> value)
        {
            set.RemoveRange(value);
            return set;
        }
    }

    /// <summary>
    ///     Represents a generic set.
    /// </summary>
    public interface ISet
    {
        /// <summary>
        ///     Gets the number of items within this set.
        /// </summary>
        int Count { get; }

        /// <summary>
        ///     Gets whether or not this set is read-only.
        /// </summary>
        bool IsReadOnly { get; }
        internal object? QueryBuilder { get; }
        internal bool IsSubQuery { get; }
        internal Type GetInnerType();
    }
}
