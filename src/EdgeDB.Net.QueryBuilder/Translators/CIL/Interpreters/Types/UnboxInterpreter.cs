using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class UnboxInterpreter : BaseCILInterpreter
    {
        public UnboxInterpreter()
            : base(OpCodeType.Unbox, OpCodeType.Unbox_any)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            var value = context.Stack.PopExp();
            var type = instruction.OprandAsType();

            return Expression.Unbox(value, type);
        }
    }
}

