#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class SysTransactionisolationMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.GetTransactionIsolation))]
        public string GetTransactionIsolationTranslator()
        {
            return $"sys::get_transaction_isolation()";
        }

    }
}
