using System;
using System.Linq.Expressions;
using System.Reflection;

namespace EdgeDB.CIL.Interpreters
{
    internal class ReturnInterpreter : BaseCILInterpreter
    {
        public ReturnInterpreter()
            : base(OpCodeType.Ret)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            // TODO: refine logic for returns
            if (context.RootMethod is not MethodInfo methodInfo)
                throw new ArgumentException("Cannot derive method info from root method");

            if (methodInfo.ReturnType == typeof(void))
                return Expression.Empty();

            if (!context.Stack.TryPeek(out var raw) || raw is not Expression resultExpression)
                throw new InvalidOperationException("a return type is required for the current method, but none is on the stack");

            if (!resultExpression.Type.IsAssignableTo(methodInfo.ReturnType))
                throw new InvalidCastException($"Cannot implicitly convert {resultExpression.Type} to {methodInfo.ReturnType}");


            if(context.Stack.ExpressionCount == 1)
            {
                // return nothing, this expression isn't a block and has
                // an implicit return
                return Expression.Empty();
            }

            var returnValue = context.Stack.PopExp();

            return Expression.Return(Expression.Label(), returnValue);
        }
    }
}

