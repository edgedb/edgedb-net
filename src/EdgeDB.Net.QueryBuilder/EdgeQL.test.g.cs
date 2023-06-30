#nullable restore
#pragma warning disable
using EdgeDB.Operators;
using EdgeDB.DataTypes;
using System.Numerics;
using DateTime = System.DateTime;

namespace EdgeDB
{
    public sealed partial class EdgeQL
    {
        /// <summary>
        ///     Check that the input set contains at most one element, raise CardinalityViolationError otherwise.
        /// </summary>
        public static TType? AssertSingle<TType>(IEnumerable<TType> input, String? message)
            => default!;
        /// <summary>
        ///     Check that the input set contains at least one element, raise CardinalityViolationError otherwise.
        /// </summary>
        public static IEnumerable<TType> AssertExists<TType>(IEnumerable<TType> input, String? message)
            => default!;
        /// <summary>
        ///     Check that the input set is a proper set, i.e. all elements are unique
        /// </summary>
        public static IEnumerable<TType> AssertDistinct<TType>(IEnumerable<TType> input, String? message)
            => default!;
        /// <summary>
        ///     Return the smallest value of the input set.
        /// </summary>
        public static TType? Min<TType>(IEnumerable<TType> vals)
            => default!;
        /// <summary>
        ///     Return the greatest value of the input set.
        /// </summary>
        public static TType? Max<TType>(IEnumerable<TType> vals)
            => default!;
        /// <summary>
        ///     Return array elements as a set.
        /// </summary>
        public static IEnumerable<TType> ArrayUnpack<TType>(IEnumerable<TType> array)
            => default!;
        /// <summary>
        ///     Return the element of *array* at the specified *index*.
        /// </summary>
        public static TType? ArrayGet<TType>(IEnumerable<TType> array, Int64 idx, TType? @default)
            => default!;
        public static IEnumerable<TType> Distinct<TType>(IEnumerable<TType> s = default)
            => default!;
        public static IEnumerable<TType> Union<TType>(IEnumerable<TType> s1 = default, IEnumerable<TType> s2 = default)
            => default!;
        public static IEnumerable<TType> Except<TType>(IEnumerable<TType> s1 = default, IEnumerable<TType> s2 = default)
            => default!;
        public static IEnumerable<TType> Intersect<TType>(IEnumerable<TType> s1 = default, IEnumerable<TType> s2 = default)
            => default!;
        /// <summary>
        ///     Assert that a boolean value is true.
        /// </summary>
        public static Boolean Assert(Boolean input, String? message)
            => default!;
        /// <summary>
        ///     Generalized boolean `AND` applied to the set of *values*.
        /// </summary>
        public static Boolean All(IEnumerable<Boolean> vals)
            => default!;
        /// <summary>
        ///     Generalized boolean `OR` applied to the set of *values*.
        /// </summary>
        public static Boolean Any(IEnumerable<Boolean> vals)
            => default!;
        /// <summary>
        ///     A polymorphic function to test if a sequence contains a certain element.
        /// </summary>
        public static Boolean Contains(String haystack, String needle)
            => default!;
        /// <summary>
        ///     A polymorphic function to test if a sequence contains a certain element.
        /// </summary>
        public static Boolean Contains(Byte[] haystack, Byte[] needle)
            => default!;
        /// <summary>
        ///     A polymorphic function to test if a sequence contains a certain element.
        /// </summary>
        public static Boolean Contains<TType>(IEnumerable<TType> haystack, TType needle)
            => default!;
        /// <summary>
        ///     Test if a regular expression has a match in a string.
        /// </summary>
        public static Boolean ReTest(String pattern, String str)
            => default!;
        public static Boolean RangeIsEmpty<TPoint>(Range<TPoint> val)
            where TPoint : struct
            => default!;
        public static Boolean RangeIsInclusiveUpper<TPoint>(Range<TPoint> r)
            where TPoint : struct
            => default!;
        public static Boolean RangeIsInclusiveLower<TPoint>(Range<TPoint> r)
            where TPoint : struct
            => default!;
        public static Boolean Contains<TPoint>(Range<TPoint> haystack, Range<TPoint> needle)
            where TPoint : struct
            => default!;
        public static Boolean Overlaps<TPoint>(Range<TPoint> l, Range<TPoint> r)
            where TPoint : struct
            => default!;
        public static Boolean Contains(Range<DateOnly> haystack, DateOnly needle)
            => default!;
        public static Boolean Not(Boolean v = default)
            => default!;
        public static Boolean In<TType>(TType e = default, IEnumerable<TType> s = default)
            => default!;
        public static Boolean NotIn<TType>(TType e = default, IEnumerable<TType> s = default)
            => default!;
        public static Boolean Exists<TType>(IEnumerable<TType> s = default)
            => default!;
        public static Boolean Like(String str = null, String pattern = null)
            => default!;
        public static Boolean ILike(String str = null, String pattern = null)
            => default!;
        public static Boolean NotLike(String str = null, String pattern = null)
            => default!;
        public static Boolean NotILike(String str = null, String pattern = null)
            => default!;
        /// <summary>
        ///     A polymorphic function to calculate a "length" of its first argument.
        /// </summary>
        public static Int64 Len(String str)
            => default!;
        /// <summary>
        ///     A polymorphic function to calculate a "length" of its first argument.
        /// </summary>
        public static Int64 Len(Byte[] bytes)
            => default!;
        /// <summary>
        ///     A polymorphic function to calculate a "length" of its first argument.
        /// </summary>
        public static Int64 Len<TType>(IEnumerable<TType> array)
            => default!;
        /// <summary>
        ///     Return the sum of the set of numbers.
        /// </summary>
        public static Int64 Sum(IEnumerable<Int32> s)
            => default!;
        /// <summary>
        ///     Return the sum of the set of numbers.
        /// </summary>
        public static Int64 Sum(IEnumerable<Int64> s)
            => default!;
        /// <summary>
        ///     Return the number of elements in a set.
        /// </summary>
        public static Int64 Count<TType>(IEnumerable<TType> s)
            => default!;
        /// <summary>
        ///     Round to the nearest value.
        /// </summary>
        public static Int64 Round(Int64 val)
            => default!;
        /// <summary>
        ///     A polymorphic function to find index of an element in a sequence.
        /// </summary>
        public static Int64 Find(String haystack, String needle)
            => default!;
        /// <summary>
        ///     A polymorphic function to find index of an element in a sequence.
        /// </summary>
        public static Int64 Find(Byte[] haystack, Byte[] needle)
            => default!;
        /// <summary>
        ///     A polymorphic function to find index of an element in a sequence.
        /// </summary>
        public static Int64 Find<TType>(IEnumerable<TType> haystack, TType needle, Int64 from_pos = 0)
            => default!;
        /// <summary>
        ///     Bitwise AND operator for 64-bit integers.
        /// </summary>
        public static Int64 BitAnd(Int64 l, Int64 r)
            => default!;
        /// <summary>
        ///     Bitwise OR operator for 64-bit integers.
        /// </summary>
        public static Int64 BitOr(Int64 l, Int64 r)
            => default!;
        /// <summary>
        ///     Bitwise exclusive OR operator for 64-bit integers.
        /// </summary>
        public static Int64 BitXor(Int64 l, Int64 r)
            => default!;
        /// <summary>
        ///     Bitwise NOT operator for 64-bit integers.
        /// </summary>
        public static Int64 BitNot(Int64 r)
            => default!;
        /// <summary>
        ///     Bitwise right-shift operator for 64-bit integers.
        /// </summary>
        public static Int64 BitRshift(Int64 val, Int64 n)
            => default!;
        /// <summary>
        ///     Bitwise left-shift operator for 64-bit integers.
        /// </summary>
        public static Int64 BitLshift(Int64 val, Int64 n)
            => default!;
        /// <summary>
        ///     Get the *nth* bit of the *bytes* value.
        /// </summary>
        public static Int64 BytesGetBit(Byte[] bytes, Int64 num)
            => default!;
        public static IEnumerable<Int64> RangeUnpack(Range<Int64> val)
            => default!;
        public static IEnumerable<Int64> RangeUnpack(Range<Int64> val, Int64 step)
            => default!;
        /// <summary>
        ///     Create a `int64` value.
        /// </summary>
        public static Int64 ToInt64(String s, String? fmt)
            => default!;
        public static Int64 SequenceReset(Type seq, Int64 value)
            => default!;
        public static Int64 SequenceReset(Type seq)
            => default!;
        public static Int64 SequenceNext(Type seq)
            => default!;
        /// <summary>
        ///     Round up to the nearest integer.
        /// </summary>
        public static Int64 Ceil(Int64 x)
            => default!;
        /// <summary>
        ///     Round down to the nearest integer.
        /// </summary>
        public static Int64 Floor(Int64 x)
            => default!;
        /// <summary>
        ///     Return the sum of the set of numbers.
        /// </summary>
        public static BigInteger Sum(IEnumerable<BigInteger> s)
            => default!;
        /// <summary>
        ///     Round to the nearest value.
        /// </summary>
        public static BigInteger Round(BigInteger val)
            => default!;
        /// <summary>
        ///     Create a `bigint` value.
        /// </summary>
        public static BigInteger ToBigint(String s, String? fmt)
            => default!;
        /// <summary>
        ///     Round up to the nearest integer.
        /// </summary>
        public static BigInteger Ceil(BigInteger x)
            => default!;
        /// <summary>
        ///     Round down to the nearest integer.
        /// </summary>
        public static BigInteger Floor(BigInteger x)
            => default!;
        /// <summary>
        ///     Return the sum of the set of numbers.
        /// </summary>
        public static Decimal Sum(IEnumerable<Decimal> s)
            => default!;
        /// <summary>
        ///     Round to the nearest value.
        /// </summary>
        public static Decimal Round(Decimal val)
            => default!;
        /// <summary>
        ///     Round to the nearest value.
        /// </summary>
        public static Decimal Round(Decimal val, Int64 d)
            => default!;
        /// <summary>
        ///     Return duration as total number of seconds in interval.
        /// </summary>
        public static Decimal DurationToSeconds(TimeSpan dur)
            => default!;
        public static IEnumerable<Decimal> RangeUnpack(Range<Decimal> val, Decimal step)
            => default!;
        /// <summary>
        ///     Create a `decimal` value.
        /// </summary>
        public static Decimal ToDecimal(String s, String? fmt)
            => default!;
        /// <summary>
        ///     Return the sample variance of the input set.
        /// </summary>
        public static Decimal? Var(IEnumerable<Decimal> vals)
            => default!;
        /// <summary>
        ///     Round up to the nearest integer.
        /// </summary>
        public static Decimal Ceil(Decimal x)
            => default!;
        /// <summary>
        ///     Round down to the nearest integer.
        /// </summary>
        public static Decimal Floor(Decimal x)
            => default!;
        /// <summary>
        ///     Return the natural logarithm of the input value.
        /// </summary>
        public static Decimal Ln(Decimal x)
            => default!;
        /// <summary>
        ///     Return the base 10 logarithm of the input value.
        /// </summary>
        public static Decimal Lg(Decimal x)
            => default!;
        /// <summary>
        ///     Return the logarithm of the input value in the specified *base*.
        /// </summary>
        public static Decimal Log(Decimal x, Decimal @base)
            => default!;
        /// <summary>
        ///     Return the square root of the input value.
        /// </summary>
        public static Decimal Sqrt(Decimal x)
            => default!;
        /// <summary>
        ///     Return the arithmetic mean of the input set.
        /// </summary>
        public static Decimal Mean(IEnumerable<Decimal> vals)
            => default!;
        /// <summary>
        ///     Return the sample standard deviation of the input set.
        /// </summary>
        public static Decimal Stddev(IEnumerable<Decimal> vals)
            => default!;
        /// <summary>
        ///     Return the population standard deviation of the input set.
        /// </summary>
        public static Decimal StddevPop(IEnumerable<Decimal> vals)
            => default!;
        /// <summary>
        ///     Return the population variance of the input set.
        /// </summary>
        public static Decimal? VarPop(IEnumerable<Decimal> vals)
            => default!;
        /// <summary>
        ///     Return the sum of the set of numbers.
        /// </summary>
        public static Single Sum(IEnumerable<Single> s)
            => default!;
        public static IEnumerable<Single> RangeUnpack(Range<Single> val, Single step)
            => default!;
        /// <summary>
        ///     Create a `float32` value.
        /// </summary>
        public static Single ToFloat32(String s, String? fmt)
            => default!;
        /// <summary>
        ///     Return the sum of the set of numbers.
        /// </summary>
        public static Double Sum(IEnumerable<Double> s)
            => default!;
        /// <summary>
        ///     Return a pseudo-random number in the range `0.0 <= x < 1.0`
        /// </summary>
        public static Double Random()
            => default!;
        /// <summary>
        ///     Round to the nearest value.
        /// </summary>
        public static Double Round(Double val)
            => default!;
        /// <summary>
        ///     Extract a specific element of input datetime by name.
        /// </summary>
        public static Double DatetimeGet(DateTimeOffset dt, String el)
            => default!;
        /// <summary>
        ///     Extract a specific element of input duration by name.
        /// </summary>
        public static Double DurationGet(TimeSpan dt, String el)
            => default!;
        public static IEnumerable<Double> RangeUnpack(Range<Double> val, Double step)
            => default!;
        /// <summary>
        ///     Return the population standard deviation of the input set.
        /// </summary>
        public static Double StddevPop(IEnumerable<Double> vals)
            => default!;
        /// <summary>
        ///     Return the sample variance of the input set.
        /// </summary>
        public static Double? Var(IEnumerable<Int64> vals)
            => default!;
        /// <summary>
        ///     Create a `float64` value.
        /// </summary>
        public static Double ToFloat64(String s, String? fmt)
            => default!;
        /// <summary>
        ///     Round up to the nearest integer.
        /// </summary>
        public static Double Ceil(Double x)
            => default!;
        /// <summary>
        ///     Round down to the nearest integer.
        /// </summary>
        public static Double Floor(Double x)
            => default!;
        /// <summary>
        ///     Return the natural logarithm of the input value.
        /// </summary>
        public static Double Ln(Int64 x)
            => default!;
        /// <summary>
        ///     Return the natural logarithm of the input value.
        /// </summary>
        public static Double Ln(Double x)
            => default!;
        /// <summary>
        ///     Return the base 10 logarithm of the input value.
        /// </summary>
        public static Double Lg(Int64 x)
            => default!;
        /// <summary>
        ///     Return the base 10 logarithm of the input value.
        /// </summary>
        public static Double Lg(Double x)
            => default!;
        /// <summary>
        ///     Return the square root of the input value.
        /// </summary>
        public static Double Sqrt(Int64 x)
            => default!;
        /// <summary>
        ///     Return the square root of the input value.
        /// </summary>
        public static Double Sqrt(Double x)
            => default!;
        /// <summary>
        ///     Return the arithmetic mean of the input set.
        /// </summary>
        public static Double Mean(IEnumerable<Int64> vals)
            => default!;
        /// <summary>
        ///     Return the arithmetic mean of the input set.
        /// </summary>
        public static Double Mean(IEnumerable<Double> vals)
            => default!;
        /// <summary>
        ///     Return the sample standard deviation of the input set.
        /// </summary>
        public static Double Stddev(IEnumerable<Int64> vals)
            => default!;
        /// <summary>
        ///     Return the sample standard deviation of the input set.
        /// </summary>
        public static Double Stddev(IEnumerable<Double> vals)
            => default!;
        /// <summary>
        ///     Return the population standard deviation of the input set.
        /// </summary>
        public static Double StddevPop(IEnumerable<Int64> vals)
            => default!;
        /// <summary>
        ///     Return the sample variance of the input set.
        /// </summary>
        public static Double? Var(IEnumerable<Double> vals)
            => default!;
        /// <summary>
        ///     Return the population variance of the input set.
        /// </summary>
        public static Double? VarPop(IEnumerable<Int64> vals)
            => default!;
        /// <summary>
        ///     Return the population variance of the input set.
        /// </summary>
        public static Double? VarPop(IEnumerable<Double> vals)
            => default!;
        /// <summary>
        ///     Extract a specific element of input time by name.
        /// </summary>
        public static Double TimeGet(TimeSpan dt, String el)
            => default!;
        /// <summary>
        ///     Extract a specific element of input date by name.
        /// </summary>
        public static Double DateGet(DateOnly dt, String el)
            => default!;
        /// <summary>
        ///     Extract a specific element of input datetime by name.
        /// </summary>
        public static Double DatetimeGet(DateTime dt, String el)
            => default!;
        /// <summary>
        ///     Return the absolute value of the input *x*.
        /// </summary>
        public static TReal Abs<TReal>(TReal x)
            => default!;
        /// <summary>
        ///     Return the smallest value of the input set.
        /// </summary>
        public static String? Min(IEnumerable<String> vals)
            => default!;
        /// <summary>
        ///     Return the greatest value of the input set.
        /// </summary>
        public static String? Max(IEnumerable<String> vals)
            => default!;
        /// <summary>
        ///     Render an array to a string.
        /// </summary>
        public static String ArrayJoin(IEnumerable<String> array, String delimiter)
            => default!;
        /// <summary>
        ///     Return the type of the outermost JSON value as a string.
        /// </summary>
        public static String JsonTypeof(Json json)
            => default!;
        /// <summary>
        ///     Replace matching substrings in a given string.
        /// </summary>
        public static String ReReplace(String pattern, String sub, String str, String flags = "")
            => default!;
        /// <summary>
        ///     Repeat the input *string* *n* times.
        /// </summary>
        public static String StrRepeat(String s, Int64 n)
            => default!;
        /// <summary>
        ///     Return a lowercase copy of the input *string*.
        /// </summary>
        public static String StrLower(String s)
            => default!;
        /// <summary>
        ///     Return an uppercase copy of the input *string*.
        /// </summary>
        public static String StrUpper(String s)
            => default!;
        /// <summary>
        ///     Return a titlecase copy of the input *string*.
        /// </summary>
        public static String StrTitle(String s)
            => default!;
        /// <summary>
        ///     Return the input string padded at the start to the length *n*.
        /// </summary>
        public static String StrPadStart(String s, Int64 n, String fill = " ")
            => default!;
        /// <summary>
        ///     Return the input string left-padded to the length *n*.
        /// </summary>
        public static String StrLpad(String s, Int64 n, String fill = " ")
            => default!;
        /// <summary>
        ///     Return the input string padded at the end to the length *n*.
        /// </summary>
        public static String StrPadEnd(String s, Int64 n, String fill = " ")
            => default!;
        /// <summary>
        ///     Return the input string right-padded to the length *n*.
        /// </summary>
        public static String StrRpad(String s, Int64 n, String fill = " ")
            => default!;
        /// <summary>
        ///     Return the input string with all *trim* characters removed from its start.
        /// </summary>
        public static String StrTrimStart(String s, String tr = " ")
            => default!;
        /// <summary>
        ///     Return the input string with all leftmost *trim* characters removed.
        /// </summary>
        public static String StrLtrim(String s, String tr = " ")
            => default!;
        /// <summary>
        ///     Return the input string with all *trim* characters removed from its end.
        /// </summary>
        public static String StrTrimEnd(String s, String tr = " ")
            => default!;
        /// <summary>
        ///     Return the input string with all rightmost *trim* characters removed.
        /// </summary>
        public static String StrRtrim(String s, String tr = " ")
            => default!;
        /// <summary>
        ///     Return the input string with *trim* characters removed from both ends.
        /// </summary>
        public static String StrTrim(String s, String tr = " ")
            => default!;
        /// <summary>
        ///     Given a string, find a matching substring and replace all its occurrences with a new substring.
        /// </summary>
        public static String StrReplace(String s, String old, String @new)
            => default!;
        /// <summary>
        ///     Reverse the order of the characters in the string.
        /// </summary>
        public static String StrReverse(String s)
            => default!;
        /// <summary>
        ///     Return string representation of the input value.
        /// </summary>
        public static String ToStr(DateTimeOffset dt, String? fmt)
            => default!;
        /// <summary>
        ///     Return string representation of the input value.
        /// </summary>
        public static String ToStr(TimeSpan td, String? fmt)
            => default!;
        /// <summary>
        ///     Return string representation of the input value.
        /// </summary>
        public static String ToStr(Int64 i, String? fmt)
            => default!;
        /// <summary>
        ///     Return string representation of the input value.
        /// </summary>
        public static String ToStr(Double f, String? fmt)
            => default!;
        /// <summary>
        ///     Return string representation of the input value.
        /// </summary>
        public static String ToStr(BigInteger d, String? fmt)
            => default!;
        /// <summary>
        ///     Return string representation of the input value.
        /// </summary>
        public static String ToStr(Decimal d, String? fmt)
            => default!;
        /// <summary>
        ///     Return string representation of the input value.
        /// </summary>
        public static String ToStr(IEnumerable<String> array, String delimiter)
            => default!;
        /// <summary>
        ///     Return string representation of the input value.
        /// </summary>
        public static String ToStr(Json json, String? fmt)
            => default!;
        /// <summary>
        ///     Return the server version as a string.
        /// </summary>
        public static String GetVersionAsStr()
            => default!;
        /// <summary>
        ///     Return the server instance name.
        /// </summary>
        public static String GetInstanceName()
            => default!;
        /// <summary>
        ///     Return the name of the current database as a string.
        /// </summary>
        public static String GetCurrentDatabase()
            => default!;
        /// <summary>
        ///     Return string representation of the input value.
        /// </summary>
        public static String ToStr(DateTime dt, String? fmt)
            => default!;
        /// <summary>
        ///     Return string representation of the input value.
        /// </summary>
        public static String ToStr(DateOnly d, String? fmt)
            => default!;
        public static String Concat(String l = null, String r = null)
            => default!;
        /// <summary>
        ///     Return the smallest value of the input set.
        /// </summary>
        public static DateTimeOffset? Min(IEnumerable<DateTimeOffset> vals)
            => default!;
        /// <summary>
        ///     Return the greatest value of the input set.
        /// </summary>
        public static DateTimeOffset? Max(IEnumerable<DateTimeOffset> vals)
            => default!;
        /// <summary>
        ///     Return the current server date and time.
        /// </summary>
        public static DateTimeOffset DatetimeCurrent()
            => default!;
        /// <summary>
        ///     Return the date and time of the start of the current transaction.
        /// </summary>
        public static DateTimeOffset DatetimeOfTransaction()
            => default!;
        /// <summary>
        ///     Return the date and time of the start of the current statement.
        /// </summary>
        public static DateTimeOffset DatetimeOfStatement()
            => default!;
        /// <summary>
        ///     Truncate the input datetime to a particular precision.
        /// </summary>
        public static DateTimeOffset DatetimeTruncate(DateTimeOffset dt, String unit)
            => default!;
        public static IEnumerable<DateTimeOffset> RangeUnpack(Range<DateTimeOffset> val, TimeSpan step)
            => default!;
        /// <summary>
        ///     Create a `datetime` value.
        /// </summary>
        public static DateTimeOffset ToDatetime(String s, String? fmt)
            => default!;
        /// <summary>
        ///     Create a `datetime` value.
        /// </summary>
        public static DateTimeOffset ToDatetime(Int64 year, Int64 month, Int64 day, Int64 hour, Int64 min, Double sec, String timezone)
            => default!;
        /// <summary>
        ///     Create a `datetime` value.
        /// </summary>
        public static DateTimeOffset ToDatetime(Double epochseconds)
            => default!;
        /// <summary>
        ///     Create a `datetime` value.
        /// </summary>
        public static DateTimeOffset ToDatetime(Int64 epochseconds)
            => default!;
        /// <summary>
        ///     Create a `datetime` value.
        /// </summary>
        public static DateTimeOffset ToDatetime(Decimal epochseconds)
            => default!;
        /// <summary>
        ///     Create a `datetime` value.
        /// </summary>
        public static DateTimeOffset ToDatetime(DateTime local, String zone)
            => default!;
        /// <summary>
        ///     Return the smallest value of the input set.
        /// </summary>
        public static TimeSpan? Min(IEnumerable<TimeSpan> vals)
            => default!;
        /// <summary>
        ///     Return the greatest value of the input set.
        /// </summary>
        public static TimeSpan? Max(IEnumerable<TimeSpan> vals)
            => default!;
        /// <summary>
        ///     Truncate the input duration to a particular precision.
        /// </summary>
        public static TimeSpan DurationTruncate(TimeSpan dt, String unit)
            => default!;
        /// <summary>
        ///     Create a `duration` value.
        /// </summary>
        public static TimeSpan ToDuration(Int64 hours = 0, Int64 minutes = 0, Double seconds = 0, Int64 microseconds = 0)
            => default!;
        /// <summary>
        ///     Return a set of tuples of the form `(index, element)`.
        /// </summary>
        public static IEnumerable<ValueTuple<Int64, TType>> Enumerate<TType>(IEnumerable<TType> vals)
            => default!;
        /// <summary>
        ///     Return set of key/value tuples that make up the JSON object.
        /// </summary>
        public static IEnumerable<ValueTuple<String, Json>> JsonObjectUnpack(Json obj)
            => default!;
        /// <summary>
        ///     Return the server version as a tuple.
        /// </summary>
        public static ValueTuple<Int64, Int64, VersionStage, Int64, IEnumerable<String>> GetVersion()
            => default!;
        /// <summary>
        ///     Bitwise AND operator for 16-bit integers.
        /// </summary>
        public static Int16 BitAnd(Int16 l, Int16 r)
            => default!;
        /// <summary>
        ///     Bitwise OR operator for 16-bit integers.
        /// </summary>
        public static Int16 BitOr(Int16 l, Int16 r)
            => default!;
        /// <summary>
        ///     Bitwise exclusive OR operator for 16-bit integers.
        /// </summary>
        public static Int16 BitXor(Int16 l, Int16 r)
            => default!;
        /// <summary>
        ///     Bitwise NOT operator for 16-bit integers.
        /// </summary>
        public static Int16 BitNot(Int16 r)
            => default!;
        /// <summary>
        ///     Bitwise right-shift operator for 16-bit integers.
        /// </summary>
        public static Int16 BitRshift(Int16 val, Int64 n)
            => default!;
        /// <summary>
        ///     Bitwise left-shift operator for 16-bit integers.
        /// </summary>
        public static Int16 BitLshift(Int16 val, Int64 n)
            => default!;
        /// <summary>
        ///     Create a `int16` value.
        /// </summary>
        public static Int16 ToInt16(String s, String? fmt)
            => default!;
        /// <summary>
        ///     Bitwise AND operator for 32-bit integers.
        /// </summary>
        public static Int32 BitAnd(Int32 l, Int32 r)
            => default!;
        /// <summary>
        ///     Bitwise OR operator for 32-bit integers.
        /// </summary>
        public static Int32 BitOr(Int32 l, Int32 r)
            => default!;
        /// <summary>
        ///     Bitwise exclusive OR operator for 32-bit integers.
        /// </summary>
        public static Int32 BitXor(Int32 l, Int32 r)
            => default!;
        /// <summary>
        ///     Bitwise NOT operator for 32-bit integers.
        /// </summary>
        public static Int32 BitNot(Int32 r)
            => default!;
        /// <summary>
        ///     Bitwise right-shift operator for 32-bit integers.
        /// </summary>
        public static Int32 BitRshift(Int32 val, Int64 n)
            => default!;
        /// <summary>
        ///     Bitwise left-shift operator for 32-bit integers.
        /// </summary>
        public static Int32 BitLshift(Int32 val, Int64 n)
            => default!;
        public static IEnumerable<Int32> RangeUnpack(Range<Int32> val)
            => default!;
        public static IEnumerable<Int32> RangeUnpack(Range<Int32> val, Int32 step)
            => default!;
        /// <summary>
        ///     Create a `int32` value.
        /// </summary>
        public static Int32 ToInt32(String s, String? fmt)
            => default!;
        /// <summary>
        ///     Return the array made from all of the input set elements.
        /// </summary>
        public static IEnumerable<TType> ArrayAgg<TType>(IEnumerable<TType> s)
            => default!;
        /// <summary>
        ///     Return an array filled with the given value repeated as many times as specified.
        /// </summary>
        public static IEnumerable<TType> ArrayFill<TType>(TType val, Int64 n)
            => default!;
        /// <summary>
        ///     Replace each array element equal to the second argument with the third argument.
        /// </summary>
        public static IEnumerable<TType> ArrayReplace<TType>(IEnumerable<TType> array, TType old, TType @new)
            => default!;
        /// <summary>
        ///     Find the first regular expression match in a string.
        /// </summary>
        public static IEnumerable<String> ReMatch(String pattern, String str)
            => default!;
        /// <summary>
        ///     Find all regular expression matches in a string.
        /// </summary>
        public static IEnumerable<IEnumerable<String>> ReMatchAll(String pattern, String str)
            => default!;
        /// <summary>
        ///     Split string into array elements using the supplied delimiter.
        /// </summary>
        public static IEnumerable<String> StrSplit(String s, String delimiter)
            => default!;
        /// <summary>
        ///     Return the smallest value of the input set.
        /// </summary>
        public static IEnumerable<DateTime>? Min(IEnumerable<IEnumerable<DateTime>> vals)
            => default!;
        /// <summary>
        ///     Return the smallest value of the input set.
        /// </summary>
        public static IEnumerable<DateOnly>? Min(IEnumerable<IEnumerable<DateOnly>> vals)
            => default!;
        /// <summary>
        ///     Return the smallest value of the input set.
        /// </summary>
        public static IEnumerable<TimeSpan>? Min(IEnumerable<IEnumerable<TimeSpan>> vals)
            => default!;
        /// <summary>
        ///     Return the smallest value of the input set.
        /// </summary>
        public static IEnumerable<DateTime>? Max(IEnumerable<IEnumerable<DateTime>> vals)
            => default!;
        /// <summary>
        ///     Return the smallest value of the input set.
        /// </summary>
        public static IEnumerable<DateOnly>? Max(IEnumerable<IEnumerable<DateOnly>> vals)
            => default!;
        /// <summary>
        ///     Return the smallest value of the input set.
        /// </summary>
        public static IEnumerable<TimeSpan>? Max(IEnumerable<IEnumerable<TimeSpan>> vals)
            => default!;
        /// <summary>
        ///     Return elements of JSON array as a set of `json`.
        /// </summary>
        public static IEnumerable<Json> JsonArrayUnpack(Json array)
            => default!;
        /// <summary>
        ///     Return a JSON object with set key/value pairs.
        /// </summary>
        public static Json JsonObjectPack(IEnumerable<ValueTuple<String, Json>> pairs)
            => default!;
        /// <summary>
        ///     Return the JSON value at the end of the specified path or an empty set.
        /// </summary>
        public static Json? JsonGet(Json json, IEnumerable<String> path, Json? @default)
            => default!;
        /// <summary>
        ///     Return an updated JSON target with a new value.
        /// </summary>
        public static Json? JsonSet(Json target, IEnumerable<String> path, Json? value, Boolean create_if_missing = true, JsonEmpty empty_treatment = JsonEmpty.ReturnEmpty)
            => default!;
        /// <summary>
        ///     Return JSON value represented by the input *string*.
        /// </summary>
        public static Json ToJson(String str)
            => default!;
        public static Json GetConfigJson(IEnumerable<String>? sources, String? max_source)
            => default!;
        public static Json Concat(Json l = default, Json r = default)
            => default!;
        /// <summary>
        ///     Return a version 1 UUID.
        /// </summary>
        public static Guid UuidGenerateV1mc()
            => default!;
        /// <summary>
        ///     Return a version 4 UUID.
        /// </summary>
        public static Guid UuidGenerateV4()
            => default!;
        public static Range<TPoint> Range<TPoint>(TPoint? lower, TPoint? upper, Boolean inc_lower = true, Boolean inc_upper = false, Boolean empty = false)
            where TPoint : struct
            => default!;
        public static TPoint? RangeGetUpper<TPoint>(Range<TPoint> r)
            where TPoint : struct
            => default!;
        public static TPoint? RangeGetLower<TPoint>(Range<TPoint> r)
            where TPoint : struct
            => default!;
        /// <summary>
        ///     Return the isolation level of the current transaction.
        /// </summary>
        public static TransactionIsolation GetTransactionIsolation()
            => default!;
        /// <summary>
        ///     Create a `cal::local_datetime` value.
        /// </summary>
        public static DateTime ToLocalDatetime(String s, String? fmt)
            => default!;
        /// <summary>
        ///     Create a `cal::local_datetime` value.
        /// </summary>
        public static DateTime ToLocalDatetime(Int64 year, Int64 month, Int64 day, Int64 hour, Int64 min, Double sec)
            => default!;
        /// <summary>
        ///     Create a `cal::local_datetime` value.
        /// </summary>
        public static DateTime ToLocalDatetime(DateTimeOffset dt, String zone)
            => default!;
        /// <summary>
        ///     Return the smallest value of the input set.
        /// </summary>
        public static DateTime? Min(IEnumerable<DateTime> vals)
            => default!;
        /// <summary>
        ///     Return the smallest value of the input set.
        /// </summary>
        public static DateTime? Max(IEnumerable<DateTime> vals)
            => default!;
        public static IEnumerable<DateTime> RangeUnpack(Range<DateTime> val, TimeSpan step)
            => default!;
        /// <summary>
        ///     Create a `cal::local_date` value.
        /// </summary>
        public static DateOnly ToLocalDate(String s, String? fmt)
            => default!;
        /// <summary>
        ///     Create a `cal::local_date` value.
        /// </summary>
        public static DateOnly ToLocalDate(DateTimeOffset dt, String zone)
            => default!;
        /// <summary>
        ///     Create a `cal::local_date` value.
        /// </summary>
        public static DateOnly ToLocalDate(Int64 year, Int64 month, Int64 day)
            => default!;
        /// <summary>
        ///     Return the smallest value of the input set.
        /// </summary>
        public static DateOnly? Max(IEnumerable<DateOnly> vals)
            => default!;
        /// <summary>
        ///     Return the smallest value of the input set.
        /// </summary>
        public static DateOnly? Min(IEnumerable<DateOnly> vals)
            => default!;
        public static IEnumerable<DateOnly> RangeUnpack(Range<DateOnly> val)
            => default!;
        public static IEnumerable<DateOnly> RangeUnpack(Range<DateOnly> val, TimeSpan step)
            => default!;
        /// <summary>
        ///     Create a `cal::local_time` value.
        /// </summary>
        public static TimeSpan ToLocalTime(String s, String? fmt)
            => default!;
        /// <summary>
        ///     Create a `cal::local_time` value.
        /// </summary>
        public static TimeSpan ToLocalTime(DateTimeOffset dt, String zone)
            => default!;
        /// <summary>
        ///     Create a `cal::local_time` value.
        /// </summary>
        public static TimeSpan ToLocalTime(Int64 hour, Int64 min, Double sec)
            => default!;
        /// <summary>
        ///     Create a `cal::relative_duration` value.
        /// </summary>
        public static TimeSpan ToRelativeDuration(Int64 years = 0, Int64 months = 0, Int64 days = 0, Int64 hours = 0, Int64 minutes = 0, Double seconds = 0, Int64 microseconds = 0)
            => default!;
        /// <summary>
        ///     Convert 24-hour chunks into days.
        /// </summary>
        public static TimeSpan DurationNormalizeHours(TimeSpan dur)
            => default!;
        /// <summary>
        ///     Convert 30-day chunks into months.
        /// </summary>
        public static TimeSpan DurationNormalizeDays(TimeSpan dur)
            => default!;
        /// <summary>
        ///     Create a `cal::date_duration` value.
        /// </summary>
        public static TimeSpan ToDateDuration(Int64 years = 0, Int64 months = 0, Int64 days = 0)
            => default!;
    }
}
