using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public class EdgeDBConfig
    {
        /// <summary>
        ///     Gets or sets whether or not to allow an unsecure connection when connecting over TCP
        /// </summary>
        public bool AllowUnsecureConnection { get; set; } = false;
    }
}
