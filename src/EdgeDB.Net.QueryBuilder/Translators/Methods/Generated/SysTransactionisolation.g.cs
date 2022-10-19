using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class SysTransactionisolation : MethodTranslator<TransactionIsolation>
    {
        [MethodName(EdgeQL.GetTransactionIsolation)]
        public string GetTransactionIsolation()
        {
            return $"sys::get_transaction_isolation()";
        }

    }
}
