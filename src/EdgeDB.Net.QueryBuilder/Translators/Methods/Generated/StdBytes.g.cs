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

        [MethodName(nameof(EdgeQL.Concat))]
        public void Concat(QueryStringWriter writer, TranslatedParameter lParam, TranslatedParameter rParam)
        {
            writer.Append(lParam).Wrapped("++", "  ").Append(rParam);
        }
    }
}
