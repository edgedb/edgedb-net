using EdgeDB.QueryNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Translators.Expressions
{
    /// <summary>
    ///     Represents a translator for translating an expression calling a 
    ///     constructor and initializing one or more members of the new object.
    /// </summary>
    internal class MemberInitExpressionTranslator : ExpressionTranslator<MemberInitExpression>
    {
        /// <inheritdoc/>
        public override string? Translate(MemberInitExpression expression, ExpressionContext context)
        {
            List<(MemberInfo, Expression)> expressions = new();

            if(expression.NewExpression.Arguments is not null && expression.NewExpression.Arguments.Any())
                for(int i = 0; i != expression.NewExpression.Arguments.Count; i++)
                    expressions.Add((expression.NewExpression.Members![i], expression.NewExpression.Arguments[i]));

            foreach (var binding in expression.Bindings.Where(x => x is MemberAssignment).Cast<MemberAssignment>())
                expressions.Add((binding.Member, binding.Expression));

            return InitializationTranslator.Translate(expressions.ToDictionary(x => x.Item1, x => x.Item2), context);
        }
    }
}
