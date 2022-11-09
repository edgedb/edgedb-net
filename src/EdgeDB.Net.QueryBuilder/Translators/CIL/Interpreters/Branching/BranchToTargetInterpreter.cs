using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class BranchToTargetInterpreter : BaseCILInterpreter
    {
        public BranchToTargetInterpreter()
            : base(OpCodeType.Br, OpCodeType.Br_s)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            // TODO: revisit this to possibly extrapolate the IL at
            // the target branch to the current expression, in
            // otherwords flatten it.
            throw new NotSupportedException("Cannot use 'goto' in translated function");
        }
    }
}

