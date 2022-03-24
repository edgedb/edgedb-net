using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EdgeDB
{
    public sealed class BuiltQuery
    {
        public string QueryText { get; set; } = "";
        public IEnumerable<KeyValuePair<string, object?>> Parameters { get; set; } = new Dictionary<string, object?>();

        public string Prettify()
        {
            // add newlines
            var result = Regex.Replace(QueryText, @"({|\(|\)|}|,)", m =>
            {
                switch (m.Groups[1].Value)
                {
                    case "{" or "(" or ",":
                        if (m.Groups[1].Value == "{" && QueryText[m.Index + 1] == '}')
                            return m.Groups[1].Value;

                        return $"{m.Groups[1].Value}\n";

                    default:
                        return $"{((m.Groups[1].Value == "}" && (QueryText[m.Index - 1] == '{' || QueryText[m.Index - 1] == '}')) ? "" : "\n")}{m.Groups[1].Value}{((QueryText.Length == m.Index + 1 ? false : (QueryText[m.Index + 1] != ',')) ? "\n" : "")}";
                }
            }).Trim().Replace("\n ", "\n");

            // clean up newline func
            result = Regex.Replace(result, "\n\n", m => "\n");

            // add indentation
            result = Regex.Replace(result, "^", m =>
            {
                int indent = 0;

                foreach(var c in result.Substring(0, m.Index))
                {
                    if (c == '(' || c == '{')
                        indent++;
                    if (c == ')' || c == '}')
                        indent--;
                }

                var next = result.Length != m.Index ? result[m.Index] : '\0';

                if (next == '}' || next == ')')
                    indent--;

                return "".PadLeft(indent * 2);
            }, RegexOptions.Multiline);

            return result;
        }
    }
}
