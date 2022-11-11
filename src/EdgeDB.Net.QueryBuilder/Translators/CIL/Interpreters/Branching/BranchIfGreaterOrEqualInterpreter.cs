using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class BranchIfGreaterOrEqualInterpreter
        : BaseConditionalBranchInterpreter
    {
        public BranchIfGreaterOrEqualInterpreter()
            : base(
                  OpCodeType.Bge,
                  OpCodeType.Bge_s,
                  OpCodeType.Bge_un,
                  OpCodeType.Bge_un_s)
        {
        }

        protected override Expression GetCondition(CILInterpreterContext context)
        {
            var right = context.Stack.PopExp();
            var left = context.Stack.PopExp();
            return Expression.GreaterThanOrEqual(left, right);
        }
    }
}

