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
    ///     Represents an exception that was caused by an error from EdgeDB.
    /// </summary>
    public sealed class EdgeDBErrorException : EdgeDBException
    {
        private const ushort ERROR_LINE_START = 0xFFF3;
        private const ushort ERROR_LINE_END = 0xFFF6;
        private const ushort ERROR_UTF16COLUMN_START = 0xFFF5;
        private const ushort ERROR_UTF16COLUMN_END = 0xFFF8;

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
        ///     Gets the query that caused this error.
        /// </summary>
        public string? Query { get; }

        /// <summary>
        ///     Gets the server error code.
        /// </summary>
        public ServerErrorCodes ErrorCode
            => ErrorResponse.ErrorCode;

        internal ErrorResponse ErrorResponse;

        /// <summary>
        ///     Constructs a new <see cref="EdgeDBErrorException"/> with the specified
        ///     <see cref="Binary.Packets.ErrorResponse"/> packet.
        /// </summary>
        /// <param name="error">
        ///     The <see cref="Binary.Packets.ErrorResponse"/> packet which
        ///     caused this exception to be thrown.
        /// </param>
        internal EdgeDBErrorException(ErrorResponse error)
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

        /// <summary>
        ///     Constructs a new <see cref="EdgeDBErrorException"/> with the specified
        ///     <see cref="Binary.Packets.ErrorResponse"/> packet and query string.
        /// </summary>
        /// <param name="error">
        ///     The <see cref="Binary.Packets.ErrorResponse"/> packet which
        ///     caused this exception to be thrown.
        /// </param>
        /// <param name="query">The query that caused this error to be thrown.</param>
        internal EdgeDBErrorException(ErrorResponse error, string? query)
            : this(error)
        {
            Query = query;
        }

        /// <summary>
        ///     Prettifies the error if it was a result of a bad query string; otherwise formats it.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var formatted = Prettify() ?? $"{ErrorResponse.ErrorCode}: {ErrorResponse.Message}";

            if (StackTrace is not null)
                formatted += $"{Environment.NewLine}{StackTrace}";

            return formatted;
        }

        private string? Prettify()
        {
            if (Query is null ||
                !ErrorResponse.TryGetAttribute(ERROR_LINE_START, out var lineStart) ||
                !ErrorResponse.TryGetAttribute(ERROR_LINE_END, out var lineEnd) ||
                !ErrorResponse.TryGetAttribute(ERROR_UTF16COLUMN_START, out var columnStart) ||
                !ErrorResponse.TryGetAttribute(ERROR_UTF16COLUMN_END, out var columnEnd))
            {
                return null;
            }

            var lines = Query.Split("\n");

            var lineNoWidth = lineEnd.ToString().Length;

            var errorMessage = $"{ErrorResponse.ErrorCode}: {ErrorResponse.Message}\n";

            errorMessage += "|".PadLeft(lineNoWidth + 3) + "\n";

            var lineStartInt = lineStart.ToInt();
            var lineEndInt = lineEnd.ToInt();
            var colStartInt = columnStart.ToInt();
            var colEndInt = columnEnd.ToInt();

            for (int i = lineStartInt; i < lineEndInt + 1; i++)
            {
                var line = lines[i - 1];
                var start = i == lineStartInt ? colStartInt : 0;
                var end = i == lineEndInt ? colEndInt : line.Length;
                errorMessage += $" {i.ToString().PadLeft(lineNoWidth)} | {line}\n";
                errorMessage += $"{"|".PadLeft(lineNoWidth + 3)} {"".PadLeft(end - start, '^').PadLeft(end)}\n";
            }

            if (Hint is not null)
                errorMessage += $"Hint: {Hint}";

            return errorMessage;
        }
    }
}
