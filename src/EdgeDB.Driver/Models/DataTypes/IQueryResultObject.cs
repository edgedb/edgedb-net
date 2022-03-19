using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DataTypes
{
    /// <summary>
    ///     Represents an abstract type result from a query.
    /// </summary>
    public interface IQueryResultObject
    {
        /// <summary>
        ///     Gets the object id of this result object.
        /// </summary>
        /// <returns>The unique id of this object.</returns>
        public Guid GetObjectId();
    }
}
