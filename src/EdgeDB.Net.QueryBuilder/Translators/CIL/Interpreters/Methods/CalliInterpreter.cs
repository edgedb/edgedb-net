using System;
using System.Linq.Expressions;
using System.Reflection;

namespace EdgeDB.CIL.Interpreters
{
    internal class CalliInterpreter : BaseCILInterpreter
    {
        public CalliInterpreter()
            : base(OpCodeType.Calli)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            // pop the method and its args
            if (context.MemberStack.Pop() is not MethodBase method)
                throw new InvalidOperationException("Expected method for calli instruction");

            if (method is not MethodInfo methodInfo)
                throw new NotSupportedException($"Expected a method info but got {method.GetType()}");

            Expression[] methodArgs = new Expression[method.GetParameters().Length];

            for (int i = methodArgs.Length - 1; i >= 0; i--)
                methodArgs[i] = context.ExpressionStack.Pop();

            // TODO: does this method call have an instance?

            return Expression.Call(null, methodInfo, methodArgs);
        }
    }
}

