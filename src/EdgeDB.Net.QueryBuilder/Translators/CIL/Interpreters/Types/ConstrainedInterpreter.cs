using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class ConstrainedInterpreter : BaseCILInterpreter
    {
        public ConstrainedInterpreter()
            : base(OpCodeType.Constrained_)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            // We can treat constrained as a noop because its
            // only related to formatting an instance reference ptr.
            // Since our interpreter doesn't do any pointer logic
            // and instead just uses strong-typed references, we
            // can ignore this instruction.

            return Expression.Empty();
        }
    }
}

