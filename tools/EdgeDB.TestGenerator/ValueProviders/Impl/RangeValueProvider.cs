using EdgeDB.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class RangeValueProvider : IWrappingValueProvider
    {
        public IValueProvider[]? Children { get => new IValueProvider[] { _child! }; set => _child = value!.First(); }

        private IValueProvider? _child;

        private Type _childValueType
            => _child!.GetType().GetInterfaces()
                .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IValueProvider<>))
                .GetGenericArguments()[0];

        public string EdgeDBName => "range";

        public object GetRandom(GenerationRuleSet rules)
        {
            var a = (IComparable)_child!.GetRandom(rules);
            var b = (IComparable)_child.GetRandom(rules);

            while(a.CompareTo(b) > 0)
            {
                b = (IComparable)_child.GetRandom(rules);
            }

           return Activator.CreateInstance(typeof(Range<>).MakeGenericType(_childValueType), a, b, true, false)!;
        }

        public override string ToString() => ((IWrappingValueProvider)this).FormatAsGeneric();

        public string ToEdgeQLFormat(object value)
        {
            var method = typeof(RangeValueProvider).GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .First(x => x.Name == "ToEdgeQLFormat");

            return (string)method.MakeGenericMethod(_childValueType).Invoke(this, new object?[] { value, _child })!;
        }

        private string ToEdgeQLFormat<T>(Range<T> range, IValueProvider<T> child)
            where T : struct
        {
            return $"range({child.ToEdgeQLFormat(range.Lower!)}, {child.ToEdgeQLFormat(range.Upper!)})";
        }
    }
}
