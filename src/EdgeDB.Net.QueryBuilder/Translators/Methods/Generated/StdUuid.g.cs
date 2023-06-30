#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdUuidMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.UuidGenerateV1mc))]
        public string UuidGenerateV1mcTranslator()
        {
            return $"std::uuid_generate_v1mc()";
        }

        [MethodName(nameof(EdgeQL.UuidGenerateV4))]
        public string UuidGenerateV4Translator()
        {
            return $"std::uuid_generate_v4()";
        }

    }
}
