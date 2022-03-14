using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DataTypes
{
    public class Set<T> : IEnumerable<T>, ICollection<T>, ISet
    {
        public virtual T this[int index]
        {
            get => Collection[index];
            set => Collection[index] = value;
        }
        public int Count => Collection.Count;

        public bool IsReadOnly { get; protected set; }

        protected readonly List<T> Collection;

        internal string? Query { get; }
        internal IDictionary<string, object?>? Arguments { get; }
        internal bool IsSubQuery { get; } = false;

        public Set() 
        {
            IsReadOnly = false;
            Collection = new();
        }

        public Set(IEnumerable<T> collection, bool readOnly) 
        {
            Collection = new(collection);
            IsReadOnly = readOnly;
        }

        public Set(int capacity)
        {
            Collection = new(capacity);
            IsReadOnly = false;
        }

        internal Set(string? query, IDictionary<string, object?> args)
            : this()
        {
            Query = query;
            Arguments = args;
            IsSubQuery = true;
        }

        public IEnumerator<T> GetEnumerator()
            => Collection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => Collection.GetEnumerator();

        public void Add(T item)
        {
            if (IsReadOnly)
                throw new NotSupportedException("Cannot add to a read only set");

            Collection.Add(item);
        }

        public void Clear()
        {
            if (IsReadOnly)
                throw new NotSupportedException("Cannot clear a read only set");

            Collection.Clear();
        }

        public bool Contains(T item)
            => Collection.Contains(item);

        public void CopyTo(T[] array, int arrayIndex)
            => Collection.CopyTo(array, arrayIndex);

        public bool Remove(T item)
        {
            if (IsReadOnly)
                throw new NotSupportedException("Cannot remove from a read only set");

            return Collection.Remove(item);
        }

        // ISet
        string? ISet.Query => Query;
        IDictionary<string, object?>? ISet.Arguments => Arguments;
        bool ISet.IsSubQuery => IsSubQuery;
    }

    public interface ISet
    {
        int Count { get; }
        bool IsReadOnly { get; }

        internal string? Query { get; }
        internal IDictionary<string, object?>? Arguments { get; }
        internal bool IsSubQuery { get; }
    }
}
