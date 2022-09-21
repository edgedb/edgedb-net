using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DataTypes
{
    /// <summary>
    ///     Represents an abstract tuple which is used for deserializing edgedb tuples to dotnet tuples.
    /// </summary>
    public readonly struct TransientTuple : ITuple
    {
        /// <summary>
        ///     Gets the types within this tuple, following the arity order of the tuple.
        /// </summary>
        public IReadOnlyCollection<Type> Types
            => _types.ToImmutableArray();

        /// <summary>
        ///     Gets the values within this tuple, following the arity order of the tuple.
        /// </summary>
        public IReadOnlyCollection<object?> Values
            => _types.ToImmutableArray();

        private readonly Type[] _types;
        private readonly object?[] _values;

        private delegate ITuple TupleBuilder(Type[] types, object?[] values);

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

        internal TransientTuple(Type[] types, object?[] values)
        {
            _values = values;
            _types = types;
        }

        /// <summary>
        ///     Converts this tuple to a <see cref="ValueTuple"/> with the specific arity.
        /// </summary>
        /// <returns>A <see cref="ValueTuple"/> boxed as a <see cref="ITuple"/>.</returns>
        public ITuple ToValueTuple()
        {
            return GenerateTuple((types, values) =>
            {
                var t = _valueTupleTypeMap[types.Length - 1];
                var generic = t.MakeGenericType(types);
                return (ITuple)Activator.CreateInstance(generic, values)!;
            });
        }

        /// <summary>
        ///     Converts this tuple to a <see cref="Tuple"/> with the specific arity.
        /// </summary>
        /// <returns>A <see cref="Tuple"/> boxed as a <see cref="ITuple"/>.</returns>
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

        /// <summary>
        ///     Gets the value within the tuple at the specified index.
        /// </summary>
        /// <remarks>
        ///     The value returned is by-ref and is read-only.
        /// </remarks>
        /// <param name="index">The index of the element to return.</param>
        /// <returns>
        ///     The value at the specified index.
        /// </returns>
        public ref readonly object? this[int index] 
            => ref _values[index];

        /// <summary>
        ///     The length of the tuple.
        /// </summary>
        public int Length => _values.Length;

        object? ITuple.this[int index] => _values[index];
    }
}
