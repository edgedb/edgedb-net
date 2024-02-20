#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdUuidMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.UuidGenerateV1mc))]
        public void UuidGenerateV1mcTranslator(QueryWriter writer)
        {
            writer.Function("std::uuid_generate_v1mc");
        }

        [MethodName(nameof(EdgeQL.UuidGenerateV4))]
        public void UuidGenerateV4Translator(QueryWriter writer)
        {
            writer.Function("std::uuid_generate_v4");
        }

    }
}
