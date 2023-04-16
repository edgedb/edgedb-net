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

        public IEnumerable<IValueProvider>? Children { get; set; }

        public object GetRandom(GenerationRuleSet rules)
        {
            var children = Children!.ToArray();

            var tArr = new Type[children.Length];
            var vArr = new object[children.Length];

            for(int i = 0; i != tArr.Length; i++)
            {
                vArr[i] = children[i].GetRandom(rules);
                tArr[i] = vArr[i].GetType();
            }


            var t = new TransientTuple(tArr, vArr);

            return t.ToValueTuple();
        }

        public string ToEdgeQLFormat(object value)
        {
            if (value is not ITuple tuple)
                throw new ArgumentException("value is not a tuple");

            var elements = new string[tuple.Length];

            int i = 0;
            foreach(var child in Children!)
            {
                elements[i] = child.ToEdgeQLFormat(tuple[i]!);
                i++;
            }

            return $"({string.Join(", ", elements)})";
        }

        public override string ToString() => ((IWrappingValueProvider)this).FormatAsGeneric();
    }
}
