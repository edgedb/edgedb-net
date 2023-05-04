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
            => RawTypes.ToImmutableArray();

        /// <summary>
        ///     Gets the values within this tuple, following the arity order of the tuple.
        /// </summary>
        public IReadOnlyCollection<object?> Values
            => RawValues.ToImmutableArray();

        internal readonly Type[] RawTypes;
        internal readonly object?[] RawValues;

        private delegate ITuple TupleBuilder(Type[] types, object?[] values);

        internal static bool IsValueTupleType(Type type)
            => ValueTupleTypeMap.Contains(type.IsConstructedGenericType
                    ? type.GetGenericTypeDefinition()
                    : type
                );

        internal static bool IsReferenceTupleType(Type type)
            => ReferenceTupleTypeMap.Contains(type.IsConstructedGenericType
                    ? type.GetGenericTypeDefinition()
                    : type
                );

        internal static readonly Type[] ValueTupleTypeMap = new[]
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

        internal static readonly Type[] ReferenceTupleTypeMap = new[]
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
            RawValues = values;
            RawTypes = types;
        }

        internal TransientTuple(object?[] values)
        {
            RawValues = values;
            RawTypes = new Type[values.Length];
            for(int i = 0; i != values.Length; i++)
            {
                RawTypes[i] = RawValues[i]?.GetType() ?? typeof(object);
            }
        }

        internal TransientTuple(ITuple tuple)
        {
            if (tuple is null)
                throw new ArgumentNullException(nameof(tuple));

            var types = new Type[tuple.Length];
            var values = new object?[tuple.Length];

            for(int i = 0; i != tuple.Length; i++)
            {
                types[i] = tuple[i]?.GetType() ?? typeof(object);
                values[i] = tuple[i];
            }

            RawValues = values;
            RawTypes = types;
        }

        /// <summary>
        ///     Converts this tuple to a <see cref="ValueTuple"/> with the specific arity.
        /// </summary>
        /// <returns>A <see cref="ValueTuple"/> boxed as a <see cref="ITuple"/>.</returns>
        public ITuple ToValueTuple()
        {
            return GenerateTuple((types, values) =>
            {
                var t = ValueTupleTypeMap[types.Length - 1];
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
                var t = ReferenceTupleTypeMap[types.Length - 1];
                var generic = t.MakeGenericType(types);
                return (ITuple)Activator.CreateInstance(generic, values)!;
            });
        }

        private ITuple GenerateTuple(TupleBuilder builder, int offset = 0)
        {
            if (RawValues.Length - offset > 7)
            {
                var innerTuple = GenerateTuple(builder, offset + 7);

                var types = new Type[8];
                types[7] = innerTuple.GetType();
                RawTypes[offset..(offset + 7)].CopyTo(types, 0);

                var values = new object?[8];
                values[7] = innerTuple;
                RawValues[offset..(offset + 7)].CopyTo(values, 0);

                return builder(types, values);
            }

            return builder(RawTypes[offset..], RawValues[offset..]);
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
            => ref RawValues[index];

        /// <summary>
        ///     The length of the tuple.
        /// </summary>
        public int Length => RawValues?.Length ?? 0;

        object? ITuple.this[int index] => RawValues[index];

        public static TransientTuple FromObjectEnumerator(ref ObjectEnumerator enumerator)
        {
            var types = new Type[enumerator.Length];
            var values = new object?[enumerator.Length];

            for(int i = 0; enumerator.Next(out _, out var value); i++)
            {
                types[i] = value?.GetType() ?? enumerator.Codecs[i].ConverterType;
                values[i] = value;
            }

            return new TransientTuple(types, values);
        }

        public static Type[] FlattenTypes(Type tuple)
        {
            if (!tuple.IsAssignableTo(typeof(ITuple)))
                throw new InvalidOperationException($"The type {tuple} is not a tuple!");

            var cTuple = tuple;

            List<Type> tupleTypes = new();

            while(true)
            {
                if(
                    cTuple.GenericTypeArguments.Length == 8 &&
                    cTuple.GenericTypeArguments[7].IsAssignableTo(typeof(ITuple)))
                {
                    // full tuple
                    tupleTypes.AddRange(cTuple.GenericTypeArguments[..7]);
                    cTuple = cTuple.GenericTypeArguments[7];
                }
                else
                {
                    tupleTypes.AddRange(cTuple.GenericTypeArguments);
                    return tupleTypes.ToArray();
                }
            }
        }

        internal static Type CreateValueTupleType(Type[] elementTypes)
            => CreateTupleType(t => ValueTupleTypeMap[t.Length - 1].MakeGenericType(t), elementTypes);

        internal static Type CreateReferenceTupleType(Type[] elementTypes)
            => CreateTupleType(t => ReferenceTupleTypeMap[t.Length - 1].MakeGenericType(t), elementTypes);

        private static Type CreateTupleType(Func<Type[], Type> ctor, Type[] elements, int offset = 0)
        {
            if (elements.Length - offset > 7)
            {
                var innerTuple = CreateTupleType(ctor, elements, offset + 7);

                var types = new Type[8];
                types[7] = innerTuple.GetType();
                elements[offset..(offset + 7)].CopyTo(types, 0);

                return ctor(types);
            }

            return ctor(elements[offset..]);
        }
    }
}
