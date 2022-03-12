using EdgeDB.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public struct ExecuteResult<TType> : IExecuteResult
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

        //internal static ExecuteResult<TType> Convert(ExecuteResult result)
        //{
        //    var converted = new ExecuteResult<TType>
        //    {
        //        IsSuccess = result.IsSuccess,
        //        Error = result.Error,
        //        Exception = result.Exception,
        //        ExecutedQuery = result.ExecutedQuery
        //    };

        //    if (result.Result is IDictionary<string, object?> rawObj)
        //    {
        //        converted.Result = ObjectBuilder.BuildResult<TType>(rawObj);
        //    }
        //    else if (typeof(TType).Name == "IReadOnlyCollection`1" && result.Result is object?[] arr)
        //    {
        //        var targetType = typeof(TType).GenericTypeArguments[0];

        //        var m = typeof(ImmutableArray).GetMethods();

        //        var createFunc = typeof(ImmutableArray).GetMethods().First(x => x.Name == "CreateRange" && x.GetParameters().Length == 1)!.MakeGenericMethod(targetType);

        //        var newArr = Array.CreateInstance(targetType, arr.Length);

        //        // convert expando objects
        //        for (int i = 0; i != arr.Length; i++)
        //        {
        //            var obj = arr[i];
        //            object? convt = obj;

        //            if (obj is IDictionary<string, object?> dict)
        //            {
        //                convt = ObjectBuilder.BuildResult(targetType, dict);
        //            }

        //            newArr.SetValue(convt, i);
        //        }

        //        converted.Result = (TType?)createFunc.Invoke(null, new object[] { newArr });
        //    }
        //    else if (result.Result != null)
        //        converted.Result = (TType?)result.Result;

        //    return converted;
        //}

        object? IExecuteResult.Result => Result;
    }
}
