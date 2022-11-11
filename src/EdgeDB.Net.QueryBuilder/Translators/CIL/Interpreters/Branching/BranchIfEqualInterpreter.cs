using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class BranchIfEqualInterpreter : BaseConditionalBranchInterpreter
    {
        public BranchIfEqualInterpreter()
            : base(OpCodeType.Beq, OpCodeType.Beq_s)
        {
        }

        protected override Expression GetCondition(CILInterpreterContext context)
        {
            var right = context.Stack.PopExp();
            var left = context.Stack.PopExp();

            return Expression.Equal(left, right);
        }
    }
}

