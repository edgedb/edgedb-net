using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal static class MemberInfoExtensions
    {
        public static Type? GetMemberType(this MemberInfo info)
        {
            switch (info.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)info).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)info).PropertyType;
            }

            return null;
        }
    }
}
