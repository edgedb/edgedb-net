using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL
{
    internal class BinaryRefiner : BaseRefiner<BinaryExpression>
    {
        protected override Expression Refine(BinaryExpression expression, RefiningContext context)
        {
            // ensure left is same type as right, if possible
            var right = RefineOther(expression.Right, context.Enter(x => x.TargetType = expression.Left.Type));

            return expression.Update(expression.Left, expression.Conversion, right);
        }
    }
}

