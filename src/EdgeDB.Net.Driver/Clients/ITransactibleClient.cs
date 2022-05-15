using EdgeDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public interface ITransactibleClient : IEdgeDBQueryable, IAsyncDisposable
    {
        TransactionState TransactionState { get; }

        Task StartTransactionAsync(Isolation isolation, bool readOnly, bool deferrable);
        Task CommitAsync();
        Task RollbackAsync();
    }
}
