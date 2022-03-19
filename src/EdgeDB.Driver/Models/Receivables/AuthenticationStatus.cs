using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#authenticationok">AuthenticationOK</see>,
    ///     <see href="https://www.edgedb.com/docs/reference/protocol/messages#authenticationsasl">AuthenticationSASL</see>,
    ///     <see href="https://www.edgedb.com/docs/reference/protocol/messages#authenticationsaslcontinue">AuthenticationSASLContinue</see>,
    ///     and <see href="https://www.edgedb.com/docs/reference/protocol/messages#authenticationsaslfinal">AuthenticationSASLFinal</see> packets.
    /// </summary>
    public struct AuthenticationStatus : IReceiveable
    {
        /// <inheritdoc/>
        public ServerMessageType Type => ServerMessageType.Authentication;

        /// <summary>
        ///     Gets the authentication state. 
        /// </summary>
        public AuthStatus AuthStatus { get; private set; }

        /// <summary>
        ///     Gets a collection of supported authentication methods.
        /// </summary>
        public string[] AuthenticationMethods { get; private set; }

        /// <summary>
        ///     Gets the SASL data.
        /// </summary>
        public byte[] SASLData { get; private set; }

        void IReceiveable.Read(PacketReader reader, uint length, EdgeDBTcpClient client)
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
