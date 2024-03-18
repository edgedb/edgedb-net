#nullable restore
#pragma warning disable
using EdgeDB.DataTypes;
using System.Numerics;
using EdgeDB.Models.DataTypes;
using DateTime = System.DateTime;

namespace EdgeDB
{
    public sealed partial class EdgeQL
    {
        /// <summary>
        ///     Check that the input set contains at most one element, raise CardinalityViolationError otherwise.
        /// </summary>
        [EdgeQLFunction("assert_single", "std", "anytype", false, true)]
        public static TType? AssertSingle<TType>(IEnumerable<TType> input, String? message = null)
            => default!;
        /// <summary>
        ///     Check that the input set contains at least one element, raise CardinalityViolationError otherwise.
        /// </summary>
        [EdgeQLFunction("assert_exists", "std", "anytype", true, false)]
        public static IEnumerable<TType> AssertExists<TType>(IEnumerable<TType> input, String? message = null)
            => default!;
        /// <summary>
        ///     Check that the input set is a proper set, i.e. all elements are unique
        /// </summary>
        [EdgeQLFunction("assert_distinct", "std", "anytype", true, false)]
        public static IEnumerable<TType> AssertDistinct<TType>(IEnumerable<TType> input, String? message = null)
            => default!;
        /// <summary>
        ///     Return the smallest value of the input set.
        /// </summary>
        [EdgeQLFunction("min", "std", "anytype", false, true)]
        public static TType? Min<TType>(IEnumerable<TType> vals)
            => default!;
        /// <summary>
        ///     Return the greatest value of the input set.
        /// </summary>
        [EdgeQLFunction("max", "std", "anytype", false, true)]
        public static TType? Max<TType>(IEnumerable<TType> vals)
            => default!;
        /// <summary>
        ///     Return the element of *array* at the specified *index*.
        /// </summary>
        [EdgeQLFunction("array_get", "std", "anytype", false, true)]
        public static TType? ArrayGet<TType>(IEnumerable<TType> array, Int64 idx, TType? @default = default)
            => default!;
        /// <summary>
        ///     Return array elements as a set.
        /// </summary>
        [EdgeQLFunction("array_unpack", "std", "anytype", true, false)]
        public static IEnumerable<TType> ArrayUnpack<TType>(IEnumerable<TType> array)
            => default!;
        [EdgeQLFunction("DISTINCT", "std", "anytype", true, false)]
        public static IEnumerable<TType> Distinct<TType>(IEnumerable<TType> s = default)
            => default!;
        [EdgeQLFunction("UNION", "std", "anytype", true, false)]
        public static IEnumerable<TType> Union<TType>(IEnumerable<TType> s1 = default, IEnumerable<TType> s2 = default)
            => default!;
        [EdgeQLFunction("EXCEPT", "std", "anytype", true, false)]
        public static IEnumerable<TType> Except<TType>(IEnumerable<TType> s1 = default, IEnumerable<TType> s2 = default)
            => default!;
        [EdgeQLFunction("INTERSECT", "std", "anytype", true, false)]
        public static IEnumerable<TType> Intersect<TType>(IEnumerable<TType> s1 = default, IEnumerable<TType> s2 = default)
            => default!;
        /// <summary>
        ///     Assert that a boolean value is true.
        /// </summary>
        [EdgeQLFunction("assert", "std", "std::bool", false, false)]
        public static Boolean Assert(Boolean input, String? message = null)
            => default!;
        /// <summary>
        ///     Generalized boolean `AND` applied to the set of *values*.
        /// </summary>
        [EdgeQLFunction("all", "std", "std::bool", false, false)]
        public static Boolean All(IEnumerable<Boolean> vals)
            => default!;
        /// <summary>
        ///     Generalized boolean `OR` applied to the set of *values*.
        /// </summary>
        [EdgeQLFunction("any", "std", "std::bool", false, false)]
        public static Boolean Any(IEnumerable<Boolean> vals)
            => default!;
        /// <summary>
        ///     A polymorphic function to test if a sequence contains a certain element.
        /// </summary>
        [EdgeQLFunction("contains", "std", "std::bool", false, false)]
        public static Boolean Contains(String haystack, String needle)
            => default!;
        /// <summary>
        ///     A polymorphic function to test if a sequence contains a certain element.
        /// </summary>
        [EdgeQLFunction("contains", "std", "std::bool", false, false)]
        public static Boolean Contains(Byte[] haystack, Byte[] needle)
            => default!;
        /// <summary>
        ///     A polymorphic function to test if a sequence contains a certain element.
        /// </summary>
        [EdgeQLFunction("contains", "std", "std::bool", false, false)]
        public static Boolean Contains<TType>(IEnumerable<TType> haystack, TType needle)
            => default!;
        /// <summary>
        ///     A polymorphic function to test if one JSON value contains another JSON value.
        /// </summary>
        [EdgeQLFunction("contains", "std", "std::bool", false, false)]
        public static Boolean Contains(Json haystack, Json needle)
            => default!;
        [EdgeQLFunction("strictly_above", "std", "std::bool", false, false)]
        public static Boolean StrictlyAbove<TPoint>(MultiRange<TPoint> l, MultiRange<TPoint> r)
            where TPoint : struct
            => default!;
        /// <summary>
        ///     Test if a regular expression has a match in a string.
        /// </summary>
        [EdgeQLFunction("re_test", "std", "std::bool", false, false)]
        public static Boolean ReTest(String pattern, String str)
            => default!;
        [EdgeQLFunction("range_is_empty", "std", "std::bool", false, false)]
        public static Boolean RangeIsEmpty<TPoint>(Range<TPoint> val)
            where TPoint : struct
            => default!;
        [EdgeQLFunction("range_is_inclusive_upper", "std", "std::bool", false, false)]
        public static Boolean RangeIsInclusiveUpper<TPoint>(Range<TPoint> r)
            where TPoint : struct
            => default!;
        [EdgeQLFunction("range_is_inclusive_lower", "std", "std::bool", false, false)]
        public static Boolean RangeIsInclusiveLower<TPoint>(Range<TPoint> r)
            where TPoint : struct
            => default!;
        [EdgeQLFunction("contains", "std", "std::bool", false, false)]
        public static Boolean Contains<TPoint>(Range<TPoint> haystack, Range<TPoint> needle)
            where TPoint : struct
            => default!;
        [EdgeQLFunction("overlaps", "std", "std::bool", false, false)]
        public static Boolean Overlaps<TPoint>(Range<TPoint> l, Range<TPoint> r)
            where TPoint : struct
            => default!;
        [EdgeQLFunction("strictly_below", "std", "std::bool", false, false)]
        public static Boolean StrictlyBelow<TPoint>(Range<TPoint> l, Range<TPoint> r)
            where TPoint : struct
            => default!;
        [EdgeQLFunction("bounded_above", "std", "std::bool", false, false)]
        public static Boolean BoundedAbove<TPoint>(Range<TPoint> l, Range<TPoint> r)
            where TPoint : struct
            => default!;
        [EdgeQLFunction("bounded_below", "std", "std::bool", false, false)]
        public static Boolean BoundedBelow<TPoint>(Range<TPoint> l, Range<TPoint> r)
            where TPoint : struct
            => default!;
        [EdgeQLFunction("adjacent", "std", "std::bool", false, false)]
        public static Boolean Adjacent<TPoint>(Range<TPoint> l, Range<TPoint> r)
            where TPoint : struct
            => default!;
        [EdgeQLFunction("contains", "std", "std::bool", false, false)]
        public static Boolean Contains(Range<DateOnly> haystack, DateOnly needle)
            => default!;
        [EdgeQLFunction("contains", "std", "std::bool", false, false)]
        public static Boolean Contains(MultiRange<DateOnly> haystack, DateOnly needle)
            => default!;
        [EdgeQLFunction("IN", "std", "std::bool", false, false)]
        public static Boolean In<TType>(TType e = default, IEnumerable<TType> s = default)
            => default!;
        [EdgeQLFunction("NOT IN", "std", "std::bool", false, false)]
        public static Boolean NotIn<TType>(TType e = default, IEnumerable<TType> s = default)
            => default!;
        [EdgeQLFunction("EXISTS", "std", "std::bool", false, false)]
        public static Boolean Exists<TType>(IEnumerable<TType> s = default)
            => default!;
        [EdgeQLFunction("LIKE", "std", "std::bool", false, false)]
        public static Boolean Like(String str = null, String pattern = null)
            => default!;
        [EdgeQLFunction("ILIKE", "std", "std::bool", false, false)]
        public static Boolean ILike(String str = null, String pattern = null)
            => default!;
        [EdgeQLFunction("NOT LIKE", "std", "std::bool", false, false)]
        public static Boolean NotLike(String str = null, String pattern = null)
            => default!;
        [EdgeQLFunction("NOT ILIKE", "std", "std::bool", false, false)]
        public static Boolean NotILike(String str = null, String pattern = null)
            => default!;
        /// <summary>
        ///     A polymorphic function to calculate a "length" of its first argument.
        /// </summary>
        [EdgeQLFunction("len", "std", "std::int64", false, false)]
        public static Int64 Len(String str)
            => default!;
        /// <summary>
        ///     A polymorphic function to calculate a "length" of its first argument.
        /// </summary>
        [EdgeQLFunction("len", "std", "std::int64", false, false)]
        public static Int64 Len(Byte[] bytes)
            => default!;
        /// <summary>
        ///     A polymorphic function to calculate a "length" of its first argument.
        /// </summary>
        [EdgeQLFunction("len", "std", "std::int64", false, false)]
        public static Int64 Len<TType>(IEnumerable<TType> array)
            => default!;
        /// <summary>
        ///     Return the sum of the set of numbers.
        /// </summary>
        [EdgeQLFunction("sum", "std", "std::int64", false, false)]
        public static Int64 Sum(IEnumerable<Int32> s)
            => default!;
        /// <summary>
        ///     Return the sum of the set of numbers.
        /// </summary>
        [EdgeQLFunction("sum", "std", "std::int64", false, false)]
        public static Int64 Sum(IEnumerable<Int64> s)
            => default!;
        /// <summary>
        ///     Return the number of elements in a set.
        /// </summary>
        [EdgeQLFunction("count", "std", "std::int64", false, false)]
        public static Int64 Count<TType>(IEnumerable<TType> s)
            => default!;
        /// <summary>
        ///     Round to the nearest value.
        /// </summary>
        [EdgeQLFunction("round", "std", "std::int64", false, false)]
        public static Int64 Round(Int64 val)
            => default!;
        /// <summary>
        ///     A polymorphic function to find index of an element in a sequence.
        /// </summary>
        [EdgeQLFunction("find", "std", "std::int64", false, false)]
        public static Int64 Find(String haystack, String needle)
            => default!;
        /// <summary>
        ///     A polymorphic function to find index of an element in a sequence.
        /// </summary>
        [EdgeQLFunction("find", "std", "std::int64", false, false)]
        public static Int64 Find(Byte[] haystack, Byte[] needle)
            => default!;
        /// <summary>
        ///     A polymorphic function to find index of an element in a sequence.
        /// </summary>
        [EdgeQLFunction("find", "std", "std::int64", false, false)]
        public static Int64 Find<TType>(IEnumerable<TType> haystack, TType needle, Int64 from_pos = 0)
            => default!;
        /// <summary>
        ///     Bitwise AND operator for 64-bit integers.
        /// </summary>
        [EdgeQLFunction("bit_and", "std", "std::int64", false, false)]
        public static Int64 BitAnd(Int64 l, Int64 r)
            => default!;
        /// <summary>
        ///     Bitwise OR operator for 64-bit integers.
        /// </summary>
        [EdgeQLFunction("bit_or", "std", "std::int64", false, false)]
        public static Int64 BitOr(Int64 l, Int64 r)
            => default!;
        /// <summary>
        ///     Bitwise exclusive OR operator for 64-bit integers.
        /// </summary>
        [EdgeQLFunction("bit_xor", "std", "std::int64", false, false)]
        public static Int64 BitXor(Int64 l, Int64 r)
            => default!;
        /// <summary>
        ///     Bitwise NOT operator for 64-bit integers.
        /// </summary>
        [EdgeQLFunction("bit_not", "std", "std::int64", false, false)]
        public static Int64 BitNot(Int64 r)
            => default!;
        /// <summary>
        ///     Bitwise right-shift operator for 64-bit integers.
        /// </summary>
        [EdgeQLFunction("bit_rshift", "std", "std::int64", false, false)]
        public static Int64 BitRshift(Int64 val, Int64 n)
            => default!;
        /// <summary>
        ///     Bitwise left-shift operator for 64-bit integers.
        /// </summary>
        [EdgeQLFunction("bit_lshift", "std", "std::int64", false, false)]
        public static Int64 BitLshift(Int64 val, Int64 n)
            => default!;
        /// <summary>
        ///     Get the *nth* bit of the *bytes* value.
        /// </summary>
        [EdgeQLFunction("bytes_get_bit", "std", "std::int64", false, false)]
        public static Int64 BytesGetBit(Byte[] bytes, Int64 num)
            => default!;
        [EdgeQLFunction("range_unpack", "std", "std::int64", true, false)]
        public static IEnumerable<Int64> RangeUnpack(Range<Int64> val)
            => default!;
        [EdgeQLFunction("range_unpack", "std", "std::int64", true, false)]
        public static IEnumerable<Int64> RangeUnpack(Range<Int64> val, Int64 step)
            => default!;
        [EdgeQLFunction("sequence_reset", "std", "std::int64", false, false)]
        public static Int64 SequenceReset(Type seq, Int64 value)
            => default!;
        /// <summary>
        ///     Create a `int64` value.
        /// </summary>
        [EdgeQLFunction("to_int64", "std", "std::int64", false, false)]
        public static Int64 ToInt64(String s, String? fmt = null)
            => default!;
        [EdgeQLFunction("sequence_reset", "std", "std::int64", false, false)]
        public static Int64 SequenceReset(Type seq)
            => default!;
        [EdgeQLFunction("sequence_next", "std", "std::int64", false, false)]
        public static Int64 SequenceNext(Type seq)
            => default!;
        /// <summary>
        ///     Round up to the nearest integer.
        /// </summary>
        [EdgeQLFunction("ceil", "math", "std::int64", false, false)]
        public static Int64 Ceil(Int64 x)
            => default!;
        /// <summary>
        ///     Round down to the nearest integer.
        /// </summary>
        [EdgeQLFunction("floor", "math", "std::int64", false, false)]
        public static Int64 Floor(Int64 x)
            => default!;
        /// <summary>
        ///     Return the sum of the set of numbers.
        /// </summary>
        [EdgeQLFunction("sum", "std", "std::bigint", false, false)]
        public static BigInteger Sum(IEnumerable<BigInteger> s)
            => default!;
        /// <summary>
        ///     Round to the nearest value.
        /// </summary>
        [EdgeQLFunction("round", "std", "std::bigint", false, false)]
        public static BigInteger Round(BigInteger val)
            => default!;
        /// <summary>
        ///     Create a `bigint` value.
        /// </summary>
        [EdgeQLFunction("to_bigint", "std", "std::bigint", false, false)]
        public static BigInteger ToBigint(String s, String? fmt = null)
            => default!;
        /// <summary>
        ///     Round up to the nearest integer.
        /// </summary>
        [EdgeQLFunction("ceil", "math", "std::bigint", false, false)]
        public static BigInteger Ceil(BigInteger x)
            => default!;
        /// <summary>
        ///     Round down to the nearest integer.
        /// </summary>
        [EdgeQLFunction("floor", "math", "std::bigint", false, false)]
        public static BigInteger Floor(BigInteger x)
            => default!;
        /// <summary>
        ///     Return the sum of the set of numbers.
        /// </summary>
        [EdgeQLFunction("sum", "std", "std::decimal", false, false)]
        public static Decimal Sum(IEnumerable<Decimal> s)
            => default!;
        /// <summary>
        ///     Round to the nearest value.
        /// </summary>
        [EdgeQLFunction("round", "std", "std::decimal", false, false)]
        public static Decimal Round(Decimal val)
            => default!;
        /// <summary>
        ///     Round to the nearest value.
        /// </summary>
        [EdgeQLFunction("round", "std", "std::decimal", false, false)]
        public static Decimal Round(Decimal val, Int64 d)
            => default!;
        /// <summary>
        ///     Return duration as total number of seconds in interval.
        /// </summary>
        [EdgeQLFunction("duration_to_seconds", "std", "std::decimal", false, false)]
        public static Decimal DurationToSeconds(TimeSpan dur)
            => default!;
        [EdgeQLFunction("range_unpack", "std", "std::decimal", true, false)]
        public static IEnumerable<Decimal> RangeUnpack(Range<Decimal> val, Decimal step)
            => default!;
        /// <summary>
        ///     Create a `decimal` value.
        /// </summary>
        [EdgeQLFunction("to_decimal", "std", "std::decimal", false, false)]
        public static Decimal ToDecimal(String s, String? fmt = null)
            => default!;
        /// <summary>
        ///     Round up to the nearest integer.
        /// </summary>
        [EdgeQLFunction("ceil", "math", "std::decimal", false, false)]
        public static Decimal Ceil(Decimal x)
            => default!;
        /// <summary>
        ///     Round down to the nearest integer.
        /// </summary>
        [EdgeQLFunction("floor", "math", "std::decimal", false, false)]
        public static Decimal Floor(Decimal x)
            => default!;
        /// <summary>
        ///     Return the natural logarithm of the input value.
        /// </summary>
        [EdgeQLFunction("ln", "math", "std::decimal", false, false)]
        public static Decimal Ln(Decimal x)
            => default!;
        /// <summary>
        ///     Return the base 10 logarithm of the input value.
        /// </summary>
        [EdgeQLFunction("lg", "math", "std::decimal", false, false)]
        public static Decimal Lg(Decimal x)
            => default!;
        /// <summary>
        ///     Return the logarithm of the input value in the specified *base*.
        /// </summary>
        [EdgeQLFunction("log", "math", "std::decimal", false, false)]
        public static Decimal Log(Decimal x, Decimal @base)
            => default!;
        /// <summary>
        ///     Return the square root of the input value.
        /// </summary>
        [EdgeQLFunction("sqrt", "math", "std::decimal", false, false)]
        public static Decimal Sqrt(Decimal x)
            => default!;
        /// <summary>
        ///     Return the arithmetic mean of the input set.
        /// </summary>
        [EdgeQLFunction("mean", "math", "std::decimal", false, false)]
        public static Decimal Mean(IEnumerable<Decimal> vals)
            => default!;
        /// <summary>
        ///     Return the sample standard deviation of the input set.
        /// </summary>
        [EdgeQLFunction("stddev", "math", "std::decimal", false, false)]
        public static Decimal Stddev(IEnumerable<Decimal> vals)
            => default!;
        /// <summary>
        ///     Return the population standard deviation of the input set.
        /// </summary>
        [EdgeQLFunction("stddev_pop", "math", "std::decimal", false, false)]
        public static Decimal StddevPop(IEnumerable<Decimal> vals)
            => default!;
        /// <summary>
        ///     Return the sample variance of the input set.
        /// </summary>
        [EdgeQLFunction("var", "math", "std::decimal", false, true)]
        public static Decimal? Var(IEnumerable<Decimal> vals)
            => default!;
        /// <summary>
        ///     Return the population variance of the input set.
        /// </summary>
        [EdgeQLFunction("var_pop", "math", "std::decimal", false, true)]
        public static Decimal? VarPop(IEnumerable<Decimal> vals)
            => default!;
        /// <summary>
        ///     Return the sum of the set of numbers.
        /// </summary>
        [EdgeQLFunction("sum", "std", "std::float32", false, false)]
        public static Single Sum(IEnumerable<Single> s)
            => default!;
        [EdgeQLFunction("range_unpack", "std", "std::float32", true, false)]
        public static IEnumerable<Single> RangeUnpack(Range<Single> val, Single step)
            => default!;
        /// <summary>
        ///     Create a `float32` value.
        /// </summary>
        [EdgeQLFunction("to_float32", "std", "std::float32", false, false)]
        public static Single ToFloat32(String s, String? fmt = null)
            => default!;
        /// <summary>
        ///     Return the sum of the set of numbers.
        /// </summary>
        [EdgeQLFunction("sum", "std", "std::float64", false, false)]
        public static Double Sum(IEnumerable<Double> s)
            => default!;
        /// <summary>
        ///     Return a pseudo-random number in the range `0.0 <= x < 1.0`
        /// </summary>
        [EdgeQLFunction("random", "std", "std::float64", false, false)]
        public static Double Random()
            => default!;
        /// <summary>
        ///     Round to the nearest value.
        /// </summary>
        [EdgeQLFunction("round", "std", "std::float64", false, false)]
        public static Double Round(Double val)
            => default!;
        /// <summary>
        ///     Extract a specific element of input datetime by name.
        /// </summary>
        [EdgeQLFunction("datetime_get", "std", "std::float64", false, false)]
        public static Double DatetimeGet(DateTimeOffset dt, String el)
            => default!;
        /// <summary>
        ///     Extract a specific element of input duration by name.
        /// </summary>
        [EdgeQLFunction("duration_get", "std", "std::float64", false, false)]
        public static Double DurationGet(TimeSpan dt, String el)
            => default!;
        [EdgeQLFunction("range_unpack", "std", "std::float64", true, false)]
        public static IEnumerable<Double> RangeUnpack(Range<Double> val, Double step)
            => default!;
        /// <summary>
        ///     Create a `float64` value.
        /// </summary>
        [EdgeQLFunction("to_float64", "std", "std::float64", false, false)]
        public static Double ToFloat64(String s, String? fmt = null)
            => default!;
        /// <summary>
        ///     Round up to the nearest integer.
        /// </summary>
        [EdgeQLFunction("ceil", "math", "std::float64", false, false)]
        public static Double Ceil(Double x)
            => default!;
        /// <summary>
        ///     Round down to the nearest integer.
        /// </summary>
        [EdgeQLFunction("floor", "math", "std::float64", false, false)]
        public static Double Floor(Double x)
            => default!;
        /// <summary>
        ///     Return the natural logarithm of the input value.
        /// </summary>
        [EdgeQLFunction("ln", "math", "std::float64", false, false)]
        public static Double Ln(Int64 x)
            => default!;
        /// <summary>
        ///     Return the natural logarithm of the input value.
        /// </summary>
        [EdgeQLFunction("ln", "math", "std::float64", false, false)]
        public static Double Ln(Double x)
            => default!;
        /// <summary>
        ///     Return the base 10 logarithm of the input value.
        /// </summary>
        [EdgeQLFunction("lg", "math", "std::float64", false, false)]
        public static Double Lg(Int64 x)
            => default!;
        /// <summary>
        ///     Return the base 10 logarithm of the input value.
        /// </summary>
        [EdgeQLFunction("lg", "math", "std::float64", false, false)]
        public static Double Lg(Double x)
            => default!;
        /// <summary>
        ///     Return the square root of the input value.
        /// </summary>
        [EdgeQLFunction("sqrt", "math", "std::float64", false, false)]
        public static Double Sqrt(Int64 x)
            => default!;
        /// <summary>
        ///     Return the square root of the input value.
        /// </summary>
        [EdgeQLFunction("sqrt", "math", "std::float64", false, false)]
        public static Double Sqrt(Double x)
            => default!;
        /// <summary>
        ///     Return the arithmetic mean of the input set.
        /// </summary>
        [EdgeQLFunction("mean", "math", "std::float64", false, false)]
        public static Double Mean(IEnumerable<Int64> vals)
            => default!;
        /// <summary>
        ///     Return the arithmetic mean of the input set.
        /// </summary>
        [EdgeQLFunction("mean", "math", "std::float64", false, false)]
        public static Double Mean(IEnumerable<Double> vals)
            => default!;
        /// <summary>
        ///     Return the sample standard deviation of the input set.
        /// </summary>
        [EdgeQLFunction("stddev", "math", "std::float64", false, false)]
        public static Double Stddev(IEnumerable<Int64> vals)
            => default!;
        /// <summary>
        ///     Return the sample standard deviation of the input set.
        /// </summary>
        [EdgeQLFunction("stddev", "math", "std::float64", false, false)]
        public static Double Stddev(IEnumerable<Double> vals)
            => default!;
        /// <summary>
        ///     Return the population standard deviation of the input set.
        /// </summary>
        [EdgeQLFunction("stddev_pop", "math", "std::float64", false, false)]
        public static Double StddevPop(IEnumerable<Int64> vals)
            => default!;
        /// <summary>
        ///     Return the population standard deviation of the input set.
        /// </summary>
        [EdgeQLFunction("stddev_pop", "math", "std::float64", false, false)]
        public static Double StddevPop(IEnumerable<Double> vals)
            => default!;
        /// <summary>
        ///     Return the sample variance of the input set.
        /// </summary>
        [EdgeQLFunction("var", "math", "std::float64", false, true)]
        public static Double? Var(IEnumerable<Int64> vals)
            => default!;
        /// <summary>
        ///     Return the sample variance of the input set.
        /// </summary>
        [EdgeQLFunction("var", "math", "std::float64", false, true)]
        public static Double? Var(IEnumerable<Double> vals)
            => default!;
        /// <summary>
        ///     Return the population variance of the input set.
        /// </summary>
        [EdgeQLFunction("var_pop", "math", "std::float64", false, true)]
        public static Double? VarPop(IEnumerable<Int64> vals)
            => default!;
        /// <summary>
        ///     Return the population variance of the input set.
        /// </summary>
        [EdgeQLFunction("var_pop", "math", "std::float64", false, true)]
        public static Double? VarPop(IEnumerable<Double> vals)
            => default!;
        /// <summary>
        ///     Extract a specific element of input time by name.
        /// </summary>
        [EdgeQLFunction("time_get", "cal", "std::float64", false, false)]
        public static Double TimeGet(TimeSpan dt, String el)
            => default!;
        /// <summary>
        ///     Extract a specific element of input date by name.
        /// </summary>
        [EdgeQLFunction("date_get", "cal", "std::float64", false, false)]
        public static Double DateGet(DateOnly dt, String el)
            => default!;
        /// <summary>
        ///     Extract a specific element of input datetime by name.
        /// </summary>
        [EdgeQLFunction("datetime_get", "std", "std::float64", false, false)]
        public static Double DatetimeGet(DateTime dt, String el)
            => default!;
        /// <summary>
        ///     Return the absolute value of the input *x*.
        /// </summary>
        [EdgeQLFunction("abs", "math", "std::anyreal", false, false)]
        public static TReal Abs<TReal>(TReal x)
            => default!;
        /// <summary>
        ///     Return the smallest value of the input set.
        /// </summary>
        [EdgeQLFunction("min", "std", "std::str", false, true)]
        public static String? Min(IEnumerable<String> vals)
            => default!;
        /// <summary>
        ///     Return the greatest value of the input set.
        /// </summary>
        [EdgeQLFunction("max", "std", "std::str", false, true)]
        public static String? Max(IEnumerable<String> vals)
            => default!;
        /// <summary>
        ///     Render an array to a string.
        /// </summary>
        [EdgeQLFunction("array_join", "std", "std::str", false, false)]
        public static String ArrayJoin(IEnumerable<String> array, String delimiter)
            => default!;
        /// <summary>
        ///     Return the type of the outermost JSON value as a string.
        /// </summary>
        [EdgeQLFunction("json_typeof", "std", "std::str", false, false)]
        public static String JsonTypeof(Json json)
            => default!;
        /// <summary>
        ///     Replace matching substrings in a given string.
        /// </summary>
        [EdgeQLFunction("re_replace", "std", "std::str", false, false)]
        public static String ReReplace(String pattern, String sub, String str, String flags = "")
            => default!;
        /// <summary>
        ///     Repeat the input *string* *n* times.
        /// </summary>
        [EdgeQLFunction("str_repeat", "std", "std::str", false, false)]
        public static String StrRepeat(String s, Int64 n)
            => default!;
        /// <summary>
        ///     Return a lowercase copy of the input *string*.
        /// </summary>
        [EdgeQLFunction("str_lower", "std", "std::str", false, false)]
        public static String StrLower(String s)
            => default!;
        /// <summary>
        ///     Return an uppercase copy of the input *string*.
        /// </summary>
        [EdgeQLFunction("str_upper", "std", "std::str", false, false)]
        public static String StrUpper(String s)
            => default!;
        /// <summary>
        ///     Return a titlecase copy of the input *string*.
        /// </summary>
        [EdgeQLFunction("str_title", "std", "std::str", false, false)]
        public static String StrTitle(String s)
            => default!;
        /// <summary>
        ///     Return the input string padded at the start to the length *n*.
        /// </summary>
        [EdgeQLFunction("str_pad_start", "std", "std::str", false, false)]
        public static String StrPadStart(String s, Int64 n, String fill = " ")
            => default!;
        /// <summary>
        ///     Return the input string left-padded to the length *n*.
        /// </summary>
        [EdgeQLFunction("str_lpad", "std", "std::str", false, false)]
        public static String StrLpad(String s, Int64 n, String fill = " ")
            => default!;
        /// <summary>
        ///     Return the input string padded at the end to the length *n*.
        /// </summary>
        [EdgeQLFunction("str_pad_end", "std", "std::str", false, false)]
        public static String StrPadEnd(String s, Int64 n, String fill = " ")
            => default!;
        /// <summary>
        ///     Return the input string right-padded to the length *n*.
        /// </summary>
        [EdgeQLFunction("str_rpad", "std", "std::str", false, false)]
        public static String StrRpad(String s, Int64 n, String fill = " ")
            => default!;
        /// <summary>
        ///     Return the input string with all *trim* characters removed from its start.
        /// </summary>
        [EdgeQLFunction("str_trim_start", "std", "std::str", false, false)]
        public static String StrTrimStart(String s, String tr = " ")
            => default!;
        /// <summary>
        ///     Return the input string with all leftmost *trim* characters removed.
        /// </summary>
        [EdgeQLFunction("str_ltrim", "std", "std::str", false, false)]
        public static String StrLtrim(String s, String tr = " ")
            => default!;
        /// <summary>
        ///     Return the input string with all *trim* characters removed from its end.
        /// </summary>
        [EdgeQLFunction("str_trim_end", "std", "std::str", false, false)]
        public static String StrTrimEnd(String s, String tr = " ")
            => default!;
        /// <summary>
        ///     Return the input string with all rightmost *trim* characters removed.
        /// </summary>
        [EdgeQLFunction("str_rtrim", "std", "std::str", false, false)]
        public static String StrRtrim(String s, String tr = " ")
            => default!;
        /// <summary>
        ///     Return the input string with *trim* characters removed from both ends.
        /// </summary>
        [EdgeQLFunction("str_trim", "std", "std::str", false, false)]
        public static String StrTrim(String s, String tr = " ")
            => default!;
        /// <summary>
        ///     Given a string, find a matching substring and replace all its occurrences with a new substring.
        /// </summary>
        [EdgeQLFunction("str_replace", "std", "std::str", false, false)]
        public static String StrReplace(String s, String old, String @new)
            => default!;
        /// <summary>
        ///     Reverse the order of the characters in the string.
        /// </summary>
        [EdgeQLFunction("str_reverse", "std", "std::str", false, false)]
        public static String StrReverse(String s)
            => default!;
        /// <summary>
        ///     Return string representation of the input value.
        /// </summary>
        [EdgeQLFunction("to_str", "std", "std::str", false, false)]
        public static String ToStr(DateTimeOffset dt, String? fmt = null)
            => default!;
        /// <summary>
        ///     Return string representation of the input value.
        /// </summary>
        [EdgeQLFunction("to_str", "std", "std::str", false, false)]
        public static String ToStr(TimeSpan td, String? fmt = null)
            => default!;
        /// <summary>
        ///     Return string representation of the input value.
        /// </summary>
        [EdgeQLFunction("to_str", "std", "std::str", false, false)]
        public static String ToStr(Int64 i, String? fmt = null)
            => default!;
        /// <summary>
        ///     Return string representation of the input value.
        /// </summary>
        [EdgeQLFunction("to_str", "std", "std::str", false, false)]
        public static String ToStr(Double f, String? fmt = null)
            => default!;
        /// <summary>
        ///     Return string representation of the input value.
        /// </summary>
        [EdgeQLFunction("to_str", "std", "std::str", false, false)]
        public static String ToStr(BigInteger d, String? fmt = null)
            => default!;
        /// <summary>
        ///     Return string representation of the input value.
        /// </summary>
        [EdgeQLFunction("to_str", "std", "std::str", false, false)]
        public static String ToStr(Decimal d, String? fmt = null)
            => default!;
        /// <summary>
        ///     Return string representation of the input value.
        /// </summary>
        [EdgeQLFunction("to_str", "std", "std::str", false, false)]
        public static String ToStr(IEnumerable<String> array, String delimiter)
            => default!;
        /// <summary>
        ///     Return string representation of the input value.
        /// </summary>
        [EdgeQLFunction("to_str", "std", "std::str", false, false)]
        public static String ToStr(Json json, String? fmt = null)
            => default!;
        /// <summary>
        ///     Convert a binary UTF-8 string to a text value.
        /// </summary>
        [EdgeQLFunction("to_str", "std", "std::str", false, false)]
        public static String ToStr(Byte[] b)
            => default!;
        /// <summary>
        ///     Return the server version as a string.
        /// </summary>
        [EdgeQLFunction("get_version_as_str", "sys", "std::str", false, false)]
        public static String GetVersionAsStr()
            => default!;
        /// <summary>
        ///     Return the server instance name.
        /// </summary>
        [EdgeQLFunction("get_instance_name", "sys", "std::str", false, false)]
        public static String GetInstanceName()
            => default!;
        /// <summary>
        ///     Return the name of the current database as a string.
        /// </summary>
        [EdgeQLFunction("get_current_database", "sys", "std::str", false, false)]
        public static String GetCurrentDatabase()
            => default!;
        /// <summary>
        ///     Return string representation of the input value.
        /// </summary>
        [EdgeQLFunction("to_str", "std", "std::str", false, false)]
        public static String ToStr(DateTime dt, String? fmt = null)
            => default!;
        /// <summary>
        ///     Return string representation of the input value.
        /// </summary>
        [EdgeQLFunction("to_str", "std", "std::str", false, false)]
        public static String ToStr(DateOnly d, String? fmt = null)
            => default!;
        /// <summary>
        ///     Encode given data as a base64 string
        /// </summary>
        [EdgeQLFunction("base64_encode", "std::enc", "std::str", false, false)]
        public static String Base64Encode(Byte[] data, Base64Alphabet alphabet = Base64Alphabet.standard, Boolean padding = true)
            => default!;
        [EdgeQLFunction("CONCAT", "std", "std::str", false, false)]
        public static String Concat(String l = null, String r = null)
            => default!;
        /// <summary>
        ///     Return the smallest value of the input set.
        /// </summary>
        [EdgeQLFunction("min", "std", "std::datetime", false, true)]
        public static DateTimeOffset? Min(IEnumerable<DateTimeOffset> vals)
            => default!;
        /// <summary>
        ///     Return the greatest value of the input set.
        /// </summary>
        [EdgeQLFunction("max", "std", "std::datetime", false, true)]
        public static DateTimeOffset? Max(IEnumerable<DateTimeOffset> vals)
            => default!;
        /// <summary>
        ///     Return the current server date and time.
        /// </summary>
        [EdgeQLFunction("datetime_current", "std", "std::datetime", false, false)]
        public static DateTimeOffset DatetimeCurrent()
            => default!;
        /// <summary>
        ///     Return the date and time of the start of the current transaction.
        /// </summary>
        [EdgeQLFunction("datetime_of_transaction", "std", "std::datetime", false, false)]
        public static DateTimeOffset DatetimeOfTransaction()
            => default!;
        /// <summary>
        ///     Return the date and time of the start of the current statement.
        /// </summary>
        [EdgeQLFunction("datetime_of_statement", "std", "std::datetime", false, false)]
        public static DateTimeOffset DatetimeOfStatement()
            => default!;
        /// <summary>
        ///     Truncate the input datetime to a particular precision.
        /// </summary>
        [EdgeQLFunction("datetime_truncate", "std", "std::datetime", false, false)]
        public static DateTimeOffset DatetimeTruncate(DateTimeOffset dt, String unit)
            => default!;
        [EdgeQLFunction("range_unpack", "std", "std::datetime", true, false)]
        public static IEnumerable<DateTimeOffset> RangeUnpack(Range<DateTimeOffset> val, TimeSpan step)
            => default!;
        /// <summary>
        ///     Create a `datetime` value.
        /// </summary>
        [EdgeQLFunction("to_datetime", "std", "std::datetime", false, false)]
        public static DateTimeOffset ToDatetime(String s, String? fmt = null)
            => default!;
        /// <summary>
        ///     Create a `datetime` value.
        /// </summary>
        [EdgeQLFunction("to_datetime", "std", "std::datetime", false, false)]
        public static DateTimeOffset ToDatetime(Int64 year, Int64 month, Int64 day, Int64 hour, Int64 min, Double sec, String timezone)
            => default!;
        /// <summary>
        ///     Create a `datetime` value.
        /// </summary>
        [EdgeQLFunction("to_datetime", "std", "std::datetime", false, false)]
        public static DateTimeOffset ToDatetime(Double epochseconds)
            => default!;
        /// <summary>
        ///     Create a `datetime` value.
        /// </summary>
        [EdgeQLFunction("to_datetime", "std", "std::datetime", false, false)]
        public static DateTimeOffset ToDatetime(Int64 epochseconds)
            => default!;
        /// <summary>
        ///     Create a `datetime` value.
        /// </summary>
        [EdgeQLFunction("to_datetime", "std", "std::datetime", false, false)]
        public static DateTimeOffset ToDatetime(Decimal epochseconds)
            => default!;
        /// <summary>
        ///     Create a `datetime` value.
        /// </summary>
        [EdgeQLFunction("to_datetime", "std", "std::datetime", false, false)]
        public static DateTimeOffset ToDatetime(DateTime local, String zone)
            => default!;
        /// <summary>
        ///     Return the smallest value of the input set.
        /// </summary>
        [EdgeQLFunction("min", "std", "std::duration", false, true)]
        public static TimeSpan? Min(IEnumerable<TimeSpan> vals)
            => default!;
        /// <summary>
        ///     Return the greatest value of the input set.
        /// </summary>
        [EdgeQLFunction("max", "std", "std::duration", false, true)]
        public static TimeSpan? Max(IEnumerable<TimeSpan> vals)
            => default!;
        /// <summary>
        ///     Truncate the input duration to a particular precision.
        /// </summary>
        [EdgeQLFunction("duration_truncate", "std", "std::duration", false, false)]
        public static TimeSpan DurationTruncate(TimeSpan dt, String unit)
            => default!;
        /// <summary>
        ///     Create a `duration` value.
        /// </summary>
        [EdgeQLFunction("to_duration", "std", "std::duration", false, false)]
        public static TimeSpan ToDuration(Int64 hours = 0, Int64 minutes = 0, Double seconds = 0, Int64 microseconds = 0)
            => default!;
        /// <summary>
        ///     Return a set of tuples of the form `(index, element)`.
        /// </summary>
        [EdgeQLFunction("enumerate", "std", "tuple<std::int64, anytype>", true, false)]
        public static IEnumerable<ValueTuple<Int64, TType>> Enumerate<TType>(IEnumerable<TType> vals)
            => default!;
        /// <summary>
        ///     Return set of key/value tuples that make up the JSON object.
        /// </summary>
        [EdgeQLFunction("json_object_unpack", "std", "tuple<std::str, std::json>", true, false)]
        public static IEnumerable<ValueTuple<String, Json>> JsonObjectUnpack(Json obj)
            => default!;
        /// <summary>
        ///     Return the server version as a tuple.
        /// </summary>
        [EdgeQLFunction("get_version", "sys", "tuple<major:std::int64, minor:std::int64, stage:sys::VersionStage, stage_no:std::int64, local:array<std|str>>", false, false)]
        public static ValueTuple<Int64, Int64, VersionStage, Int64, IEnumerable<String>> GetVersion()
            => default!;
        /// <summary>
        ///     Search an object using its fts::index index. Returns objects that match the specified query and the matching score.
        /// </summary>
        [EdgeQLFunction("search", "fts", "tuple<object:anyobject, score:std::float32>", false, true)]
        public static ValueTuple<TObject, Single>? Search<TObject>(TObject @object, String query, String language = "eng", IEnumerable<Double>? weights = null)
            => default!;
        /// <summary>
        ///     Bitwise AND operator for 16-bit integers.
        /// </summary>
        [EdgeQLFunction("bit_and", "std", "std::int16", false, false)]
        public static Int16 BitAnd(Int16 l, Int16 r)
            => default!;
        /// <summary>
        ///     Bitwise OR operator for 16-bit integers.
        /// </summary>
        [EdgeQLFunction("bit_or", "std", "std::int16", false, false)]
        public static Int16 BitOr(Int16 l, Int16 r)
            => default!;
        /// <summary>
        ///     Bitwise exclusive OR operator for 16-bit integers.
        /// </summary>
        [EdgeQLFunction("bit_xor", "std", "std::int16", false, false)]
        public static Int16 BitXor(Int16 l, Int16 r)
            => default!;
        /// <summary>
        ///     Bitwise NOT operator for 16-bit integers.
        /// </summary>
        [EdgeQLFunction("bit_not", "std", "std::int16", false, false)]
        public static Int16 BitNot(Int16 r)
            => default!;
        /// <summary>
        ///     Bitwise right-shift operator for 16-bit integers.
        /// </summary>
        [EdgeQLFunction("bit_rshift", "std", "std::int16", false, false)]
        public static Int16 BitRshift(Int16 val, Int64 n)
            => default!;
        /// <summary>
        ///     Bitwise left-shift operator for 16-bit integers.
        /// </summary>
        [EdgeQLFunction("bit_lshift", "std", "std::int16", false, false)]
        public static Int16 BitLshift(Int16 val, Int64 n)
            => default!;
        /// <summary>
        ///     Create a `int16` value.
        /// </summary>
        [EdgeQLFunction("to_int16", "std", "std::int16", false, false)]
        public static Int16 ToInt16(String s, String? fmt = null)
            => default!;
        /// <summary>
        ///     Bitwise AND operator for 32-bit integers.
        /// </summary>
        [EdgeQLFunction("bit_and", "std", "std::int32", false, false)]
        public static Int32 BitAnd(Int32 l, Int32 r)
            => default!;
        /// <summary>
        ///     Bitwise OR operator for 32-bit integers.
        /// </summary>
        [EdgeQLFunction("bit_or", "std", "std::int32", false, false)]
        public static Int32 BitOr(Int32 l, Int32 r)
            => default!;
        /// <summary>
        ///     Bitwise exclusive OR operator for 32-bit integers.
        /// </summary>
        [EdgeQLFunction("bit_xor", "std", "std::int32", false, false)]
        public static Int32 BitXor(Int32 l, Int32 r)
            => default!;
        /// <summary>
        ///     Bitwise NOT operator for 32-bit integers.
        /// </summary>
        [EdgeQLFunction("bit_not", "std", "std::int32", false, false)]
        public static Int32 BitNot(Int32 r)
            => default!;
        /// <summary>
        ///     Bitwise right-shift operator for 32-bit integers.
        /// </summary>
        [EdgeQLFunction("bit_rshift", "std", "std::int32", false, false)]
        public static Int32 BitRshift(Int32 val, Int64 n)
            => default!;
        /// <summary>
        ///     Bitwise left-shift operator for 32-bit integers.
        /// </summary>
        [EdgeQLFunction("bit_lshift", "std", "std::int32", false, false)]
        public static Int32 BitLshift(Int32 val, Int64 n)
            => default!;
        [EdgeQLFunction("range_unpack", "std", "std::int32", true, false)]
        public static IEnumerable<Int32> RangeUnpack(Range<Int32> val)
            => default!;
        [EdgeQLFunction("range_unpack", "std", "std::int32", true, false)]
        public static IEnumerable<Int32> RangeUnpack(Range<Int32> val, Int32 step)
            => default!;
        /// <summary>
        ///     Create a `int32` value.
        /// </summary>
        [EdgeQLFunction("to_int32", "std", "std::int32", false, false)]
        public static Int32 ToInt32(String s, String? fmt = null)
            => default!;
        /// <summary>
        ///     Return the array made from all of the input set elements.
        /// </summary>
        [EdgeQLFunction("array_agg", "std", "array<anytype>", false, false)]
        public static IEnumerable<TType> ArrayAgg<TType>(IEnumerable<TType> s)
            => default!;
        /// <summary>
        ///     Return an array filled with the given value repeated as many times as specified.
        /// </summary>
        [EdgeQLFunction("array_fill", "std", "array<anytype>", false, false)]
        public static IEnumerable<TType> ArrayFill<TType>(TType val, Int64 n)
            => default!;
        /// <summary>
        ///     Replace each array element equal to the second argument with the third argument.
        /// </summary>
        [EdgeQLFunction("array_replace", "std", "array<anytype>", false, false)]
        public static IEnumerable<TType> ArrayReplace<TType>(IEnumerable<TType> array, TType old, TType @new)
            => default!;
        /// <summary>
        ///     Find the first regular expression match in a string.
        /// </summary>
        [EdgeQLFunction("re_match", "std", "array<std::str>", false, false)]
        public static IEnumerable<String> ReMatch(String pattern, String str)
            => default!;
        /// <summary>
        ///     Find all regular expression matches in a string.
        /// </summary>
        [EdgeQLFunction("re_match_all", "std", "array<std::str>", true, false)]
        public static IEnumerable<IEnumerable<String>> ReMatchAll(String pattern, String str)
            => default!;
        /// <summary>
        ///     Split string into array elements using the supplied delimiter.
        /// </summary>
        [EdgeQLFunction("str_split", "std", "array<std::str>", false, false)]
        public static IEnumerable<String> StrSplit(String s, String delimiter)
            => default!;
        /// <summary>
        ///     Return the smallest value of the input set.
        /// </summary>
        [EdgeQLFunction("max", "std", "array<cal::relative_duration>", false, true)]
        public static IEnumerable<TimeSpan>? Max(IEnumerable<IEnumerable<TimeSpan>> vals)
            => default!;
        /// <summary>
        ///     Return the smallest value of the input set.
        /// </summary>
        [EdgeQLFunction("min", "std", "array<cal::local_datetime>", false, true)]
        public static IEnumerable<DateTime>? Min(IEnumerable<IEnumerable<DateTime>> vals)
            => default!;
        /// <summary>
        ///     Return the smallest value of the input set.
        /// </summary>
        [EdgeQLFunction("min", "std", "array<cal::local_date>", false, true)]
        public static IEnumerable<DateOnly>? Min(IEnumerable<IEnumerable<DateOnly>> vals)
            => default!;
        /// <summary>
        ///     Return the smallest value of the input set.
        /// </summary>
        [EdgeQLFunction("min", "std", "array<cal::local_time>", false, true)]
        public static IEnumerable<TimeSpan>? Min(IEnumerable<IEnumerable<TimeSpan>> vals)
            => default!;
        /// <summary>
        ///     Return the smallest value of the input set.
        /// </summary>
        [EdgeQLFunction("max", "std", "array<cal::local_datetime>", false, true)]
        public static IEnumerable<DateTime>? Max(IEnumerable<IEnumerable<DateTime>> vals)
            => default!;
        /// <summary>
        ///     Return the smallest value of the input set.
        /// </summary>
        [EdgeQLFunction("max", "std", "array<cal::local_date>", false, true)]
        public static IEnumerable<DateOnly>? Max(IEnumerable<IEnumerable<DateOnly>> vals)
            => default!;
        /// <summary>
        ///     Return elements of JSON array as a set of `json`.
        /// </summary>
        [EdgeQLFunction("json_array_unpack", "std", "std::json", true, false)]
        public static IEnumerable<Json> JsonArrayUnpack(Json array)
            => default!;
        /// <summary>
        ///     Return a JSON object with set key/value pairs.
        /// </summary>
        [EdgeQLFunction("json_object_pack", "std", "std::json", false, false)]
        public static Json JsonObjectPack(IEnumerable<ValueTuple<String, Json>> pairs)
            => default!;
        /// <summary>
        ///     Return the JSON value at the end of the specified path or an empty set.
        /// </summary>
        [EdgeQLFunction("json_get", "std", "std::json", false, true)]
        public static Json? JsonGet(Json json, IEnumerable<String> path, Json? @default = default)
            => default!;
        /// <summary>
        ///     Return an updated JSON target with a new value.
        /// </summary>
        [EdgeQLFunction("json_set", "std", "std::json", false, true)]
        public static Json? JsonSet(Json target, IEnumerable<String> path, Json? value = default, Boolean create_if_missing = true, JsonEmpty empty_treatment = JsonEmpty.ReturnEmpty)
            => default!;
        /// <summary>
        ///     Return JSON value represented by the input *string*.
        /// </summary>
        [EdgeQLFunction("to_json", "std", "std::json", false, false)]
        public static Json ToJson(String str)
            => default!;
        [EdgeQLFunction("get_config_json", "cfg", "std::json", false, false)]
        public static Json GetConfigJson(IEnumerable<String>? sources = null, String? max_source = null)
            => default!;
        [EdgeQLFunction("CONCAT", "std", "std::json", false, false)]
        public static Json Concat(Json l = default, Json r = default)
            => default!;
        /// <summary>
        ///     Return a version 1 UUID.
        /// </summary>
        [EdgeQLFunction("uuid_generate_v1mc", "std", "std::uuid", false, false)]
        public static Guid UuidGenerateV1mc()
            => default!;
        /// <summary>
        ///     Return a version 4 UUID.
        /// </summary>
        [EdgeQLFunction("uuid_generate_v4", "std", "std::uuid", false, false)]
        public static Guid UuidGenerateV4()
            => default!;
        [EdgeQLFunction("range", "std", "range<std::anypoint>", false, false)]
        public static Range<TPoint> Range<TPoint>(TPoint? lower = default, TPoint? upper = default, Boolean inc_lower = true, Boolean inc_upper = false, Boolean empty = false)
            where TPoint : struct
            => default!;
        [EdgeQLFunction("multirange_unpack", "std", "range<std::int32>", true, false)]
        public static IEnumerable<Range<Int32>> MultirangeUnpack(MultiRange<Int32> val)
            => default!;
        [EdgeQLFunction("multirange_unpack", "std", "range<std::int64>", true, false)]
        public static IEnumerable<Range<Int64>> MultirangeUnpack(MultiRange<Int64> val)
            => default!;
        [EdgeQLFunction("multirange_unpack", "std", "range<std::float32>", true, false)]
        public static IEnumerable<Range<Single>> MultirangeUnpack(MultiRange<Single> val)
            => default!;
        [EdgeQLFunction("multirange_unpack", "std", "range<std::float64>", true, false)]
        public static IEnumerable<Range<Double>> MultirangeUnpack(MultiRange<Double> val)
            => default!;
        [EdgeQLFunction("multirange_unpack", "std", "range<std::decimal>", true, false)]
        public static IEnumerable<Range<Decimal>> MultirangeUnpack(MultiRange<Decimal> val)
            => default!;
        [EdgeQLFunction("multirange_unpack", "std", "range<std::datetime>", true, false)]
        public static IEnumerable<Range<DateTimeOffset>> MultirangeUnpack(MultiRange<DateTimeOffset> val)
            => default!;
        [EdgeQLFunction("multirange_unpack", "std", "range<cal::local_datetime>", true, false)]
        public static IEnumerable<Range<DateTime>> MultirangeUnpack(MultiRange<DateTime> val)
            => default!;
        [EdgeQLFunction("multirange_unpack", "std", "range<cal::local_date>", true, false)]
        public static IEnumerable<Range<DateOnly>> MultirangeUnpack(MultiRange<DateOnly> val)
            => default!;
        [EdgeQLFunction("multirange", "std", "multirange<std::anypoint>", false, false)]
        public static MultiRange<TPoint> Multirange<TPoint>(IEnumerable<Range<TPoint>> ranges)
            where TPoint : struct
            => default!;
        [EdgeQLFunction("range_get_upper", "std", "std::anypoint", false, true)]
        public static TPoint? RangeGetUpper<TPoint>(Range<TPoint> r)
            where TPoint : struct
            => default!;
        [EdgeQLFunction("range_get_lower", "std", "std::anypoint", false, true)]
        public static TPoint? RangeGetLower<TPoint>(Range<TPoint> r)
            where TPoint : struct
            => default!;
        /// <summary>
        ///     Convert a text string to a binary UTF-8 string.
        /// </summary>
        [EdgeQLFunction("to_bytes", "std", "std::bytes", false, false)]
        public static Byte[] ToBytes(String s)
            => default!;
        /// <summary>
        ///     Decode the byte64-encoded byte string and return decoded bytes.
        /// </summary>
        [EdgeQLFunction("base64_decode", "std::enc", "std::bytes", false, false)]
        public static Byte[] Base64Decode(String data, Base64Alphabet alphabet = Base64Alphabet.standard, Boolean padding = true)
            => default!;
        [EdgeQLFunction("CONCAT", "std", "std::bytes", false, false)]
        public static Byte[] Concat(Byte[] l = null, Byte[] r = null)
            => default!;
        /// <summary>
        ///     Return the isolation level of the current transaction.
        /// </summary>
        [EdgeQLFunction("get_transaction_isolation", "sys", "sys::TransactionIsolation", false, false)]
        public static TransactionIsolation GetTransactionIsolation()
            => default!;
        /// <summary>
        ///     Create a `cal::local_datetime` value.
        /// </summary>
        [EdgeQLFunction("to_local_datetime", "cal", "cal::local_datetime", false, false)]
        public static DateTime ToLocalDatetime(String s, String? fmt = null)
            => default!;
        /// <summary>
        ///     Create a `cal::local_datetime` value.
        /// </summary>
        [EdgeQLFunction("to_local_datetime", "cal", "cal::local_datetime", false, false)]
        public static DateTime ToLocalDatetime(Int64 year, Int64 month, Int64 day, Int64 hour, Int64 min, Double sec)
            => default!;
        /// <summary>
        ///     Create a `cal::local_datetime` value.
        /// </summary>
        [EdgeQLFunction("to_local_datetime", "cal", "cal::local_datetime", false, false)]
        public static DateTime ToLocalDatetime(DateTimeOffset dt, String zone)
            => default!;
        /// <summary>
        ///     Return the smallest value of the input set.
        /// </summary>
        [EdgeQLFunction("min", "std", "cal::local_datetime", false, true)]
        public static DateTime? Min(IEnumerable<DateTime> vals)
            => default!;
        /// <summary>
        ///     Return the smallest value of the input set.
        /// </summary>
        [EdgeQLFunction("max", "std", "cal::local_datetime", false, true)]
        public static DateTime? Max(IEnumerable<DateTime> vals)
            => default!;
        [EdgeQLFunction("range_unpack", "std", "cal::local_datetime", true, false)]
        public static IEnumerable<DateTime> RangeUnpack(Range<DateTime> val, TimeSpan step)
            => default!;
        /// <summary>
        ///     Create a `cal::local_date` value.
        /// </summary>
        [EdgeQLFunction("to_local_date", "cal", "cal::local_date", false, false)]
        public static DateOnly ToLocalDate(String s, String? fmt = null)
            => default!;
        /// <summary>
        ///     Create a `cal::local_date` value.
        /// </summary>
        [EdgeQLFunction("to_local_date", "cal", "cal::local_date", false, false)]
        public static DateOnly ToLocalDate(DateTimeOffset dt, String zone)
            => default!;
        /// <summary>
        ///     Create a `cal::local_date` value.
        /// </summary>
        [EdgeQLFunction("to_local_date", "cal", "cal::local_date", false, false)]
        public static DateOnly ToLocalDate(Int64 year, Int64 month, Int64 day)
            => default!;
        /// <summary>
        ///     Return the smallest value of the input set.
        /// </summary>
        [EdgeQLFunction("min", "std", "cal::local_date", false, true)]
        public static DateOnly? Min(IEnumerable<DateOnly> vals)
            => default!;
        /// <summary>
        ///     Return the smallest value of the input set.
        /// </summary>
        [EdgeQLFunction("max", "std", "cal::local_date", false, true)]
        public static DateOnly? Max(IEnumerable<DateOnly> vals)
            => default!;
        [EdgeQLFunction("range_unpack", "std", "cal::local_date", true, false)]
        public static IEnumerable<DateOnly> RangeUnpack(Range<DateOnly> val)
            => default!;
        [EdgeQLFunction("range_unpack", "std", "cal::local_date", true, false)]
        public static IEnumerable<DateOnly> RangeUnpack(Range<DateOnly> val, TimeSpan step)
            => default!;
        /// <summary>
        ///     Create a `cal::local_time` value.
        /// </summary>
        [EdgeQLFunction("to_local_time", "cal", "cal::local_time", false, false)]
        public static TimeSpan ToLocalTime(String s, String? fmt = null)
            => default!;
        /// <summary>
        ///     Create a `cal::local_time` value.
        /// </summary>
        [EdgeQLFunction("to_local_time", "cal", "cal::local_time", false, false)]
        public static TimeSpan ToLocalTime(DateTimeOffset dt, String zone)
            => default!;
        /// <summary>
        ///     Create a `cal::local_time` value.
        /// </summary>
        [EdgeQLFunction("to_local_time", "cal", "cal::local_time", false, false)]
        public static TimeSpan ToLocalTime(Int64 hour, Int64 min, Double sec)
            => default!;
        /// <summary>
        ///     Create a `cal::relative_duration` value.
        /// </summary>
        [EdgeQLFunction("to_relative_duration", "cal", "cal::relative_duration", false, false)]
        public static TimeSpan ToRelativeDuration(Int64 years = 0, Int64 months = 0, Int64 days = 0, Int64 hours = 0, Int64 minutes = 0, Double seconds = 0, Int64 microseconds = 0)
            => default!;
        /// <summary>
        ///     Convert 24-hour chunks into days.
        /// </summary>
        [EdgeQLFunction("duration_normalize_hours", "cal", "cal::relative_duration", false, false)]
        public static TimeSpan DurationNormalizeHours(TimeSpan dur)
            => default!;
        /// <summary>
        ///     Convert 30-day chunks into months.
        /// </summary>
        [EdgeQLFunction("duration_normalize_days", "cal", "cal::relative_duration", false, false)]
        public static TimeSpan DurationNormalizeDays(TimeSpan dur)
            => default!;
        /// <summary>
        ///     Create a `cal::date_duration` value.
        /// </summary>
        [EdgeQLFunction("to_date_duration", "cal", "cal::date_duration", false, false)]
        public static TimeSpan ToDateDuration(Int64 years = 0, Int64 months = 0, Int64 days = 0)
            => default!;
        /// <summary>
        ///     Adds language and weight category information to a string, so it be indexed with fts::index.
        /// </summary>
        [EdgeQLFunction("with_options", "fts", "fts::document", false, false)]
        public static Document WithOptions<TEnum>(String text, TEnum language, Weight? weight_category = Weight.A)
            => default!;
    }
}
