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

        public string? ExecutedQuery { get; set; }

        internal ExecuteResult(bool success, TType? result, ErrorResponse? error, Exception? exc, string? executedQuery)
        {
            IsSuccess = success;
            Result = result;
            Error = error;
            Exception = exc;
            ExecutedQuery = executedQuery;
        }

        internal static ExecuteResult<TType> Convert(ExecuteResult result)
        {
            var converted = new ExecuteResult<TType>
            {
                IsSuccess = result.IsSuccess,
                Error = result.Error,
                Exception = result.Exception,
                ExecutedQuery = result.ExecutedQuery
            };

            if (result.Result is IDictionary<string, object?> rawObj)
            {
                converted.Result = ResultBuilder.BuildResult<TType>(rawObj);
            }
            else if (result.Result != null)
                converted.Result = (TType?)result.Result;

            return converted;
        }
    }

    public struct ExecuteResult
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
}
