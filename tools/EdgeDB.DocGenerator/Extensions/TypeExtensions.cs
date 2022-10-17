using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DocGenerator
{
    public static class TypeExtensions
    {
        public static string ToFormattedString(this Type type, bool includeParents = false)
        {
            switch (type.Name)
            {
                case "String":
                    return "string";
                case "Int32":
                    return "int";
                case "Decimal":
                    return "decimal";
                case "Object":
                    return "object";
                case "Void":
                    return "void";
                default:
                    {
                        return includeParents ? type.FullName ?? type.Name : type.Name;
                    }
            }
        }
    }
}
