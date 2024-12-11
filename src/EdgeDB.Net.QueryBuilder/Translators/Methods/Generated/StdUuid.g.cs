#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace EdgeDB.Translators
{
    internal partial class StdUuidMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.UuidGenerateV1mc))]
        public void UuidGenerateV1mcTranslator(QueryWriter writer, MethodInfo method)
        {
            writer.Function("std::uuid_generate_v1mc", debug: null, metadata: new FunctionMetadata("std::uuid_generate_v1mc", method));
        }

        [MethodName(nameof(EdgeQL.UuidGenerateV4))]
        public void UuidGenerateV4Translator(QueryWriter writer, MethodInfo method)
        {
            writer.Function("std::uuid_generate_v4", debug: null, metadata: new FunctionMetadata("std::uuid_generate_v4", method));
        }

    }
}
