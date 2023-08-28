using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary
{
    internal enum ClientMessageTypes : sbyte
    {
		AuthenticationSASLInitialResponse = 0x70,
		AuthenticationSASLResponse = 0x72,
		ClientHandshake = 0x56,
		DescribeStatement = 0x44,
		Dump = 0x3e,
		Execute = 0x4f,
		ExecuteScript = 0x51,
		Flush = 0x48,
		Parse = 0x50,
		Restore = 0x3c,
		RestoreBlock = 0x3d,
		RestoreEOF = 0x2e,
		Sync = 0x53,
		Terminate = 0x58,
	}
}
