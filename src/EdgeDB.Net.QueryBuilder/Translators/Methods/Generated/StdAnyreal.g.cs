#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdAnyrealMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.Abs))]
        public void AbsTranslator(QueryWriter writer, TranslatedParameter xParam)
        {
            writer.Function("math::abs", xParam);
        }

    }
}
