using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class UnalignedInterpreter : BaseCILInterpreter
    {
        public UnalignedInterpreter()
            : base(OpCodeType.Unaligned_)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            // we don't do any pointer logic for expressions.
            // This can be treated as a noop

            return Expression.Empty();
        }
    }
}

