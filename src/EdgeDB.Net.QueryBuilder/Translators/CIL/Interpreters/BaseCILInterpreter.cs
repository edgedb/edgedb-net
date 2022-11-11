using System;
using System.Diagnostics;
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
            => InterpretInstructions(context.Reader.ReadTo(label), context);

        protected Expression InterpretInstructions(IEnumerable<Instruction> instructions, CILInterpreterContext context)
        {
            var stack = new InterpreterStack();
            var blockContext = context.Enter(x =>
            {
                x.Stack = stack;
            });

            foreach (var instruction in instructions)
            {
                var expression = CILInterpreter.Interpret(instruction, context);
                if (expression is DefaultExpression d && d.Type == typeof(void))
                    continue;
                stack.Push(expression);
            }

            var tree = stack.GetTree();

            return tree.Count == 1
                ? tree.First()
                : Expression.Block(tree.Reverse());
        }
    }
}

