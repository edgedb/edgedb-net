using EdgeDB.CIL.Interpreters;
using System;
using System.Linq.Expressions;
using System.Reflection.Metadata;

namespace EdgeDB.CIL
{
    internal class ConstantRefiner : BaseRefiner<ConstantExpression>
    {
        protected override Expression Refine(ConstantExpression expression, RefiningContext context)
        {
            if (context.TargetType is null)
                return expression;

            if (expression.Type.IsSignedNumber() && context.TargetType.IsUnsignedNumber())
            {
                // TODO: size change needs a check here?
                var converted = expression.Type.ConvertToTargetNumber(expression.Value!, context.TargetType);
                return Expression.Constant(converted, context.TargetType);
            }
            // since there is no bool type in CIL we must convert an int to bool
            else if (IsBooleanConversion(expression.Type, context.TargetType))
            {
                return Expression.Constant((int)expression.Value! > 0, context.TargetType);
            }
            // TODO: more type handle conversion?
            else if (expression.Type == typeof(EntityHandle) && (context.TargetType?.IsRuntimeHandle() ?? false))
            {
                var handle = ((EntityHandle)expression.Value!).ConvertToRuntimeHandle(context.Module!);
                return Expression.Constant(handle);
            }

            return expression;
        }

        private static bool IsBooleanConversion(Type source, Type target)
            => source == typeof(int) && target == typeof(bool);


    }
}

