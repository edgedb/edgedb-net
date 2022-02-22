using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    public enum TransactionState : byte
    {
        NotInTransaction = 0x49,
        InTransaction = 0x54,
        InFailedTransaction = 0x45,
    }
}
