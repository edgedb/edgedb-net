using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class ArrayValueProvider : IWrappingValueProvider
    {
        private static readonly Random _random = new Random();

        public string EdgeDBName => "array";

        public IEnumerable<IValueProvider>? Children
        {
            get => new IValueProvider[] { _child! };
            set => _child = value!.First();
        }

        private IValueProvider? _child;

        public object GetRandom(GenerationRuleSet rules)
        {
            var sz = _random.Next(rules.GetRange<ArrayValueProvider>());
            var t = _child!.GetRandom(rules).GetType();

            var arr = Array.CreateInstance(t, sz);

            for(int i = 0; i != sz; i++)
            {
                var v = _child!.GetRandom(rules);
                arr.SetValue(v, i);
            }

            return arr;
        }

        public override string ToString() => ((IWrappingValueProvider)this).FormatAsGeneric();
        public string ToEdgeQLFormat(object value)
        {
            if (value is not Array arr)
                throw new ArgumentException("value is not an array");

            return $"[{string.Join(", ", arr.Cast<object>().Select(x => _child!.ToEdgeQLFormat(x)))}]";
        }
    }
}
