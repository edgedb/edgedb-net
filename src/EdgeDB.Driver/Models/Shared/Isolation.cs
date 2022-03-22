using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    /// <summary>
    ///     An enum representing the transaction mode within a <see cref="Transaction"/>.
    /// </summary>
    public enum Isolation
    {
        /// <summary>
        ///     All statements of the current transaction can only see data changes 
        ///     committed before the first query or data-modification statement was 
        ///     executed in this transaction. If a pattern of reads and writes among 
        ///     concurrent serializable transactions would create a situation which 
        ///     could not have occurred for any serial (one-at-a-time) execution of 
        ///     those transactions, one of them will be rolled back with a serialization_failure error.
        /// </summary>
        Serializable,

        /// <summary>
        ///     All statements of the current transaction can only see data committed
        ///     before the first query or data-modification statement was executed in 
        ///     this transaction.
        /// </summary>
        RepeatableRead,
    }
}
