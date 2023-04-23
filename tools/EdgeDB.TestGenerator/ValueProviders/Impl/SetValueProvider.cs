using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class SetValueProvider : IWrappingValueProvider
    {
        public IValueProvider[]? Children
        {
            get => new IValueProvider[] { _child! };
            set
            {
                _child = value!.First();
            }
        }

        private IValueProvider? _child;

        public string EdgeDBName => "set";

        public object GetRandom(GenerationRuleSet rules)
        {
            var sz = rules.Random.Next(rules.GetRange<SetValueProvider>());

            List<object> set = new List<object>();

            for (int i = 0; i != sz; i++)
            {
                set.Add(_child!.GetRandom(rules));
            }

            return set;
        }

        public override string ToString() => ((IWrappingValueProvider)this).FormatAsGeneric();

        public string ToEdgeQLFormat(object value)
        {
            if (value is not List<object> list)
                throw new ArgumentException("value is not a list");

            var result = $"{{ {string.Join(", ", list.Select(x => _child!.ToEdgeQLFormat(x)))} }}";

            if(result == "{  }")
            {

            }

            return result;
        }
    }
}
