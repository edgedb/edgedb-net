#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace EdgeDB.Translators
{
    internal partial class StdAnyrealMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.Abs))]
        public void AbsTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter xParam)
        {
            writer.Function("math::abs", debug: null, metadata: new FunctionMetadata("math::abs", method), xParam);
        }

    }
}
