using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class NamedTupleValueProvider : IWrappingValueProvider
    {
        protected virtual Func<int, GenerationRuleSet, string>? NameProvider { get;}

        public string EdgeDBName => "namedtuple";

        public IValueProvider[]? Children { get; set; }

        private readonly Dictionary<object, Dictionary<string, IValueProvider>> _childMap = new();

        private StringValueProvider? _strProvider;

        public object GetRandom(GenerationRuleSet rules)
        {
            var nameProvider = NameProvider ?? ((_, rules) => (_strProvider ??= new()).GetRandom(rules)); 

            var providerMap = new Dictionary<string, IValueProvider>();
            var data = new Dictionary<string, object>();

            foreach (var child in Children!)
            {
                var key = $"{IValueProvider.GetSmallHash(child)}_{nameProvider(child.GetHashCode(), rules)}";
                data.TryAdd(key, child.GetRandom(rules));
                providerMap.TryAdd(key, child);
            }

            _childMap[data] = providerMap;

            return data;
        }

        public string ToEdgeQLFormat(object value)
        {
            if (value is not Dictionary<string, object> dict)
                throw new ArgumentException("value is not a dictionary");

            var mapped = new List<string>();

            if (!_childMap.TryGetValue(dict, out var providerMap))
                throw new InvalidOperationException("No provider map found that details the enumerated children of the given value");

            foreach(var kvp in dict)
            {
                if (!providerMap.TryGetValue(kvp.Key, out var provider))
                    throw new InvalidCastException($"Cannot find provider for the given key {kvp.Key}");

                mapped.Add($"{kvp.Key.Split("_")[1]} := {provider.ToEdgeQLFormat(kvp.Value)}");
            }

            var result = $"({string.Join(", ", mapped)})";

            return result;
        }
        public override string ToString() => ((IWrappingValueProvider)this).FormatAsGeneric();
    }

    internal class IdenticalNamedTupleValueProvider : NamedTupleValueProvider, IIdenticallyNamedProvider
    {
        protected override Func<int, GenerationRuleSet, string>? NameProvider
            => _factory.GetOrGenerate;

        private readonly CachedNameFactory _factory = new();
    }
}
