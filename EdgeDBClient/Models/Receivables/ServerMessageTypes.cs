using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    public enum ServerMessageTypes : sbyte
	{
		Authentication = 0x52,
		CommandComplete = 0x43,
		CommandDataDescription = 0x54,
		Data = 0x44,
		DumpBlock = 0x3d,
		DumpHeader = 0x40,
		ErrorResponse = 0x45,
		LogMessage = 0x4c,
		ParameterStatus = 0x53,
		PrepareComplete = 0x31,
		ReadyForCommand = 0x5a,
		RestoreReady = 0x2b,
		ServerHandshake = 0x76,
		ServerKeyData = 0x4b
	}
}
