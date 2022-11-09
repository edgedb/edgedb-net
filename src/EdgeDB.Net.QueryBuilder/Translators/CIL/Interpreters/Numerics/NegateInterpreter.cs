using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class NegateInterpreter : BaseCILInterpreter
    {
        public NegateInterpreter()
            : base(OpCodeType.Neg)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            var value = context.Stack.Pop();
            return Expression.Negate(value);
        }
    }
}

