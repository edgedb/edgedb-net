using System;
using System.Linq.Expressions;
using System.Reflection.Emit;

namespace EdgeDB.CIL.Interpreters
{
    internal abstract class BaseCILInterpreter
    {
        public OpCodeType[] SupportedCodes { get; }

        public BaseCILInterpreter(params OpCodeType[] codes)
        {
            SupportedCodes = codes;
        }

        public abstract Expression Interpret(Instruction instruction, CILInterpreterContext context);

        protected Expression InterpretTo(Label label, CILInterpreterContext context)
        {
            var blockContext = context.Enter(x =>
            {
                // reset stack to capture block (should not reference to instructions from previous frames)
                x.ExpressionStack = new();
            });

            return InterpretInstructions(context.Reader.ReadTo(label), blockContext);
        }

        protected Expression InterpretInstructions(IEnumerable<Instruction> instructions, CILInterpreterContext context)
        {
            List<Expression> tree = new();

            foreach (var instruction in instructions)
            {
                var expression = Interpret(instruction, context);
                if (expression is DefaultExpression d && d.Type == typeof(void))
                    continue;
                tree.Add(expression);
            }

            return tree.Count == 1 ? tree[0] : Expression.Block(tree);
        }
    }
}

