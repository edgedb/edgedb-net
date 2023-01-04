using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     An enum representing the placement of null values within queries.
    /// </summary>
    public enum OrderByNullPlacement
    {
        /// <summary>
        ///     Places <see langword="null"/> values at the front of the ordered set.
        /// </summary>
        First,

        /// <summary>
        ///     Places <see langword="null"/> values at the end of the ordered set.
        /// </summary>
        Last
    }
}
