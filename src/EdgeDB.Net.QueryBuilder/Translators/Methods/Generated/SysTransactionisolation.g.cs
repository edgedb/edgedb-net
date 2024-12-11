#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace EdgeDB.Translators
{
    internal partial class SysTransactionisolationMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.GetTransactionIsolation))]
        public void GetTransactionIsolationTranslator(QueryWriter writer, MethodInfo method)
        {
            writer.Function("sys::get_transaction_isolation", debug: null, metadata: new FunctionMetadata("sys::get_transaction_isolation", method));
        }

    }
}
