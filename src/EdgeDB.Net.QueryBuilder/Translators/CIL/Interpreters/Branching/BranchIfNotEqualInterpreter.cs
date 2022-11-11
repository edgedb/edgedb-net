using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class BranchIfNotEqualInterpreter
        : BaseConditionalBranchInterpreter
    {
        public BranchIfNotEqualInterpreter()
            : base(
                  OpCodeType.Bne_un,
                  OpCodeType.Bne_un_s)
        {
        }

        protected override Expression GetCondition(CILInterpreterContext context)
        {
            var right = context.Stack.PopExp();
            var left = context.Stack.PopExp();

            return Expression.NotEqual(left, right);
        }
    }
}

