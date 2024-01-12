#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class MultirangeMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.Multirange))]
        public void MultirangeTranslator(QueryStringWriter writer, TranslatedParameter rangesParam)
        {
            writer.Function("std::multirange", rangesParam);
        }

    }
}
