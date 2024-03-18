using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.LinqBuilders
{
    internal sealed class WhereBuilder : BaseLinqBuilder
    {
        public List<Expression> Conditions { get; }
        public ParameterExpression? Parameter { get; set; }

        public WhereBuilder()
            : base(QueryableMethods.Where)
        {
            Conditions = new();
        }

        public override void Visit(PartContext context)
        {
            if (Unquote(context.Parameters[0]) is not LambdaExpression lmd)
                throw new NotSupportedException();

            Parameter = lmd.Parameters[0];

            Conditions.Add(lmd.Body);
        }

        private void AcceptCondition(Expression condition)
            => Conditions.Add(condition);

        public override void Build(GenericlessQueryBuilder builder, BaseLinqBuilder? next)
        {
            // if the next node is another where, add the condition to the next node
            if(next is WhereBuilder nextWhere)
            {
                nextWhere.AcceptCondition(Conditions[0]);
                return;
            }

            Expression condition = Conditions.Count > 1
                ? Conditions.Aggregate((a,b) => Expression.AndAlso(b,a)) // reverse order of conditions to match the order defined by user
                : Conditions[0];

            builder.Filter(Expression.Lambda(condition, Parameter!));
        }
    }
}
