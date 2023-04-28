using EdgeDB.TestGenerator.ValueProviders.Impl;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.TestGenerator.ValueProviders
{
    internal sealed class CachedNameFactory
    {
        private readonly ConcurrentDictionary<GenerationRuleSet, ConcurrentDictionary<int, string>> _cache = new();

        public string GetOrGenerate<T>(T identifier, GenerationRuleSet rules)
        {
            if (identifier is null)
                throw new NullReferenceException("Identifier cannot be null");

            var hashcode = identifier is int i ? i : identifier.GetHashCode();

            return _cache.GetOrAdd(rules, r => new()).GetOrAdd(hashcode, v => StringValueProvider.Shared.GetRandom(rules));
        }
    }
}
