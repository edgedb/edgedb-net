using System;
using System.Linq.Expressions;

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
            // due to the way our method call evaluation works,
            // we can treat return as a noop as we don't have
            // stack frames or 'isolated stacks' for different
            // method translations.
            return Expression.Empty();
        }
    }
}

