#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class SysTransactionisolationMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.GetTransactionIsolation))]
        public void GetTransactionIsolationTranslator(QueryWriter writer)
        {
            writer.Function("sys::get_transaction_isolation");
        }

    }
}
