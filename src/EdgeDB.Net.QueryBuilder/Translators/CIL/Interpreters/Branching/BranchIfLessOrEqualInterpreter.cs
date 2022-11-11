using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class BranchIfLessOrEqualInterpreter
        : BaseConditionalBranchInterpreter
    {
        public BranchIfLessOrEqualInterpreter()
            : base(
                  OpCodeType.Ble,
                  OpCodeType.Ble_s,
                  OpCodeType.Ble_un,
                  OpCodeType.Ble_un_s)
        {
        }

        protected override Expression GetCondition(CILInterpreterContext context)
        {
            var right = context.Stack.PopExp();
            var left = context.Stack.PopExp();

            return Expression.LessThanOrEqual(left, right);
        }
    }
}

