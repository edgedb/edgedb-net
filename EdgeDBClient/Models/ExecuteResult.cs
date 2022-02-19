using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    public struct ExecuteResult<TType>
    {
        public bool IsSuccess { get; set; }

        public TType? Result { get; set; }

        public ErrorResponse? Error { get; set; }

        public Exception? Exception { get; set; }

        internal ExecuteResult(bool success, TType? result, ErrorResponse? error, Exception? exc)
        {
            IsSuccess = success;
            Result = result;
            Error = error;
            Exception = exc;
        }
    }

    public struct ExecuteResult
    {
        public bool IsSuccess { get; set; }

        public object? Result { get; set; }

        public ErrorResponse? Error { get; set; }

        public Exception? Exception { get; set; }

        internal ExecuteResult(bool success, object? result, ErrorResponse? error, Exception? exc)
        {
            IsSuccess = success;
            Result = result;
            Error = error;
            Exception = exc;
        }
    }
}
