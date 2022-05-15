using EdgeDB.Models;

namespace EdgeDB
{
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
