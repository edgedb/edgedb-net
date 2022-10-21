using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EdgeDB.DocGenerator
{
    public static class TypeExtensions
    {
        public static string ToFormattedString(this Type type, bool includeParents = false)
        {
            var name = type.Name switch
            {
                "String" => "string",
                "Int16" => "short",
                "Int32" => "int",
                "Int64" => "long",
                "Boolean" => "bool",
                "Object" => "object",
                "Void" => "void",
                "Byte" => "byte",
                "SByte" => "sbyte",
                "UInt16" => "ushort",
                "UInt32" => "uint",
                "UInt64" => "ulong",
                "Char" => "char",
                "Decimal" => "decimal",
                "Double" => "double",
                "Single" => "float",
                _ => includeParents ? type.FullName ?? type.Name : type.Name
            };

            return Regex.Replace(name, @"`\d+", m => $"<{string.Join(", ", type.GetGenericArguments().Select(t => t.ToFormattedString()))}>");
        }
    }
}
