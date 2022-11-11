using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class IsInstanceInterpreter : BaseCILInterpreter
    {
        public IsInstanceInterpreter()
            : base(OpCodeType.Isinst)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            var testObject = context.Stack.PopExp();
            var type = instruction.OprandAsType();

            return Expression.TypeIs(testObject, type);
        }
    }
}

