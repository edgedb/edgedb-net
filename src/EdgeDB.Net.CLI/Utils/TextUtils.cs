﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EdgeDB.CLI.Utils
{
    /// <summary>
    ///     A utility class containing methods related to text operations.
    /// </summary>
    internal static class TextUtils
    {
        /// <summary>
        ///     The current culture info.
        /// </summary>
        private static CultureInfo? _cultureInfo;

        /// <summary>
        ///     Converts the given string to pascal case.
        /// </summary>
        /// <param name="input">The string to convert to pascal case.</param>
        /// <returns>A pascal-cased version of the input string.</returns>
        public static string ToPascalCase(string input)
        {
            _cultureInfo ??= CultureInfo.CurrentCulture;
            var t = Regex.Replace(input, @"[^^]([A-Z])", m => $"{m.Value[0]} {m.Groups[1].Value}");
            
            return _cultureInfo.TextInfo.ToTitleCase(t.Replace("_", " ")).Replace(" ", "");
        }

        /// <summary>
        ///     Converts the given string to camel case.
        /// </summary>
        /// <param name="input">The string to convert to pascal case.</param>
        /// <returns>A camel-cased version of the input string.</returns>
        public static string ToCamelCase(string input)
        {
            var p = ToPascalCase(input);
            return $"{p[0].ToString().ToLower()}{p[1..]}";
        }
    }
}
