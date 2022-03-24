using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DataTypes
{
    public abstract class BaseObject
    {
        [EdgeDBProperty("id", IsLink = false, IsRequired = true)]
        public readonly Guid Id = Guid.NewGuid();
    }
}
