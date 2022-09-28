using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal sealed class PascalNamingStrategy : INamingStrategy
    {
        public string Convert(PropertyInfo property)
            => Convert(property.Name);

        public string Convert(string name)
        {
            var sample = string.Join("", name.Select(c => Char.IsLetterOrDigit(c) ? c.ToString().ToLower() : "_").ToArray());

            var arr = sample
                .Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => $"{s[..1].ToUpper()}{s[1..]}");

            return string.Join("", arr);
        }
    }
}
