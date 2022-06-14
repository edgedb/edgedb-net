using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Serializer
{
    public sealed class PascalNamingStrategy : INamingStrategy
    {
        public string GetName(PropertyInfo property)
        {
            var str = property.Name;

            string sample = string.Join("", str.Select(c => Char.IsLetterOrDigit(c) ? c.ToString().ToLower() : "_").ToArray());
            
            var arr = sample
                .Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => $"{s.Substring(0, 1).ToUpper()}{s.Substring(1)}");

            sample = string.Join("", arr);

            return sample;
        }
    }
}
