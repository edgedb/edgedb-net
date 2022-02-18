using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models.DataTypes
{
    public struct Memory
    {
        public long TotalBytes { get; }
        public long TotalMegabytes
            => TotalBytes / 1024;

        internal Memory(long bytes)
        {
            TotalBytes = bytes;
        }
    }
}
