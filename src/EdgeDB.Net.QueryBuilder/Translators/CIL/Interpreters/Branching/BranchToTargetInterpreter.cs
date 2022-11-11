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
            if (!instruction.TryGetOperandAs<Label>(out var label))
                throw new NotSupportedException("branch target must be a label");

            context.Reader.Seek(label);

            return Expression.Empty();
        }
    }
}

