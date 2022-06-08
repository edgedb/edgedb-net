using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents the log message severity within a <see cref="LogMessage"/>
    /// </summary>
    public enum MessageSeverity : byte
    {
        Debug = 0x14,
        Info = 0x28,
        Notice = 0x3c,
        Warning = 0x50
    }
}
