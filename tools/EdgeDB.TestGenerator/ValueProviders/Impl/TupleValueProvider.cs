using EdgeDB.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class TupleValueProvider : IWrappingValueProvider
    {
        public string EdgeDBName => "tuple";

        public IValueProvider[]? Children { get; set; }

        private readonly Dictionary<object, List<IValueProvider>> _childMap = new();

        public object GetRandom(GenerationRuleSet rules)
        {
            var children = Children!.ToList();

            var tArr = new Type[children.Count];
            var vArr = new object[children.Count];

            for(int i = 0; i != tArr.Length; i++)
            {
                vArr[i] = children[i].GetRandom(rules);
                tArr[i] = vArr[i].GetType();
            }

            var transient = new TransientTuple(tArr, vArr);

            var value = transient.ToValueTuple();

            _childMap[value] = children;

            return value;
        }

        public string ToEdgeQLFormat(object value)
        {
            if (value is not ITuple tuple)
                throw new ArgumentException("value is not a tuple");

            if (!_childMap.TryGetValue(value, out var providers))
                throw new InvalidOperationException("Cannot determine providers used for the provided value");

            var elements = new string[tuple.Length];

            for(int i = 0; i != elements.Length; i++)
            {
                elements[i] = providers[i].ToEdgeQLFormat(tuple[i]!);
            }

            return $"({string.Join(", ", elements)})";
        }

        public override string ToString() => ((IWrappingValueProvider)this).FormatAsGeneric();
    }
}
