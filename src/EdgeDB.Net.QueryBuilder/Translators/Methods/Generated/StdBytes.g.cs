#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace EdgeDB.Translators
{
    internal partial class StdBytesMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.ToBytes))]
        public void ToBytesTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter sParam)
        {
            writer.Function("std::to_bytes", debug: null, metadata: new FunctionMetadata("std::to_bytes", method), sParam);
        }

        [MethodName(nameof(EdgeQL.Base64Decode))]
        public void Base64DecodeTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter dataParam, TranslatedParameter alphabetParam, TranslatedParameter paddingParam)
        {
            writer.Function("std::enc::base64_decode", debug: null, metadata: new FunctionMetadata("std::enc::base64_decode", method), dataParam, new Terms.FunctionArg(alphabetParam, "alphabet"), new Terms.FunctionArg(paddingParam, "padding"));
        }

        [MethodName(nameof(EdgeQL.Concat))]
        public void Concat(QueryWriter writer, TranslatedParameter lParam, TranslatedParameter rParam)
        {
            writer.Append(lParam).Append(" ++ ").Append(rParam);
        }
    }
}
