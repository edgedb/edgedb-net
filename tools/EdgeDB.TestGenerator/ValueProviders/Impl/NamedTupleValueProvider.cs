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
        private static readonly StringValueProvider _nameProvider = new();
        public string EdgeDBName => "namedtuple";

        public IEnumerable<IValueProvider>? Children { get; set; }

        public object GetRandom(GenerationRuleSet rules)
        {
            var data = new Dictionary<string, object>();

            foreach (var child in Children!)
            {
                data.TryAdd($"{IValueProvider.GetSmallHash(child)}_{_nameProvider.GetRandom(rules)}", child.GetRandom(rules));
            }

            return data;
        }

        public string ToEdgeQLFormat(object value)
        {
            if (value is not Dictionary<string, object> dict)
                throw new ArgumentException("value is not a dictionary");

            var mapped = dict.Select(x => (Provider: Children!.First(y => x.Key.StartsWith(IValueProvider.GetSmallHash(y))), KVP: x));

            return $"{{ {string.Join(", ", mapped.Select(x => $"{x.KVP.Key.Split("_")[1]} := {x.Provider.ToEdgeQLFormat(x.KVP.Value)}"))} }}";
        }
        public override string ToString() => ((IWrappingValueProvider)this).FormatAsGeneric();
    }
}
