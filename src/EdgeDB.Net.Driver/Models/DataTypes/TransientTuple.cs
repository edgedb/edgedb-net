using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DataTypes
{
    public readonly struct TransientTuple : ITuple
    {
        private delegate ITuple TupleBuilder(Type[] types, object?[] values);

        private readonly Type[] _types;
        private readonly object?[] _values;

        private static readonly Type[] _valueTupleTypeMap = new[]
        {
            typeof(ValueTuple<>),
            typeof(ValueTuple<,>),
            typeof(ValueTuple<,,>),
            typeof(ValueTuple<,,,>),
            typeof(ValueTuple<,,,,>),
            typeof(ValueTuple<,,,,,>),
            typeof(ValueTuple<,,,,,,>),
            typeof(ValueTuple<,,,,,,,>)
        };

        private static readonly Type[] _referenceTupleTypeMap = new[]
        {
            typeof(Tuple<>),
            typeof(Tuple<,>),
            typeof(Tuple<,,>),
            typeof(Tuple<,,,>),
            typeof(Tuple<,,,,>),
            typeof(Tuple<,,,,,>),
            typeof(Tuple<,,,,,,>),
            typeof(Tuple<,,,,,,,>)
        };


        public TransientTuple(Type[] types, object?[] values)
        {
            _values = values;
            _types = types;
        }

        public ITuple ToValueTuple()
        {
            return GenerateTuple((types, values) =>
            {
                var t = _valueTupleTypeMap[types.Length - 1];
                var generic = t.MakeGenericType(types);
                return (ITuple)Activator.CreateInstance(generic, values)!;
            });
        }

        public ITuple ToReferenceTuple()
        {
            return GenerateTuple((types, values) =>
            {
                var t = _referenceTupleTypeMap[types.Length - 1];
                var generic = t.MakeGenericType(types);
                return (ITuple)Activator.CreateInstance(generic, values)!;
            });
        }

        private ITuple GenerateTuple(TupleBuilder builder, int offset = 0)
        {
            if (_values.Length - offset > 7)
            {
                var innerTuple = GenerateTuple(builder, offset + 7);

                var types = new Type[8];
                types[7] = innerTuple.GetType();
                _types[offset..(offset + 7)].CopyTo(types, 0);

                var values = new object?[8];
                values[7] = innerTuple;
                _values[offset..(offset + 7)].CopyTo(values, 0);

                return builder(types, values);
            }
            return builder(_types[offset..], _values[offset..]);

        }

        public object? this[int index] 
            => _values[index];

        public int Length => _values.Length;
    }
}
