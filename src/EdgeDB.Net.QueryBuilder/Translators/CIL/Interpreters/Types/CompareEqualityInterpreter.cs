using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class CompareEqualityInterpreter : BaseCILInterpreter
    {
        public CompareEqualityInterpreter()
            : base(OpCodeType.Ceq)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            var right = context.Stack.PopExp();
            var left = context.Stack.PopExp();

            return Expression.Equal(left, right);
        }
    }
}

