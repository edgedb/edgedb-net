#nullable restore
#pragma warning disable
using EdgeDB.Operators;
using EdgeDB.DataTypes;
using System.Numerics;

namespace EdgeDB
{
    public sealed partial class EdgeQL
    {
        #region Generic

        #region Equals
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> ?= <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.GenericEquals))]
        public static bool Equals(object? a, object? b) { return default!; }
        #endregion

        #region NotEqual
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> ?!= <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.GenericNotEqual))]
        public static bool NotEqual(object? a, object? b) { return default!; }
        #endregion

        #region LessThan
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> &lt; <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.GenericLessThan))]
        public static bool LessThan(object? a, object? b) { return default!; }
        #endregion

        #region GreaterThan
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> &gt; <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.GenericGreaterThan))]
        public static bool GreaterThan(object? a, object? b) { return default!; }
        #endregion

        #region LessThanOrEqual
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> &lt;= <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.GenericLessThanOrEqual))]
        public static bool LessThanOrEqual(object? a, object? b) { return default!; }
        #endregion

        #region GreaterThanOrEqual
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> &gt;= <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.GenericGreaterThanOrEqual))]
        public static bool GreaterThanOrEqual(object? a, object? b) { return default!; }
        #endregion

        #region Length
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>len(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.GenericLength))]
        public static long Length(object? a) { return default!; }
        #endregion

        #region Contains
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>contains(<paramref name="source"/>, <paramref name="element"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.GenericContains))]
        public static bool Contains(object? source, object? element) { return default!; }
        #endregion

        #region Find
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>find(<paramref name="source"/>, <paramref name="element"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.GenericFind))]
        public static long IndexOf(object? source, object? element) { return default!; }
        #endregion

        #endregion Generic

        #region string

        #region Index
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/>[<paramref name="b"/>]</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.StringIndex))]
        public static string? Index(string? a, long b) { return default!; }
        #endregion

        #region Slice
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="str"/>[<paramref name="startIndex"/>:{2?}]</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.StringSlice))]
        public static string? Slice(string? str, long startIndex) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="str"/>[<paramref name="startIndex"/>:{2?}]</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.StringSlice))]
        public static string? Slice(string? str, long startIndex, long endIndex) { return default!; }
        #endregion

        #region Concat
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> ++ <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.StringConcat))]
        public static string? Concat(string? a, string? b) { return default!; }
        #endregion

        #region Like
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> like <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.StringLike))]
        public static bool Like(string? a, string? b) { return default!; }
        #endregion

        #region ILike
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> ilike <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.StringILike))]
        public static bool ILike(string? a, string? b) { return default!; }
        #endregion

        #region ToString
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>to_str(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.StringToString))]
        public static string? ToString(object? a) { return default!; }
        #endregion

        #region Length
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>len(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.StringLength))]
        public static long Length(string? a) { return default!; }
        #endregion

        #region Contains
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>contains(<paramref name="a"/>, <paramref name="b"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.StringContains))]
        public static bool Contains(string? a, string? b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>contains(<paramref name="a"/>, <paramref name="b"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.StringContains))]
        public static bool Contains(string? a, char b) { return default!; }
        #endregion

        #region Find
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>find(<paramref name="a"/>, <paramref name="b"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.StringFind))]
        public static long Find(string? a, string? b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>find(<paramref name="a"/>, <paramref name="b"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.StringFind))]
        public static long Find(string? a, char b) { return default!; }
        #endregion

        #region ToLower
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>str_lower(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.StringToLower))]
        public static string? ToLower(string? a) { return default!; }
        #endregion

        #region ToUpper
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>str_upper(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.StringToUpper))]
        public static string? ToUpper(string? a) { return default!; }
        #endregion

        #region ToTitle
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>str_title(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.StringToTitle))]
        public static string? ToTitle(string? a) { return default!; }
        #endregion

        #region PadLeft
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>str_pad_start(<paramref name="a"/>, <paramref name="count"/>, {2?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.StringPadLeft))]
        public static string? PadLeft(string? a, long count) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>str_pad_start(<paramref name="a"/>, <paramref name="count"/>, {2?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.StringPadLeft))]
        public static string? PadLeft(string? a, long count, string? fill) { return default!; }
        #endregion

        #region PadRight
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>str_pad_end(<paramref name="a"/>, <paramref name="count"/>, {2?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.StringPadRight))]
        public static string? PadRight(string? a, long count) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>str_pad_end(<paramref name="a"/>, <paramref name="count"/>, {2?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.StringPadRight))]
        public static string? PadRight(string? a, long count, string? fill) { return default!; }
        #endregion

        #region Trim
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>str_trim(<paramref name="a"/>, {1?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.StringTrim))]
        public static string? Trim(string? a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>str_trim(<paramref name="a"/>, {1?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.StringTrim))]
        public static string? Trim(string? a, string? trimCharacters) { return default!; }
        #endregion

        #region TrimStart
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>str_trim_start(<paramref name="a"/>, {1?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.StringTrimStart))]
        public static string? TrimStart(string? a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>str_trim_start(<paramref name="a"/>, {1?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.StringTrimStart))]
        public static string? TrimStart(string? a, string? trimCharacters) { return default!; }
        #endregion

        #region TrimEnd
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>str_trim_end(<paramref name="a"/>, {1?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.StringTrimEnd))]
        public static string? TrimEnd(string? a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>str_trim_end(<paramref name="a"/>, {1?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.StringTrimEnd))]
        public static string? TrimEnd(string? a, string? trimCharacters) { return default!; }
        #endregion

        #region Repeat
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>str_repeat(<paramref name="a"/>, <paramref name="count"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.StringRepeat))]
        public static string? Repeat(string? a, long count) { return default!; }
        #endregion

        #region Split
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>str_split(<paramref name="a"/>, <paramref name="delimiter"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.StringSplit))]
        public static string?[] Split(string? a, string? delimiter) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>str_split(<paramref name="a"/>, <paramref name="delimiter"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.StringSplit))]
        public static string?[] Split(string? a, char delimiter) { return default!; }
        #endregion

        #region Match
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>re_match(<paramref name="pattern"/>, <paramref name="input"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.StringMatch))]
        public static string?[] Match(string? pattern, string? input) { return default!; }
        #endregion

        #region MatchAll
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>re_match_all(<paramref name="pattern"/>, <paramref name="input"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.StringMatchAll))]
        public static Set<string[]> MatchAll(string? pattern, string? input) { return default!; }
        #endregion

        #region Replace
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>re_replace(<paramref name="pattern"/>, <paramref name="substitute"/>, <paramref name="input"/>, &lt;flags := {3?}&gt;)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.StringReplace))]
        public static string? Replace(string? pattern, string? substitute, string? input, string? flags) { return default!; }
        #endregion

        #region IsMatch
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>re_test(<paramref name="pattern"/>, <paramref name="input"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.StringIsMatch))]
        public static bool IsMatch(string? pattern, string? input) { return default!; }
        #endregion

        #endregion string

        #region boolean

        #region Or
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> or <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.BooleanOr))]
        public static bool Or(bool a, bool b) { return default!; }
        #endregion

        #region And
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> and <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.BooleanAnd))]
        public static bool And(bool a, bool b) { return default!; }
        #endregion

        #region Not
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>not <paramref name="a"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.BooleanNot))]
        public static bool Not(bool a) { return default!; }
        #endregion

        #region All
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>all(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.BooleanAll))]
        public static bool All(IEnumerable<bool> a) { return default!; }
        #endregion

        #region Any
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>any(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.BooleanAny))]
        public static bool Any(IEnumerable<bool> a) { return default!; }
        #endregion

        #endregion boolean

        #region numbers

        #region Add
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> + <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersAdd))]
        public static long Add(long a, long b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> + <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersAdd))]
        public static short Add(short a, short b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> + <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersAdd))]
        public static int Add(int a, int b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> + <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersAdd))]
        public static double Add(double a, double b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> + <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersAdd))]
        public static float Add(float a, float b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> + <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersAdd))]
        public static decimal Add(decimal a, decimal b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> + <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersAdd))]
        public static byte Add(byte a, byte b) { return default!; }
        #endregion

        #region Subtract
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> - <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersSubtract))]
        public static long Subtract(long a, long b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> - <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersSubtract))]
        public static short Subtract(short a, short b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> - <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersSubtract))]
        public static int Subtract(int a, int b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> - <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersSubtract))]
        public static double Subtract(double a, double b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> - <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersSubtract))]
        public static float Subtract(float a, float b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> - <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersSubtract))]
        public static decimal Subtract(decimal a, decimal b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> - <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersSubtract))]
        public static byte Subtract(byte a, byte b) { return default!; }
        #endregion

        #region Negative
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>-<paramref name="a"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersNegative))]
        public static long Negative(long a, long b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>-<paramref name="a"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersNegative))]
        public static short Negative(short a, short b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>-<paramref name="a"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersNegative))]
        public static int Negative(int a, int b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>-<paramref name="a"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersNegative))]
        public static double Negative(double a, double b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>-<paramref name="a"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersNegative))]
        public static float Negative(float a, float b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>-<paramref name="a"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersNegative))]
        public static decimal Negative(decimal a, decimal b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>-<paramref name="a"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersNegative))]
        public static byte Negative(byte a, byte b) { return default!; }
        #endregion

        #region Multiply
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> * <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersMultiply))]
        public static long Multiply(long a, long b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> * <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersMultiply))]
        public static short Multiply(short a, short b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> * <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersMultiply))]
        public static int Multiply(int a, int b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> * <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersMultiply))]
        public static double Multiply(double a, double b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> * <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersMultiply))]
        public static float Multiply(float a, float b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> * <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersMultiply))]
        public static decimal Multiply(decimal a, decimal b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> * <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersMultiply))]
        public static byte Multiply(byte a, byte b) { return default!; }
        #endregion

        #region Divide
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> / <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersDivide))]
        public static long Divide(long a, long b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> / <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersDivide))]
        public static short Divide(short a, short b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> / <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersDivide))]
        public static int Divide(int a, int b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> / <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersDivide))]
        public static double Divide(double a, double b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> / <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersDivide))]
        public static float Divide(float a, float b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> / <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersDivide))]
        public static decimal Divide(decimal a, decimal b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> / <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersDivide))]
        public static byte Divide(byte a, byte b) { return default!; }
        #endregion

        #region Floor
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> // <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersFloor))]
        public static long Floor(long a, long b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> // <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersFloor))]
        public static short Floor(short a, short b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> // <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersFloor))]
        public static int Floor(int a, int b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> // <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersFloor))]
        public static double Floor(double a, double b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> // <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersFloor))]
        public static float Floor(float a, float b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> // <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersFloor))]
        public static decimal Floor(decimal a, decimal b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> // <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersFloor))]
        public static byte Floor(byte a, byte b) { return default!; }
        #endregion

        #region Modulo
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> % <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersModulo))]
        public static long Modulo(long a, long b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> % <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersModulo))]
        public static short Modulo(short a, short b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> % <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersModulo))]
        public static int Modulo(int a, int b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> % <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersModulo))]
        public static double Modulo(double a, double b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> % <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersModulo))]
        public static float Modulo(float a, float b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> % <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersModulo))]
        public static decimal Modulo(decimal a, decimal b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> % <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersModulo))]
        public static byte Modulo(byte a, byte b) { return default!; }
        #endregion

        #region Power
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> ^ <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersPower))]
        public static long Power(long a, long b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> ^ <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersPower))]
        public static short Power(short a, short b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> ^ <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersPower))]
        public static int Power(int a, int b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> ^ <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersPower))]
        public static double Power(double a, double b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> ^ <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersPower))]
        public static float Power(float a, float b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> ^ <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersPower))]
        public static decimal Power(decimal a, decimal b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> ^ <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersPower))]
        public static byte Power(byte a, byte b) { return default!; }
        #endregion

        #region Sum
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>sum(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersSum))]
        public static long Sum(Set<long> a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>sum(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersSum))]
        public static long Sum(Set<int> a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>sum(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersSum))]
        public static decimal Sum(Set<decimal> a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>sum(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersSum))]
        public static float Sum(Set<float> a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>sum(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersSum))]
        public static double Sum(Set<double> a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>sum(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersSum))]
        public static BigInteger Sum(Set<BigInteger> a) { return default!; }
        #endregion

        #region Round
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>round(<paramref name="a"/>, {1?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersRound))]
        public static long Round(long a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>round(<paramref name="a"/>, {1?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersRound))]
        public static long Round(double a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>round(<paramref name="a"/>, {1?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersRound))]
        public static long Round(BigInteger a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>round(<paramref name="a"/>, {1?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersRound))]
        public static long Round(decimal a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>round(<paramref name="a"/>, {1?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersRound))]
        public static long Round(decimal a, long decimalPoint) { return default!; }
        #endregion

        #region Random
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>random()</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersRandom))]
        public static double Random() { return default!; }
        #endregion

        #region ToBigInteger
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>to_bigint(<paramref name="a"/>, {1?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersToBigInteger))]
        public static BigInteger ToBigInteger(string? a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>to_bigint(<paramref name="a"/>, {1?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersToBigInteger))]
        public static BigInteger ToBigInteger(string? a, string? format) { return default!; }
        #endregion

        #region ToDecimal
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>to_decimal(<paramref name="a"/>, {1?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersToDecimal))]
        public static decimal ToDecimal(string? a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>to_decimal(<paramref name="a"/>, {1?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersToDecimal))]
        public static decimal ToDecimal(string? a, string? format) { return default!; }
        #endregion

        #region ToShort
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>to_int16(<paramref name="a"/>, {1?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersToShort))]
        public static short ToShort(string? a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>to_int16(<paramref name="a"/>, {1?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersToShort))]
        public static short ToShort(string? a, string? format) { return default!; }
        #endregion

        #region ToInt
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>to_int32(<paramref name="a"/>, {1?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersToInt))]
        public static int ToInt(string? a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>to_int32(<paramref name="a"/>, {1?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersToInt))]
        public static int ToInt(string? a, string? format) { return default!; }
        #endregion

        #region ToLong
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>to_int64(<paramref name="a"/>, {1?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersToLong))]
        public static long ToLong(string? a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>to_int64(<paramref name="a"/>, {1?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersToLong))]
        public static long ToLong(string? a, string? format) { return default!; }
        #endregion

        #region ToFloat
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>to_float32(<paramref name="a"/>, {1?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersToFloat))]
        public static float ToFloat(string? a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>to_float32(<paramref name="a"/>, {1?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersToFloat))]
        public static float ToFloat(string? a, string? format) { return default!; }
        #endregion

        #region ToDouble
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>to_float64(<paramref name="a"/>, {1?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersToDouble))]
        public static double ToDouble(string? a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>to_float64(<paramref name="a"/>, {1?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.NumbersToDouble))]
        public static double ToDouble(string? a, string? format) { return default!; }
        #endregion

        #endregion numbers

        #region json

        #region Index
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/>[<paramref name="index"/>]</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.JsonIndex))]
        public static Json Index(Json a, long index) { return default!; }
        #endregion

        #region Slice
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/>[<paramref name="b"/>:{2?}]</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.JsonSlice))]
        public static Json Slice(Json a, long b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/>[<paramref name="b"/>:{2?}]</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.JsonSlice))]
        public static Json Slice(Json a, long b, long c) { return default!; }
        #endregion

        #region Concat
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> ++ <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.JsonConcat))]
        public static Json Concat(Json a, Json b) { return default!; }
        #endregion

        #region Index
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/>[<paramref name="b"/>]</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.JsonIndex))]
        public static Json Index(Json a, string? b) { return default!; }
        #endregion

        #region ToJson
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>to_json(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.JsonToJson))]
        public static Json ToJson(string? a) { return default!; }
        #endregion

        #region UnpackJsonArray
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>json_array_unpack(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.JsonUnpackJsonArray))]
        public static Set<Json> UnpackJsonArray(Json a) { return default!; }
        #endregion

        #region JsonGet
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>json_get(<paramref name="a"/>, <paramref name="string[]"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.JsonJsonGet))]
        public static Json? JsonGet(Json a, params string[] path) { return default!; }
        #endregion

        #region UnpackJsonObject
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>json_object_unpack(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.JsonUnpackJsonObject))]
        public static Set<Tuple<string, Json>> UnpackJsonObject(Json a) { return default!; }
        #endregion

        #region JsonTypeof
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>json_typeof(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.JsonJsonTypeof))]
        public static string? JsonTypeof(Json a) { return default!; }
        #endregion

        #endregion json

        #region uuid

        #region GenerateGuid
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>uuid_generate_v1mc()</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.UuidGenerateGuid))]
        public static Guid GenerateGuid() { return default!; }
        #endregion

        #endregion uuid

        #region temporal

        #region Add
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> + <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TemporalAdd))]
        public static DateTimeOffset Add(DateTimeOffset a, TimeSpan b) { return default!; }
        #endregion

        #region Subtract
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> - <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TemporalSubtract))]
        public static TimeSpan Subtract(TimeSpan a, TimeSpan b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> - <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TemporalSubtract))]
        public static DateTimeOffset Subtract(DateTimeOffset a, TimeSpan b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> - <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TemporalSubtract))]
        public static DateTimeOffset Subtract(DateTimeOffset a, DateTimeOffset b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> - <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TemporalSubtract))]
        public static DateTime Subtract(DateTime a, TimeSpan b) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> - <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TemporalSubtract))]
        public static DateTimeOffset Subtract(DateTime a, DateTime b) { return default!; }
        #endregion

        #region GetCurrentDateTime
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>std::datetime_current()</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TemporalGetCurrentDateTime))]
        public static DateTimeOffset GetCurrentDateTime() { return default!; }
        #endregion

        #region GetTransactionDateTime
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>std::datetime_of_transaction()</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TemporalGetTransactionDateTime))]
        public static DateTimeOffset GetTransactionDateTime() { return default!; }
        #endregion

        #region GetStatementDateTime
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>std::datetime_of_statement()</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TemporalGetStatementDateTime))]
        public static DateTimeOffset GetStatementDateTime() { return default!; }
        #endregion

        #region GetDatetimeElement
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>datetime_get(<paramref name="a"/>, <paramref name="b"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TemporalGetDatetimeElement))]
        public static double GetDatetimeElement(DateTimeOffset a, DateTimeElement b) { return default!; }
        #endregion

        #region GetTimespanElement
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>cal::time_get(<paramref name="a"/>, <paramref name="b"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TemporalGetTimespanElement))]
        public static double GetTimespanElement(TimeSpan a, TimeSpanElement b) { return default!; }
        #endregion

        #region GetLocalDateElement
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>cal::date_get(<paramref name="a"/>, <paramref name="b"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TemporalGetLocalDateElement))]
        public static double GetLocalDateElement(DateTime a, LocalDateElement b) { return default!; }
        #endregion

        #region TruncateDateTimeOffset
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>datetime_truncate(<paramref name="a"/>, <paramref name="b"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TemporalTruncateDateTimeOffset))]
        public static DateTimeOffset TruncateDateTimeOffset(DateTimeOffset a, DateTimeTruncateUnit b) { return default!; }
        #endregion

        #region TruncateTimeSpan
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>duration_truncate(<paramref name="a"/>, <paramref name="b"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TemporalTruncateTimeSpan))]
        public static TimeSpan TruncateTimeSpan(TimeSpan a, DurationTruncateUnit b) { return default!; }
        #endregion

        #region ToDateTimeOffset
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>to_datetime(<paramref name="a"/>, {1?}, {2?}, {3?}, {4?}, {5?}, {6?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TemporalToDateTimeOffset))]
        public static DateTimeOffset ToDateTimeOffset(string? a, string? format) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>to_datetime(<paramref name="a"/>, {1?}, {2?}, {3?}, {4?}, {5?}, {6?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TemporalToDateTimeOffset))]
        public static DateTimeOffset ToDateTimeOffset(DateTime a, string? timezone) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>to_datetime(<paramref name="year"/>, {1?}, {2?}, {3?}, {4?}, {5?}, {6?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TemporalToDateTimeOffset))]
        public static DateTimeOffset ToDateTimeOffset(long year, long month, long day, long hour, long min, long sec, string? timezone) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>to_datetime(<paramref name="unixSeconds"/>, {1?}, {2?}, {3?}, {4?}, {5?}, {6?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TemporalToDateTimeOffset))]
        public static DateTimeOffset ToDateTimeOffset(decimal unixSeconds) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>to_datetime(<paramref name="unixSeconds"/>, {1?}, {2?}, {3?}, {4?}, {5?}, {6?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TemporalToDateTimeOffset))]
        public static DateTimeOffset ToDateTimeOffset(double unixSeconds) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>to_datetime(<paramref name="unixSeconds"/>, {1?}, {2?}, {3?}, {4?}, {5?}, {6?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TemporalToDateTimeOffset))]
        public static DateTimeOffset ToDateTimeOffset(long unixSeconds) { return default!; }
        #endregion

        #region ToDateTime
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>cal::to_local_datetime(<paramref name="a"/>, {1?}, {2?}, {3?}, {4?}, {5?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TemporalToDateTime))]
        public static DateTime ToDateTime(string? a, string? format) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>cal::to_local_datetime(<paramref name="a"/>, {1?}, {2?}, {3?}, {4?}, {5?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TemporalToDateTime))]
        public static DateTime ToDateTime(DateTimeOffset a, string? timezone) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>cal::to_local_datetime(<paramref name="year"/>, {1?}, {2?}, {3?}, {4?}, {5?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TemporalToDateTime))]
        public static DateTime ToDateTime(long year, long month, long day, long hour, long min, long sec) { return default!; }
        #endregion

        #region ToLocalDate
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>cal::to_local_date(<paramref name="a"/>, {1?}, {2?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TemporalToLocalDate))]
        public static DateTime ToLocalDate(string? a, string? format) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>cal::to_local_date(<paramref name="a"/>, {1?}, {2?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TemporalToLocalDate))]
        public static DateTime ToLocalDate(DateTimeOffset a, string? timezone) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>cal::to_local_date(<paramref name="year"/>, {1?}, {2?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TemporalToLocalDate))]
        public static DateTime ToLocalDate(long year, long month, long day) { return default!; }
        #endregion

        #region ToLocalTime
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>cal::to_local_time(<paramref name="a"/>, {1?}, {2?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TemporalToLocalTime))]
        public static TimeSpan ToLocalTime(string? a, string? format) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>cal::to_local_time(<paramref name="a"/>, {1?}, {2?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TemporalToLocalTime))]
        public static TimeSpan ToLocalTime(DateTimeOffset a, string? timezone) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>cal::to_local_time(<paramref name="hour"/>, {1?}, {2?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TemporalToLocalTime))]
        public static TimeSpan ToLocalTime(long hour, long minute, double second) { return default!; }
        #endregion

        #region ToTimeSpan
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>to_duration(&lt;hours := {0?}&gt;, &lt;minutes := {1?}&gt;, &lt;seconds := {2?}&gt; &lt;microseconds := {3?}&gt;)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TemporalToTimeSpan))]
        public static TimeSpan ToTimeSpan(long? hours = null, long? minutes = null, double? seconds = null, double? microseconds = null) { return default!; }
        #endregion

        #region TimeSpanToSeconds
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>std::duration_to_seconds(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TemporalTimeSpanToSeconds))]
        public static decimal TimeSpanToSeconds(TimeSpan a) { return default!; }
        #endregion

        #region ToRelativeDuration
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>cal::to_relative_duration(&lt;years := {0?}&gt;, &lt;months := {1?}&gt;, &lt;days := {2?}&gt;, &lt;hours := {3?}&gt;, &lt;minutes := {4?}&gt;, &lt;seconds := {5?}&gt;, &lt;microseconds := {6?}&gt;)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TemporalToRelativeDuration))]
        public static TimeSpan ToRelativeDuration(long? years = null, long? months = null, long? days = null, long? hours = null, long? minutes = null, long? seconds = null, long? microseconds = null) { return default!; }
        #endregion

        #endregion temporal

        #region bytes

        #region Index
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/>[<paramref name="index"/>]</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.BytesIndex))]
        public static byte[] Index(byte[] a, long index) { return default!; }
        #endregion

        #region Slice
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/>[<paramref name="startIndex"/>:{2?}]</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.BytesSlice))]
        public static byte[] Slice(byte[] a, long startIndex) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/>[<paramref name="startIndex"/>:{2?}]</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.BytesSlice))]
        public static byte[] Slice(byte[] a, long startIndex, long endIndex) { return default!; }
        #endregion

        #region Concat
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> ++ <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.BytesConcat))]
        public static byte[] Concat(byte[] a, byte[] b) { return default!; }
        #endregion

        #region GetBit
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>bytes_get_bit(<paramref name="a"/>, <paramref name="bitIndex"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.BytesGetBit))]
        public static long GetBit(byte[] a, long bitIndex) { return default!; }
        #endregion

        #endregion bytes

        #region sequence

        #region IncrementSequence
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>sequence_next(&lt;introspect typeof <paramref name="seq"/>&gt;)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.SequenceIncrementSequence))]
        public static long IncrementSequence(long seq) { return default!; }
        #endregion

        #region ResetSequence
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>sequence_reset(&lt;introspect typeof <paramref name="seq"/>&gt;, {1?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.SequenceResetSequence))]
        public static long ResetSequence(long seq) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>sequence_reset(&lt;introspect typeof <paramref name="seq"/>&gt;, {1?})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.SequenceResetSequence))]
        public static long ResetSequence(long seq, long resetTo) { return default!; }
        #endregion

        #endregion sequence

        #region array

        #region Index<TType>
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/>[<paramref name="index"/>]</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.ArrayIndex))]
        public static TType Index<TType>(IEnumerable<TType> a, long index) { return default!; }
        #endregion

        #region Slice<TType>
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/>[<paramref name="startIndex"/>:{2?}]</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.ArraySlice))]
        public static IEnumerable<TType> Slice<TType>(IEnumerable<TType> a, long startIndex) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/>[<paramref name="startIndex"/>:{2?}]</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.ArraySlice))]
        public static IEnumerable<TType> Slice<TType>(IEnumerable<TType> a, long startIndex, long endIndex) { return default!; }
        #endregion

        #region Concat<TType>
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> ++ <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.ArrayConcat))]
        public static IEnumerable<TType> Concat<TType>(IEnumerable<TType> a, IEnumerable<TType> b) { return default!; }
        #endregion

        #region Aggregate<TType>
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>array_agg(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.ArrayAggregate))]
        public static IEnumerable<TType> Aggregate<TType>(Set<TType> a) { return default!; }
        #endregion

        #region IndexOrDefault<TType>
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>array_get(<paramref name="a"/>, <paramref name="index"/>, &lt;default := {2?}&gt;)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.ArrayIndexOrDefault))]
        public static TType? IndexOrDefault<TType>(IEnumerable<TType> a, long index, TType? defaultValue = default) { return default!; }
        #endregion

        #region UnpackArray<TType>
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>array_unpack(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.ArrayUnpackArray))]
        public static Set<TType> UnpackArray<TType>(IEnumerable<TType> a) { return default!; }
        #endregion

        #region Join
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>array_join(<paramref name="a"/>, <paramref name="delimiter"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.ArrayJoin))]
        public static string? Join(IEnumerable<string> a, string? delimiter) { return default!; }
        #endregion

        #endregion array

        #region sets

        #region Distinct<TType>
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>distinct <paramref name="a"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.SetsDistinct))]
        public static Set<TType> Distinct<TType>(Set<TType> a) { return default!; }
        #endregion

        #region Contains<TType>
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="element"/> in <paramref name="a"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.SetsContains))]
        public static bool Contains<TType>(Set<TType> a, TType element) { return default!; }
        #endregion

        #region Union<TType>
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> union <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.SetsUnion))]
        public static Set<TType> Union<TType>(Set<TType> a, Set<TType> b) { return default!; }
        #endregion

        #region Conditional<TType>
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="trueReturn"/> if <paramref name="condition"/> else <paramref name="falseReturn"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.SetsConditional))]
        public static TType Conditional<TType>(bool condition, TType trueReturn, TType falseReturn) { return default!; }
        #endregion

        #region Coalesce<TType>
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> ?? <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.SetsCoalesce))]
        public static TType Coalesce<TType>(TType? a, TType b) { return default!; }
        #endregion

        #region Detached<TType>
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>detached <paramref name="a"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.SetsDetached))]
        public static TType Detached<TType>(TType a) { return default!; }
        #endregion

        #region NotNull<TType>
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>exists <paramref name="value"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.SetsNotNull))]
        public static bool NotNull<TType>(TType value) { return default!; }
        #endregion

        #region CastIfTypeIs<TDesired>
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/>[is <typeparamref name="TDesired"/>]</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.SetsCastIfTypeIs))]
        [ParameterMap(1, "TDesired")]
        public static TDesired CastIfTypeIs<TDesired>(object? a) { return default!; }
        #endregion

        #region AssertDistinct<TType>
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>assert_distinct(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.SetsAssertDistinct))]
        public static Set<TType> AssertDistinct<TType>(Set<TType> a) { return default!; }
        #endregion

        #region AssertSingle<TType>
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>assert_single(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.SetsAssertSingle))]
        public static TType AssertSingle<TType>(Set<TType> a) { return default!; }
        #endregion

        #region AssertNotNull<TType>
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>assert_exists(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.SetsAssertNotNull))]
        public static TType AssertNotNull<TType>(TType? a) { return default!; }
        #endregion

        #region Count<TType>
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>count(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.SetsCount))]
        public static long Count<TType>(Set<TType> a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>count(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.SetsCount))]
        public static long Count<TType>(IEnumerable<TType> a) { return default!; }
        #endregion

        #region Enumerate<TType>
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>enumerate(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.SetsEnumerate))]
        public static Set<Tuple<long, TType>> Enumerate<TType>(Set<TType> a) { return default!; }
        #endregion

        #region Min<TType>
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>min(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.SetsMin))]
        public static TType Min<TType>(Set<TType> a) { return default!; }
        #endregion

        #region Max<TType>
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>max(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.SetsMax))]
        public static TType Max<TType>(Set<TType> a) { return default!; }
        #endregion

        #endregion sets

        #region types

        #region Is<TType>
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> is <typeparamref name="TType"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TypesIs))]
        [ParameterMap(1, "TType")]
        public static bool Is<TType>(object? a) { return default!; }
        #endregion

        #region Is
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> is <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TypesIs))]
        public static bool Is(object? a, Type b) { return default!; }
        #endregion

        #region IsNot<TType>
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> is not <typeparamref name="TType"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TypesIsNot))]
        [ParameterMap(1, "TType")]
        public static bool IsNot<TType>(object? a) { return default!; }
        #endregion

        #region TypeUnion
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>(<paramref name="a"/> | <paramref name="b"/> { | :2+})</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TypesTypeUnion))]
        public static Type TypeUnion(Type a, Type b, params Type[] additional) { return default!; }
        #endregion

        #region Cast<TType>
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>&lt;<paramref name="a"/>&gt;<paramref name="a"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TypesCast))]
        [ParameterMap(0, "TType")]
        public static TType Cast<TType>(object? a) { return default!; }
        #endregion

        #region GetType
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>introspect (typeof <paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TypesGetType))]
        public static Type GetType(object? a) { return default!; }
        #endregion

        #region IsTypeOf
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> is typeof <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TypesIsTypeOf))]
        public static bool IsTypeOf(object? a, object? b) { return default!; }
        #endregion

        #region IsNotTypeOf
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code><paramref name="a"/> is not typeof <paramref name="b"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TypesIsNotTypeOf))]
        public static bool IsNotTypeOf(object? a, object? b) { return default!; }
        #endregion

        #region Introspect
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>introspect <paramref name="a"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.TypesIntrospect))]
        public static Type Introspect(Type a) { return default!; }
        #endregion

        #endregion types

        #region math

        #region Abs
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::abs(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathAbs))]
        public static short Abs(short a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::abs(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathAbs))]
        public static int Abs(int a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::abs(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathAbs))]
        public static long Abs(long a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::abs(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathAbs))]
        public static float Abs(float a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::abs(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathAbs))]
        public static double Abs(double a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::abs(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathAbs))]
        public static decimal Abs(decimal a) { return default!; }
        #endregion

        #region Ceil
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::ceil(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathCeil))]
        public static double Ceil(long a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::ceil(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathCeil))]
        public static double Ceil(double a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::ceil(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathCeil))]
        public static BigInteger Ceil(BigInteger a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::ceil(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathCeil))]
        public static decimal Ceil(decimal a) { return default!; }
        #endregion

        #region Floor
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::floor(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathFloor))]
        public static double Floor(long a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::floor(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathFloor))]
        public static double Floor(double a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::floor(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathFloor))]
        public static BigInteger Floor(BigInteger a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::floor(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathFloor))]
        public static decimal Floor(decimal a) { return default!; }
        #endregion

        #region NaturalLog
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::ln(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathNaturalLog))]
        public static double NaturalLog(long a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::ln(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathNaturalLog))]
        public static double NaturalLog(double a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::ln(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathNaturalLog))]
        public static decimal NaturalLog(decimal a) { return default!; }
        #endregion

        #region Logarithm
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::lg(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathLogarithm))]
        public static double Logarithm(long a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::lg(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathLogarithm))]
        public static double Logarithm(double a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::lg(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathLogarithm))]
        public static decimal Logarithm(decimal a) { return default!; }
        #endregion

        #region Logarithm
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::log(<paramref name="a"/> &lt;base := <paramref name="numericBase"/>&gt;)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathLogarithm))]
        public static decimal Logarithm(decimal a, decimal numericBase) { return default!; }
        #endregion

        #region Mean
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::mean(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathMean))]
        public static double Mean(Set<long> a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::mean(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathMean))]
        public static double Mean(Set<double> a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::mean(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathMean))]
        public static decimal Mean(Set<decimal> a) { return default!; }
        #endregion

        #region StandardDeviation
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::stddev(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathStandardDeviation))]
        public static double StandardDeviation(Set<long> a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::stddev(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathStandardDeviation))]
        public static double StandardDeviation(Set<double> a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::stddev(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathStandardDeviation))]
        public static decimal StandardDeviation(Set<decimal> a) { return default!; }
        #endregion

        #region StandardDeviationPop
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::stddev_pop(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathStandardDeviationPop))]
        public static double StandardDeviationPop(Set<long> a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::stddev_pop(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathStandardDeviationPop))]
        public static double StandardDeviationPop(Set<double> a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::stddev_pop(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathStandardDeviationPop))]
        public static decimal StandardDeviationPop(Set<decimal> a) { return default!; }
        #endregion

        #region Variance
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::var(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathVariance))]
        public static double Variance(Set<long> a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::var(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathVariance))]
        public static double Variance(Set<double> a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::var(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathVariance))]
        public static decimal Variance(Set<decimal> a) { return default!; }
        #endregion

        #region VariancePop
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::var_pop(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathVariancePop))]
        public static double VariancePop(Set<long> a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::var_pop(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathVariancePop))]
        public static double VariancePop(Set<double> a) { return default!; }
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>math::var_pop(<paramref name="a"/>)</code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.MathVariancePop))]
        public static decimal VariancePop(Set<decimal> a) { return default!; }
        #endregion

        #endregion math

        #region links

        #region AddLink<TSource, TType>
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>+= <paramref name="element"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.LinksAddLink))]
        public static TSource AddLink<TSource, TType>(TSource source, TType element) where TSource : IEnumerable<TType>? { return default!; }
        #endregion

        #region RemoveLink<TSource, TType>
        /// <summary>
        ///     A function that represents the EdgeQL version of: <code>-= <paramref name="element"/></code>
        /// </summary>
        [EquivalentOperator(typeof(EdgeDB.Operators.LinksRemoveLink))]
        public static TSource RemoveLink<TSource, TType>(TSource source, TType element) where TSource : IEnumerable<TType>? { return default!; }
        #endregion

        #endregion links

        #region variables

        #endregion variables

        internal static Dictionary<string, IEdgeQLOperator> FunctionOperators = new()
        {
            { "String.ToLower", new StringToLower()},
            { "String.ToUpper", new StringToUpper()},
            { "Enumerable.Concat", new SetsUnion()},
        }
;
    }
}
#nullable restore
#pragma warning restore
