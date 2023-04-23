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
        public string EdgeDBName => "array";

        public IValueProvider[]? Children
        {
            get => new IValueProvider[] { _child! };
            set => _child = value![0];
        }

        private IValueProvider? _child;

        private readonly Dictionary<object, IValueProvider> _providerMap = new();

        public object GetRandom(GenerationRuleSet rules)
        {
            var ts = _child!.ToString();

            var sz = rules.Random.Next(rules.GetRange<ArrayValueProvider>());
            var t = _child!.GetRandom(rules).GetType();

            var arr = Array.CreateInstance(t, sz);

            for(int i = 0; i != sz; i++)
            {
                var v = _child!.GetRandom(rules);

                if(v.GetType() != t)
                {
                    var ts2 = _child.ToString();
                    var x = ts == ts2;
                    Console.WriteLine($"T1: {ts} T2: {ts2}, x {x}");
                }

                try
                {
                    arr.SetValue(v, i);
                }
                catch(Exception x)
                {
                    Console.WriteLine($"Child type {t}: v: {v}", x);
                }
            }

            _providerMap[arr] = _child;

            return arr;
        }

        public override string ToString() => ((IWrappingValueProvider)this).FormatAsGeneric();
        public string ToEdgeQLFormat(object value)
        {
            if (value is not Array arr)
                throw new ArgumentException("value is not an array");

            if (!_providerMap.TryGetValue(value, out var provider))
                throw new InvalidOperationException("Cannot determine provider used for the given value");

            return $"[{string.Join(", ", arr.Cast<object>().Select(x => provider.ToEdgeQLFormat(x)))}]";
        }
    }
}
