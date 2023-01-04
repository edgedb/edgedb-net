using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Translators.Methods
{
    /// <summary>
    ///     Represents a translator for translating methods within the <see cref="string"/> class.
    /// </summary>
    internal class StringMethodTranslators : MethodTranslator<string>
    {
        /// <summary>
        ///     Translates the method <see cref="string.Concat(object?, object?)"/>.
        /// </summary>
        /// <param name="instance">The instance of the string to concat agains.</param>
        /// <param name="variableArgs">The variable arguments that should be concatenated together.</param>
        /// <returns>The EdgeQL equivalent of the method.</returns>
        [MethodName(nameof(string.Concat))]
        public string Concat(string? instance, params string[] variableArgs)
        {
            if (instance is not null)
                return $"{instance} ++ {string.Join(" ++ ", variableArgs)}";

            return string.Join(" ++ ", variableArgs);
        }

        /// <summary>
        ///     Translates the method <see cref="string.Contains(string)"/>.
        /// </summary>
        /// <param name="instance">The instance of the string to concat agains.</param>
        /// <param name="target">The value to check whether or not its within the instance</param>
        /// <returns>The EdgeQL equivalent of the method.</returns>
        [MethodName(nameof(string.Contains))]
        public string Contains(string instance, string target)
            => $"contains({instance}, {target})";

        /// <summary>
        ///     Translates the method <see cref="string.IndexOf(string)"/>.
        /// </summary>
        /// <param name="instance">The instance of the string.</param>
        /// <param name="target">The target substring to find within the instance.</param>
        /// <returns>The EdgeQL equivalent of the method.</returns>
        [MethodName(nameof(string.IndexOf))]
        public string Find(string instance, string target)
            => $"find({instance}, {target})";

        /// <summary>
        ///     Translates the method <see cref="string.ToLower()"/>.
        /// </summary>
        /// <param name="instance">The instance of the string.</param>
        /// <returns>The EdgeQL equivalent of the method.</returns>
        [MethodName(nameof(string.ToLower))]
        [MethodName(nameof(string.ToLowerInvariant))]
        public string ToLower(string instance)
            => $"str_lower({instance})";

        /// <summary>
        ///     Translates the method <see cref="string.ToUpper()"/>.
        /// </summary>
        /// <param name="instance">The instance of the string.</param>
        /// <returns>The EdgeQL equivalent of the method.</returns>
        [MethodName(nameof(string.ToUpper))]
        [MethodName(nameof(string.ToUpperInvariant))]
        public string ToUpper(string instance)
            => $"str_upper({instance})";

        /// <summary>
        ///     Translates the method <see cref="string.PadLeft(int)"/>.
        /// </summary>
        /// <param name="instance">The instance of the string.</param>
        /// <param name="amount">The amount to pad left</param>
        /// <param name="fill">The fill character to pad with</param>
        /// <returns>The EdgeQL equivalent of the method.</returns>
        [MethodName(nameof(string.PadLeft))]
        public string PadLeft(string instance, string amount, string? fill)
            => $"str_pad_start({instance}, {amount}{OptionalArg(fill)})";

        /// <summary>
        ///     Translates the method <see cref="string.PadRight(int)"/>.
        /// </summary>
        /// <param name="instance">The instance of the string.</param>
        /// <param name="amount">The amount to pad left</param>
        /// <param name="fill">The fill character to pad with</param>
        /// <returns>The EdgeQL equivalent of the method.</returns>
        [MethodName(nameof(string.PadRight))]
        public string PadRight(string instance, string amount, string? fill)
            => $"str_pad_end({instance}, {amount}{OptionalArg(fill)})";

        /// <summary>
        ///     Translates the method <see cref="string.Trim()"/>.
        /// </summary>
        /// <param name="instance">The instance of the string.</param>
        /// <param name="trimChars">The characters to trim.</param>
        /// <returns>The EdgeQL equivalent of the method.</returns>
        [MethodName(nameof(string.Trim))]
        public string Trim(string instance, params string[]? trimChars)
        {
            if (trimChars != null && trimChars.Any())
                return $"str_trim({instance}, '{string.Join("", trimChars.Select(x => x.Replace("\"", "")))}')";
            return $"str_trim({instance})";
        }

        /// <summary>
        ///     Translates the method <see cref="string.TrimStart()"/>.
        /// </summary>
        /// <param name="instance">The instance of the string.</param>
        /// <param name="trimChars">The characters to trim.</param>
        /// <returns>The EdgeQL equivalent of the method.</returns>
        [MethodName(nameof(string.TrimStart))]
        public string TrimStart(string instance, params string[] trimChars)
        {
            if (trimChars != null && trimChars.Any())
                return $"str_trim_start({instance}, '{string.Join("", trimChars.Select(x => x.Replace("\"", "")))}')";
            return $"str_trim_start({instance})";
        }

        /// <summary>
        ///     Translates the method <see cref="string.TrimEnd()"/>.
        /// </summary>
        /// <param name="instance">The instance of the string.</param>
        /// <param name="trimChars">The characters to trim.</param>
        /// <returns>The EdgeQL equivalent of the method.</returns>
        [MethodName(nameof(string.TrimEnd))]
        public string TrimEnd(string instance, params string[] trimChars)
        {
            if (trimChars != null && trimChars.Any())
                return $"str_trim_end({instance}, '{string.Join("", trimChars.Select(x => x.Replace("\"", "")))}')";
            return $"str_trim_end({instance})";
        }

        /// <summary>
        ///     Translates the method <see cref="string.Replace(char, char)"/>.
        /// </summary>
        /// <param name="instance">The instance of the string.</param>
        /// <param name="old">The old string to replace.</param>
        /// <param name="newStr">The new string to replace the old one.</param>
        /// <returns>The EdgeQL equivalent of the method.</returns>
        [MethodName(nameof(string.Replace))]
        public string Replace(string instance, string old, string newStr)
            => $"str_replace({instance}, {old}, {newStr})";

        /// <summary>
        ///     Translates the method <see cref="string.Split(char[])"/>.
        /// </summary>
        /// <param name="instance">The instance of the string.</param>
        /// <param name="separator">The char to split by.</param>
        /// <returns>The EdgeQL equivalent of the method.</returns>
        [MethodName(nameof(string.Split))]
        public string Split(string instance, string separator)
            => $"str_split({instance}, {separator})";
    }
}
