using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
            => InterpretInstructions(context.Reader.ReadTo(label), context);

        protected Expression InterpretInstructions(IEnumerable<Instruction> instructions, CILInterpreterContext context)
        {
            var stack = context.Stack.TransitionStack();
            var blockContext = context.Enter(x =>
            {
                x.Stack = stack;
            });

            foreach (var instruction in instructions)
            {
                var expression = CILInterpreter.Interpret(instruction, blockContext);
                if (expression is DefaultExpression d && d.Type == typeof(void))
                    continue;
                stack.Push(expression);
            }

            var tree = stack.GetTree();

            return tree.Count == 1
                ? tree.First()
                : Expression.Block(tree.Reverse());
        }

        protected void Refine(ref Expression expression, CILInterpreterContext context, Type? target = null)
        {
            // TODO: CIL context?
            expression = ExpressionRefiner.RefineExpression(expression, new RefiningContext(context)
            {
                TargetType = target,
            });
        }
    }
}

