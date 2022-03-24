using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    /// <summary>
    ///     Represents the authentication state.
    /// </summary>
    public enum AuthStatus : uint
    {
        /// <summary>
        ///     The authentication was successful and is validated.
        /// </summary>
        AuthenticationOK = 0x0,

        /// <summary>
        ///     The server requires an SASL message.
        /// </summary>
        AuthenticationRequiredSASLMessage = 0xa,

        /// <summary>
        ///     The client should continue sending SASL messages.
        /// </summary>
        AuthenticationSASLContinue = 0xb,

        /// <summary>
        ///     The received message was the final authentication message.
        /// </summary>
        AuthenticationSASLFinal = 0xc,
    }
}
