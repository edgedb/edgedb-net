using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class NoopInterpreter : BaseCILInterpreter
    {
        public NoopInterpreter()
            : base(OpCodeType.Nop)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            return Expression.Empty();
        }
    }
}

