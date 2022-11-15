using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL
{
    internal class UnaryRefiner : BaseRefiner<UnaryExpression>
    {
        protected override Expression Refine(UnaryExpression expression, RefiningContext context)
        {
            // simply the convert if possible by directly changing the type
            var convertBody = expression.Operand;

            // if its an upscale of numbers, use convert.changetype
            if (convertBody.Type.IsNumericType() && expression.Type.IsNumericType() && convertBody is ConstantExpression cnst)
            {
                // see if we can also match the context type
                if (context.TargetType is not null && expression.NodeType is ExpressionType.Convert or ExpressionType.ConvertChecked)
                {
                    return Expression.Constant(Convert.ChangeType(cnst.Value, context.TargetType));
                }
                else
                {
                    convertBody = Expression.Constant(Convert.ChangeType(cnst.Value, expression.Type));
                }
            }

            return expression.Update(RefineOther(convertBody, context));
        }
    }
}

