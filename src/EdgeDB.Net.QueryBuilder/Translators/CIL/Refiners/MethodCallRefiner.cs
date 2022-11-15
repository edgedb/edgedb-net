using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

namespace EdgeDB.CIL
{
    internal class MethodCallRefiner : BaseRefiner<MethodCallExpression>
    {
        private static readonly MethodInfo _typeGetTypeFromHandle;
        private static readonly MethodInfo _runtimeInitializeArray;

        private static readonly ConcurrentDictionary<MethodInfo, Func<MethodCallExpression, RefiningContext, Expression>> _methodCallRefiners;

        static MethodCallRefiner()
        {
            _typeGetTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Static | BindingFlags.Public)!;
            _runtimeInitializeArray = typeof(RuntimeHelpers).GetMethod("InitializeArray", BindingFlags.Static | BindingFlags.Public)!;


            _methodCallRefiners = new(new Dictionary<MethodInfo, Func<MethodCallExpression, RefiningContext, Expression>>()
            {
                {
                    _typeGetTypeFromHandle,
                    OptimizeGetTypeFromHandle
                },
                {
                    _runtimeInitializeArray,
                    OptimizeArrayInitialization
                }
            });
        }

        protected override Expression Refine(MethodCallExpression expression, RefiningContext context)
        {
            return _methodCallRefiners.TryGetValue(expression.Method, out var refiner)
                ? refiner(expression, context)
                : expression;
            
        }

        private static Expression OptimizeGetTypeFromHandle(MethodCallExpression expression, RefiningContext context)
        {
            // typeof is translated to a call to Type.GetTypeFromHandle.

            // get the type handle
            var handle = (EntityHandle)((ConstantExpression)expression.Arguments[0]).Value!;

            // get the type from the handle
            var type = context.Module!.ResolveType(handle.GetMetadataToken());

            return Expression.Constant(type, typeof(Type));
        }

        private static Expression OptimizeArrayInitialization(MethodCallExpression expression, RefiningContext context)
        {
            // The array was most likely dup'd on the stack, check if the prev expression was
            // a new array
            if(context.Stack.TryPeek(out var prev) && prev is NewArrayExpression)
            {
                // pop it from the stack
                _ = context.Stack.PopExp();
            }


            // 2nd param is the field containing the values to initialize with
            var fieldHandle = expression.Arguments[1].Expect<ConstantExpression>();
            var arrayExp = expression.Arguments[0].Expect<NewArrayExpression>();
            var arrayType = arrayExp.Type.GetElementType()!;

            // TODO: multi-dimentional arrays
            var sizeEpression = arrayExp.Expressions.First().Expect<ConstantExpression>();

            var arraySize = (int)sizeEpression.Value!;

            // TODO: long lengths?
            var arr = Array.CreateInstance(arrayType, arraySize);

            RuntimeHelpers.InitializeArray(arr, (RuntimeFieldHandle)fieldHandle.Value!);

            Expression[] initializations = new Expression[arraySize];

            for(int i = 0; i != arraySize; i++)
            {
                initializations[i] = Expression.Constant(arr.GetValue(i), arrayType);
            }


            return Expression.NewArrayInit(arrayType, initializations);
        }
    }
}

