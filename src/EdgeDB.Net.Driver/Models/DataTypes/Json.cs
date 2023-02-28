using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DataTypes
{
    /// <summary>
    ///     Represents a standard json value.
    /// </summary>
    public readonly struct Json 
    {
        /// <summary>
        ///     Gets or sets the raw json value.
        /// </summary>
        public readonly string? Value;

        /// <summary>
        ///     Creates a new json type with a provided value.
        /// </summary>
        /// <param name="value">The raw json value of this json object.</param>
        public Json(string? value)
        {
            Value = value;
        }

        public static implicit operator string?(Json j) => j.Value;
        public static implicit operator Json(string? value) => new(value);
    }
}
