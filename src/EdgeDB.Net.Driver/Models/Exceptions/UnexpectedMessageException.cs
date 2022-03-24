using EdgeDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
