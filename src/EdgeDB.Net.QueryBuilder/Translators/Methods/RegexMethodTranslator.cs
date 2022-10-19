using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EdgeDB.Translators.Methods
{
    /// <summary>
    ///     Represents a translator for translating methods within the <see cref="Regex"/> class.
    /// </summary>
    internal class RegexMethodTranslator : MethodTranslator<Regex>
    {
        /// <summary>
        ///     Translates the method <see cref="Regex.Replace(string, string, string)"/>.
        /// </summary>
        /// <param name="input">The input string to test against.</param>
        /// <param name="pattern">The regular expression pattern.</param>
        /// <param name="replacement">The replacement value to replace matches with.</param>
        /// <returns>The EdgeQL equivalent of the method.</returns>
        [MethodName(nameof(Regex.Replace))]
        public string Replace(string input, string pattern, string replacement)
            => $"re_replace({pattern}, {replacement}, {input})";

        /// <summary>
        ///     Translates the method <see cref="Regex.IsMatch(string, string)"/>.
        /// </summary>
        /// <param name="testString">The string to test against.</param>
        /// <param name="pattern">The regex pattern.</param>
        /// <returns>The EdgeQL equivalent of the method.</returns>
        [MethodName(nameof(Regex.IsMatch))]
        public string Test(string testString, string pattern)
            => $"re_test({pattern}, {testString})";
    }
}
