using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     An enum representing the error severity of a <see cref="ErrorResponse"/>.
    /// </summary>
    public enum ErrorSeverity : byte
    {
        /// <summary>
        ///     An error.
        /// </summary>
        Error = 0x78,

        /// <summary>
        ///     A fatal error.
        /// </summary>
        Fatal = 0xc8,

        /// <summary>
        ///     <!--fun little easter egg :D-->
        ///     A panic<see href="https://www.youtube.com/watch?v=IPXIgEAGe4U">.</see>
        /// </summary>
        Panic = 0xff
    }
}
