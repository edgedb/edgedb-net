using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class LoadVirtFunctionInterpreter : BaseCILInterpreter
    {
        public LoadVirtFunctionInterpreter()
            : base(OpCodeType.Ldvirtftn)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            // TODO: is this correct?

            var method = instruction.OprandAsMethod();
            context.Stack.Push(method);
            return Expression.Empty();
        }
    }
}

