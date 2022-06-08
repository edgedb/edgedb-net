using EdgeDB.Binary;
using EdgeDB.Models;

namespace EdgeDB
{
    /// <summary>
    ///     Represents an exception that occurs when the client receives an unexpected exception.
    /// </summary>
    public class UnexpectedMessageException : EdgeDBException
    {
        public UnexpectedMessageException(ServerMessageType unexpected)
            : base($"Got unexpected message type {unexpected}")
        {

        }

        public UnexpectedMessageException(ServerMessageType expected, ServerMessageType actual)
            : base($"Expected message type {expected} but got {actual}")
        {

        }
    }
}
