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

        //public TType? ResutAs<TType>()
        //{
        //    if (Result is IDictionary<string, object?> rawObj)
        //    {
        //        return ObjectBuilder.BuildResult<TType>(rawObj);
        //    }
        //    else if (typeof(TType).Name == "IReadOnlyCollection`1" && Result is object?[] arr)
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

        //        return (TType?)createFunc.Invoke(null, new object[] { newArr });
        //    }
        //    else if (Result != null)
        //        return (TType?)Result;

        //    return default(TType);
        //}
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
