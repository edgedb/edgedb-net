using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class TailInterpreter
        : BaseCILInterpreter
    {
        public TailInterpreter()
            : base(OpCodeType.Tail_)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            // we don't care about actually emptying the stack,
            // as our stack contains the code. We can treat this
            // as a noop.

            return Expression.Empty();
        }
    }
}

