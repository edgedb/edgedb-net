using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    public readonly struct AuthenticationStatus : IReceiveable
    {
        /// <inheritdoc/>
        public ServerMessageType Type 
            => ServerMessageType.Authentication;

        /// <summary>
        ///     Gets the authentication state. 
        /// </summary>
        public AuthStatus AuthStatus { get; }

        /// <summary>
        ///     Gets a collection of supported authentication methods.
        /// </summary>
        public string[]? AuthenticationMethods { get; }

        /// <summary>
        ///     Gets the SASL data.
        /// </summary>
        public IReadOnlyCollection<byte>? SASLData
            => SASLDataBuffer?.ToImmutableArray();

        internal byte[] SASLDataBuffer { get; }

        internal AuthenticationStatus(ref PacketReader reader)
        {
            AuthStatus = (AuthStatus)reader.ReadUInt32();

            switch (AuthStatus)
            {
                case AuthStatus.AuthenticationRequiredSASLMessage:
                    {
                        var count = reader.ReadUInt32();
                        AuthenticationMethods = new string[count];

                        for (int i = 0; i != count; i++)
                        {
                            AuthenticationMethods[i] = reader.ReadString();
                        }
                        SASLDataBuffer = Array.Empty<byte>();
                    }
                    break;
                case AuthStatus.AuthenticationSASLContinue or AuthStatus.AuthenticationSASLFinal:
                    {
                        SASLDataBuffer = reader.ReadByteArray();
                        AuthenticationMethods = Array.Empty<string>();
                    }
                    break;
                default:
                    SASLDataBuffer = Array.Empty<byte>();
                    AuthenticationMethods = Array.Empty<string>();
                    break;
            }
        }
    }
}
