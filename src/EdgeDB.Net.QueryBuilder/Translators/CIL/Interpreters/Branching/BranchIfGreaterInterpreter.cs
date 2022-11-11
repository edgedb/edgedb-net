using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class BranchIfGreaterInterpreter
        : BaseConditionalBranchInterpreter
    {
        public BranchIfGreaterInterpreter()
            : base(
                  OpCodeType.Bgt,
                  OpCodeType.Bgt_s,
                  OpCodeType.Bgt_un,
                  OpCodeType.Bgt_un_s)
        {
        }

        protected override Expression GetCondition(CILInterpreterContext context)
        {
            var right = context.Stack.PopExp();
            var left = context.Stack.PopExp();

            return Expression.GreaterThan(left, right);
        }
    }
}

