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
        /// <param name="writer">The query string writer to append the translated method to.</param>
        /// <param name="input">The input string to test against.</param>
        /// <param name="pattern">The regular expression pattern.</param>
        /// <param name="replacement">The replacement value to replace matches with.</param>
        /// <returns>The EdgeQL equivalent of the method.</returns>
        [MethodName(nameof(Regex.Replace))]
        public void Replace(QueryStringWriter writer, TranslatedParameter input, TranslatedParameter pattern,
            TranslatedParameter replacement)
            => writer.Function("re_replace", pattern, replacement, input);

        /// <summary>
        ///     Translates the method <see cref="Regex.IsMatch(string, string)"/>.
        /// </summary>
        /// <param name="writer">The query string writer to append the translated method to.</param>
        /// <param name="testString">The string to test against.</param>
        /// <param name="pattern">The regex pattern.</param>
        /// <returns>The EdgeQL equivalent of the method.</returns>
        [MethodName(nameof(Regex.IsMatch))]
        public void Test(QueryStringWriter writer, TranslatedParameter testString, TranslatedParameter pattern)
            => writer.Function("re_test", pattern, testString);
    }
}
