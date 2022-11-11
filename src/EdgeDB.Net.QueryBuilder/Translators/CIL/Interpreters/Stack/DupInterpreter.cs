using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class DupInterpreter : BaseCILInterpreter
    {
        public DupInterpreter()
            : base(OpCodeType.Dup)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            context.Stack.Push(context.Stack.Peek());
            return Expression.Empty();
        }
    }
}

