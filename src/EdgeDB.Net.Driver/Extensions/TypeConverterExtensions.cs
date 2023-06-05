using EdgeDB.Binary;
using EdgeDB.TypeConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal static class TypeConverterExtensions
    {
        internal static void ValidateTargetType(this IEdgeDBTypeConverter converter)
        {
            if (!CodecBuilder.ContainsScalarCodec(converter.Target))
                throw new MissingCodecException($"Cannot use {converter.Target.Name} as a target type because it is not a scalar type.");
        }
    }
}
