using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

#if NET461
using TupleBox = System.Object;
#else
using TupleBox = System.Runtime.CompilerServices.ITuple;
#endif


namespace EdgeDB.DataTypes
{
    /// <summary>
    ///     Represents an abstract tuple which is used for deserializing edgedb tuples to dotnet tuples.
    /// </summary>
    public readonly struct TransientTuple
#if !NET461
        : ITuple
#endif
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

        private delegate TupleBox TupleBuilder(Type[] types, object?[] values);

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

#if !NET461
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
#endif

        /// <summary>
        ///     Converts this tuple to a <see cref="ValueTuple"/> with the specific arity.
        /// </summary>
        /// <returns>A <see cref="ValueTuple"/> boxed as a <see cref="TupleBox"/>.</returns>
        public TupleBox ToValueTuple()
        {
            return GenerateTuple((types, values) =>
            {
                var t = ValueTupleTypeMap[types.Length - 1];
                var generic = t.MakeGenericType(types);
                return (TupleBox)Activator.CreateInstance(generic, values)!;
            });
        }

        /// <summary>
        ///     Converts this tuple to a <see cref="Tuple"/> with the specific arity.
        /// </summary>
        /// <returns>A <see cref="Tuple"/> boxed as a <see cref="TupleBox"/>.</returns>
        public TupleBox ToReferenceTuple()
        {
            return GenerateTuple((types, values) =>
            {
                var t = ReferenceTupleTypeMap[types.Length - 1];
                var generic = t.MakeGenericType(types);
                return (TupleBox)Activator.CreateInstance(generic, values)!;
            });
        }

        private TupleBox GenerateTuple(TupleBuilder builder, int offset = 0)
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

#if !NET461
        object? ITuple.this[int index] => RawValues[index];
#endif

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
            if (!tuple.IsTuple())
                throw new InvalidOperationException($"The type {tuple} is not a tuple!");

            var cTuple = tuple;

            List<Type> tupleTypes = new();

            while(true)
            {
                if(
                    cTuple.GenericTypeArguments.Length == 8 &&
                    cTuple.GenericTypeArguments[7].IsTuple())
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

#if NET461
        public static TransientTuple FromBoxed(TupleBox box)
        {
            var type = box.GetType();
            if (IsValueTupleType(type))
                return IterateBoxed(type, box, (inst, el) => inst.GetType().GetField(el.HasValue ? $"Item{el.Value + 1}" : "Rest"));
            else if (IsReferenceTupleType(type))
                return IterateBoxed(type, box, (inst, el) => inst.GetType().GetProperty(el.HasValue ? $"Item{el.Value + 1}" : "Rest"));
            else
                throw new ArgumentException("Boxed object is not a tuple!", nameof(box));
        }

        private static TransientTuple IterateBoxed(Type tupleType, TupleBox value, Func<object, int?, object?> getProp)
        {
            // calc length
            int tupleLength = 0;

            var tempType = tupleType;

            while (tempType.GetGenericArguments().Length == 8)
            {
                tupleLength += 7;
                tempType = tupleType.GetGenericArguments()[7];
            }

            tupleLength += tempType.GetGenericArguments().Length;

            tempType = tupleType;
            var tempValue = value;

            var types = new Type[tupleLength];
            var values = new object?[tupleLength];

            for (int i = 0; i != tupleLength; i++)
            {
                if (i != 0 && i % 8 == 0)
                {
                    tempValue = getProp(tempValue!, null);
                    tempType = tempType.GetGenericArguments()[7];
                }

                var rel = i % 8;

                values[i] = getProp(tempValue!, rel);
                types[i] = tempType.GetGenericArguments()[rel];
            }

            return new TransientTuple(types, values);
        }
#endif
    }
}
