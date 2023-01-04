using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal sealed class SnakeCaseNamingStrategy : INamingStrategy
    {
        public string Convert(MemberInfo member)
            => Convert(member.Name);

        public string Convert(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            var upperCaseLength = name.Where((c, i) => c is >= 'A' and <= 'Z' && i != 0).Count();

            if (upperCaseLength == 0)
                return name.ToLower();

            var bufferSize = name.Length + upperCaseLength;
            Span<char> buffer = stackalloc char[bufferSize];
            var bufferPosition = 0;
            var namePosition = 0;
            while (bufferPosition < buffer.Length)
            {
                if (namePosition > 0 && name[namePosition] >= 'A' && name[namePosition] <= 'Z')
                {
                    buffer[bufferPosition] = '_';
                    buffer[bufferPosition + 1] = name[namePosition];
                    bufferPosition += 2;
                    namePosition++;
                    continue;
                }
                buffer[bufferPosition] = name[namePosition];
                bufferPosition++;
                namePosition++;
            }

            return new string(buffer).ToLower();
        }
            
    }
}
