using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public struct BuiltQuery
    {
        public string QueryText { get; set; }
        public IEnumerable<KeyValuePair<string, object?>> Parameters { get; set; } = new Dictionary<string, object?>();

        public string Prettify()
        {
            int tabIndex = 0;
            bool escaped = false;
            var raw = QueryText;
            var result = "";

            for (int i = 0; i != raw.Length; i++)
            {
                var c = raw[i];
                var prev = i == 0 ? '\0' : raw[i - 1];
                var prevFormatted = i == 0 ? '\0' : result.Last();
                var next = i == raw.Length - 1 ? '\0' : raw[i + 1];

                if (c == '\"')
                    escaped = !escaped;
                else if (escaped)
                {
                    result += c;
                    continue;
                }

                string padChar = "".PadLeft(tabIndex);

                if (
                    (c == ' ' && (next == '{' || next == '}')) ||
                    (c == ' ' && prevFormatted == ' ')
                )
                {
                    continue;
                }

                if (prevFormatted == '\n' && c != ',' && prev != ' ')
                    result += padChar;
                if (c == ',')
                {
                    result += $"{c}\n{padChar}";
                }
                else if (c == '{' || c == '(')
                {
                    tabIndex += 2;
                    result += $"{(c != '(' ? $"\n{padChar}" : "")}{c}\n{"".PadLeft(tabIndex)}";
                }
                else if (c == '}' || c == ')')
                {
                    tabIndex -= 2;
                    result += $"{((prevFormatted != ' ' && prevFormatted != '\n') ? $"\n{"".PadLeft(tabIndex)}" : "")}{c}{(next != ',' ? '\n' : "")}";
                }
                else result += c;
            }

            return result;
        }
    }
}
