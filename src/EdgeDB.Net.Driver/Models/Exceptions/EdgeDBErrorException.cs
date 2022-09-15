using EdgeDB.Binary.Packets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents an exception that wraps <see cref="Binary.Packets.ErrorResponse"/>.
    /// </summary>
    public sealed class EdgeDBErrorException : EdgeDBException
    {
        /// <summary>
        ///     Gets the details related to the error.
        /// </summary>
        public string? Details { get; }

        /// <summary>
        ///     Gets the server traceback log for the error.
        /// </summary>
        public string? ServerTraceBack { get; }

        /// <summary>
        ///     Gets the hint for the error.
        /// </summary>
        public string? Hint { get; }

        /// <summary>
        ///     Gets the raw <see cref="Binary.Packets.ErrorResponse"/> packet.
        /// </summary>
        public ErrorResponse ErrorResponse { get; }

        public EdgeDBErrorException(ErrorResponse error)
            : base(error.Message, typeof(ServerErrorCodes).GetField(error.ErrorCode.ToString())?.IsDefined(typeof(ShouldRetryAttribute), false) ?? false)
        {
            if(error.Attributes.Any(x => x.Code == 0x0002))
                Details = Encoding.UTF8.GetString(error.Attributes.FirstOrDefault(x => x.Code == 0x0002).Value);

            if (error.Attributes.Any(x => x.Code == 0x0101))
                ServerTraceBack = Encoding.UTF8.GetString(error.Attributes.FirstOrDefault(x => x.Code == 0x0101).Value);

            if (error.Attributes.Any(x => x.Code == 0x0001))
                Hint = Encoding.UTF8.GetString(error.Attributes.FirstOrDefault(x => x.Code == 0x0001).Value);

            ErrorResponse = error;
        }
    }
}
