using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol.DumpRestore
{
    internal interface IDumpRestoreProvider
    {
        ProtocolVersion DumpRestoreVersion { get; }

        Task<string> RestoreDatabaseAsync(EdgeDBBinaryClient client, Stream stream, CancellationToken token);
        Task DumpDatabaseAsync(EdgeDBBinaryClient client, Stream stream, CancellationToken token = default);
    }
}
