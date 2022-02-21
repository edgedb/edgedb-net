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

        internal static ExecuteResult<TType>? Convert(ExecuteResult? result)
        {
            if (!result.HasValue)
                return null;

            var converted = new ExecuteResult<TType>
            {
                IsSuccess = result.Value.IsSuccess,
                Error = result.Value.Error,
                Exception = result.Value.Exception,
                ExecutedQuery = result.Value.ExecutedQuery
            };

            if (result.Value.Result is IDictionary<string, object?> rawObj)
            {
                converted.Result = ResultBuilder.BuildResult<TType>(rawObj);
            }
            else if (result.Value.Result != null)
                converted.Result = (TType?)result.Value.Result;

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
