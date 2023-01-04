using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents a generic query.
    /// </summary>
    /// <typeparam name="TType">The inner 'working' type of the query.</typeparam>
    public interface IQuery<TType> : IQueryBuilder { }
}
