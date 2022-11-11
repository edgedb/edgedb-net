using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class LoadFunctionInterpreter : BaseCILInterpreter
    {
        public LoadFunctionInterpreter()
            : base(OpCodeType.Ldftn)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            var method = instruction.OprandAsMethod();
            context.Stack.Push(method);
            return Expression.Empty();
        }
    }
}

