using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdUuid : MethodTranslator<Guid>
    {
        [MethodName(EdgeQL.UuidGenerateV1mc)]
        public string UuidGenerateV1mc()
        {
            return $"std::uuid_generate_v1mc()";
        }

        [MethodName(EdgeQL.UuidGenerateV4)]
        public string UuidGenerateV4()
        {
            return $"std::uuid_generate_v4()";
        }

    }
}
