using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class PopInterpreter : BaseCILInterpreter
    {
        public PopInterpreter()
            : base(OpCodeType.Pop)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            // pop a value from the stack to discard it.
            _ = context.Stack.PopExp();

            return Expression.Empty();
        }
    }
}

