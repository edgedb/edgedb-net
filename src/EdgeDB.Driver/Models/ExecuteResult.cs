using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    public struct ExecuteResult : IExecuteResult
    {
        public bool IsSuccess { get; set; }

        public object? Result { get; set; }

        public ErrorResponse? Error { get; set; }

        public Exception? Exception { get; set; }

        public string? ExecutedQuery { get; set; }

        internal ExecuteResult(bool success, object? result, ErrorResponse? error, Exception? exc, string? executedQuery)
        {
            IsSuccess = success;
            Result = result;
            Error = error;
            Exception = exc;
            ExecutedQuery = executedQuery;
        }
    }

    public interface IExecuteResult
    {
        bool IsSuccess { get; }
        object? Result { get; }
        ErrorResponse? Error { get; }
        Exception? Exception { get; }
        string? ExecutedQuery { get; }
    }
}
