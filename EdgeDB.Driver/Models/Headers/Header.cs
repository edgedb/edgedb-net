using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    public struct Header
    {
        public ushort Code { get; set; }
        public byte[] Value { get; set; }

        /// <summary>
        ///     Converts this headers value to a UTF8 encoded string
        /// </summary>
        public override string ToString()
        {
            return Encoding.UTF8.GetString(Value);
        }
    }
}
