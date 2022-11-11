using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class RethrowInterpreter : BaseCILInterpreter
    {
        public RethrowInterpreter()
            : base(OpCodeType.Rethrow)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
            => Expression.Rethrow();
    }
}

