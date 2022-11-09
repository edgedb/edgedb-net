using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class BranchIfFalseInterpreter : BaseCILInterpreter
    {
        public BranchIfFalseInterpreter()
            : base(OpCodeType.Brfalse, OpCodeType.Brfalse_s)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            // brfalse is commonly used for 'if true' statements.
            // we can get a "else" body which is our current position..branch target
            // and then invert the else and if clause to form the proper if statement.

            var conditional = context.Stack.Pop();

            if (!instruction.TryGetOperandAs<Label>(out var label))
                throw new ArgumentException("Oprand must be a label");

            var clauseBlock = InterpretTo(label, context);

            return Expression.IfThen(conditional, clauseBlock);
        }
    }
}

