using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    public struct AuthenticationStatus : IReceiveable
    {
        public ServerMessageTypes Type => ServerMessageTypes.Authentication;

        public AuthStatus AuthStatus { get; set; }

        public string[] AuthenticationMethods { get; set; }

        public byte[] SASLData { get; set; }

        public void Read(PacketReader reader, uint length, EdgeDBTcpClient client)
        {
            AuthStatus = (AuthStatus)reader.ReadUInt32();

            switch (AuthStatus)
            {
                case AuthStatus.AuthenticationRequiredSASLMessage:
                    {
                        var count = reader.ReadUInt32();
                        AuthenticationMethods = new string[count];

                        for(int i = 0; i != count; i++)
                        {
                            AuthenticationMethods[i] = reader.ReadString();
                        }
                    }
                    break;
                case AuthStatus.AuthenticationSASLContinue or AuthStatus.AuthenticationSASLFinal:
                    {
                        SASLData = reader.ReadByteArray();
                    }
                    break;
            }
        }
    }
}
