using System;
using System.Linq.Expressions;
using System.Reflection;

namespace EdgeDB.CIL.Interpreters
{
    internal class CallInterpreter : BaseCILInterpreter
    {
        public CallInterpreter()
            : base(OpCodeType.Call, OpCodeType.Callvirt)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            var method = instruction.OprandAsMethod();

            if (method is not MethodInfo methodInfo)
                throw new NotSupportedException($"Expected a method info but got {method.GetType()}");

            Expression[] methodArgs = new Expression[method.GetParameters().Length];

            for (int i = methodArgs.Length - 1; i >= 0; i--)
                methodArgs[i] = context.Stack.PopExp();

            var instance = instruction.OpCodeType is OpCodeType.Callvirt
                ? context.Stack.PopExp()
                : null;

            return Expression.Call(instance, methodInfo, methodArgs);
        }
    }
}

