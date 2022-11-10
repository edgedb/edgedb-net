using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class BranchIfTrueInterpreter : BaseCILInterpreter
    {
        public BranchIfTrueInterpreter()
            : base(OpCodeType.Brtrue, OpCodeType.Brtrue_s)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            var conditional = context.ExpressionStack.Pop();

            if(!instruction.TryGetOperandAs<Label>(out var label))
                throw new ArgumentException("Oprand must be a label");

            // parse everything at the jump point and make if else for body block

            var clauseInstructions = context.Reader.ReadTo(label).ToArray();

            if (!(clauseInstructions[^1].OpCodeType is OpCodeType.Br or OpCodeType.Br_s))
                throw new NotSupportedException($"Unable to determine else clase start for {instruction.OpCodeType}: please file a bug report with the query that threw this error");

            // get the else clause instructions
            var elseClauseInstructions = context.Reader.ReadTo(label);

            return Expression.IfThenElse(
                conditional,
                InterpretInstructions(clauseInstructions, context),
                InterpretInstructions(elseClauseInstructions, context)
            );
        }
    }
}

