using EdgeDB.Binary;

namespace EdgeDB
{
    /// <summary>
    ///     Represents an exception that occurs when the client receives an unexpected message.
    /// </summary>
    public class UnexpectedMessageException : EdgeDBException
    {
        /// <summary>
        ///     Constructs a new <see cref="UnexpectedMessageException"/> with the message type the client <i>wasn't</i> expecting.
        /// </summary>
        /// <param name="unexpected">The unexcepted message type.</param>
        public UnexpectedMessageException(ServerMessageType unexpected)
            : base($"Got unexpected message type {unexpected}")
        {

        }

        /// <summary>
        ///     Constructs a new <see cref="UnexpectedMessageException"/> with the expected and actual message types.
        /// </summary>
        /// <param name="expected">The expected message type.</param>
        /// <param name="actual">The actual message type.</param>
        public UnexpectedMessageException(ServerMessageType expected, ServerMessageType actual)
            : base($"Expected message type {expected} but got {actual}")
        {

        }
    }
}
