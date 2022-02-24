using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DataTypes
{
    public class NamedSet : Set<KeyValuePair<string, object?>>, IDictionary<string, object?>
    {
        public object? this[string key]
        {
            get => Collection.FirstOrDefault(x => x.Key == key);
            set
            {
                var index = Collection.FindIndex(x => x.Key == key);
                var original = base[index];
                base[index] = new KeyValuePair<string, object?>(original.Key, value);
            }
        }

        public ICollection<string> Keys => Collection.Select(x => x.Key).ToArray();

        public ICollection<object?> Values => Collection.Select(x => x.Value).ToArray();

        public NamedSet(IReadOnlyDictionary<string, object?> collection) : this(collection.ToDictionary(x => x.Key, x => x.Value), true) { }
        public NamedSet(IDictionary<string, object?> collection, bool isReadOnly) : base(collection, isReadOnly) { }

        public void Add(string key, object? value)
            => Add(new KeyValuePair<string, object?>(key, value));

        public bool ContainsKey(string key)
            => Collection.Any(x => x.Key == key);

        public bool Remove(string key)
            => Remove(key);

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object? value)
        {
            value = null;

            var index = Collection.FindIndex(x => x.Key == key);

            if (index == -1)
                return false;

            value = this[index].Value;

            return true;
        }
    }
}
