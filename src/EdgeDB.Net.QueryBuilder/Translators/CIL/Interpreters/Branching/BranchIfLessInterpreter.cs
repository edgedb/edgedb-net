using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class BranchIfLessInterpreter
        : BaseConditionalBranchInterpreter
    {
        public BranchIfLessInterpreter()
            : base(
                  OpCodeType.Blt,
                  OpCodeType.Blt_s,
                  OpCodeType.Blt_un,
                  OpCodeType.Blt_un_s)
        {
        }

        protected override Expression GetCondition(CILInterpreterContext context)
        {
            var right = context.Stack.PopExp();
            var left = context.Stack.PopExp();

            return Expression.LessThan(left, right);
        }
    }
}

