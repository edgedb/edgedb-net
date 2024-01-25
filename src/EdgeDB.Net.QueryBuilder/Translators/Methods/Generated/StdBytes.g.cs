#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdBytesMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.ToBytes))]
        public void ToBytesTranslator(QueryStringWriter writer, TranslatedParameter sParam)
        {
            writer.Function("std::to_bytes", sParam);
        }

        [MethodName(nameof(EdgeQL.Base64Decode))]
        public void Base64DecodeTranslator(QueryStringWriter writer, TranslatedParameter dataParam, TranslatedParameter alphabetParam, TranslatedParameter paddingParam)
        {
            writer.Function("std::enc::base64_decode", dataParam, new QueryStringWriter.FunctionArg(alphabetParam, "alphabet"), new QueryStringWriter.FunctionArg(paddingParam, "padding"));
        }

        [MethodName(nameof(EdgeQL.Concat))]
        public void Concat(QueryStringWriter writer, TranslatedParameter lParam, TranslatedParameter rParam)
        {
            writer.Append(lParam).Wrapped("++", "  ").Append(rParam);
        }
    }
}