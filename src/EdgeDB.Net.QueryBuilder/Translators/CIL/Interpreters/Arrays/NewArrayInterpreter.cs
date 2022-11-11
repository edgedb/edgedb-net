using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class NewArrayInterpreter : BaseCILInterpreter
    {
        public NewArrayInterpreter()
            : base(OpCodeType.Newarr)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            var size = context.Stack.PopExp();
            var type = instruction.OprandAsType();

            return Expression.NewArrayBounds(type, size);
        }
    }
}

