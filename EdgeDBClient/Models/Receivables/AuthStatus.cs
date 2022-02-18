using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    public enum AuthStatus : uint
    {
        AuthenticationOK = 0x0,
        AuthenticationRequiredSASLMessage = 0xa,
        AuthenticationSASLContinue = 0xb,
        AuthenticationSASLFinal = 0xc,
    }
}
