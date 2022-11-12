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

            EnsureValidTypes(ref condition);

            if (!instruction.TryGetOperandAs<Label>(out var label))
                throw new NotSupportedException("branch instructions must contain a label");

            // the true clause is everything from our current
            // to the label, read it
            var bodyClauseInstructions = context.Reader.ReadTo(label).ToArray();

            Instruction? jumpClauseInstruction = null;

            if (bodyClauseInstructions[^1].OpCodeType is OpCodeType.Br or OpCodeType.Br_s)
            {
                // remove the ending branch instruction
                // and store it for later
                jumpClauseInstruction = bodyClauseInstructions[^1];
                bodyClauseInstructions = bodyClauseInstructions[..^1];
            }

            var bodyClause = base.InterpretInstructions(
                bodyClauseInstructions,
                context);

            // if the true clause ends with a br or br.s that signifies
            // that there is an else block
            Expression? jumpClause = null;

            if (jumpClauseInstruction.HasValue)
            {
                if(!jumpClauseInstruction.Value.TryGetOperandAs<Label>(out label))
                    throw new NotSupportedException("branch instructions must contain a label");

                // pull the else block
                jumpClause = InterpretTo(label, context);
            }

            return jumpClause is null
                ? Expression.IfThen(condition, bodyClause)
                : instruction.OpCodeType is OpCodeType.Brtrue or OpCodeType.Brtrue_s
                    ? Expression.Condition(condition, jumpClause, bodyClause)   // ternary
                    : Expression.IfThenElse(condition, bodyClause, jumpClause); // standard ifelse (flipped due to brfalse)
        }
    }
}

