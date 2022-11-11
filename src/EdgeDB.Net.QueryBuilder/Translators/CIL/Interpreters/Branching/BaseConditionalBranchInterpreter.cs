using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal abstract class BaseConditionalBranchInterpreter
        : BaseCILInterpreter
    {
        public BaseConditionalBranchInterpreter(params OpCodeType[] opcodes)
            : base(opcodes)
        {
        }

        protected abstract Expression GetCondition(CILInterpreterContext context);

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            var condition = GetCondition(context);

            if (!instruction.TryGetOperandAs<Label>(out var label))
                throw new NotSupportedException("branch instructions must contain a label");

            // the true clause is everything from our current
            // to the label, read it
            var trueClauseInstructions = context.Reader.ReadTo(label).ToArray();

            Instruction? trueClauseBranch = null;

            if (trueClauseInstructions[^1].OpCodeType is OpCodeType.Br or OpCodeType.Br_s)
            {
                // remove the ending branch instruction
                // and store it for later
                trueClauseBranch = trueClauseInstructions[^1];
                trueClauseInstructions = trueClauseInstructions[..^1];
            }

            var trueClause = base.InterpretInstructions(
                trueClauseInstructions,
                context);

            // if the true clause ends with a br or br.s that signifies
            // that there is an else block
            Expression? elseClause = null;

            if (trueClauseBranch.HasValue)
            {
                if(!trueClauseBranch.Value.TryGetOperandAs<Label>(out label))
                    throw new NotSupportedException("branch instructions must contain a label");

                // pull the else block
                elseClause = InterpretTo(label, context);
            }

            return elseClause is null
                ? Expression.IfThen(condition, trueClause)
                : Expression.IfThenElse(condition, trueClause, elseClause);
        }
    }
}

