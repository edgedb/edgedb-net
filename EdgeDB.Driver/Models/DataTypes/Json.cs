using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models.DataTypes
{
    public struct Json 
    {
        public string? Value { get; set; }

        public Json(string? value)
        {
            Value = value;
        }
    }
}
