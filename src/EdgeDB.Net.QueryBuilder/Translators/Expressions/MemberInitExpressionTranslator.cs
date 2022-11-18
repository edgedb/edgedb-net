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
            List<(EdgeDBPropertyInfo, Expression)> expressions = new();

            var map = new Dictionary<PropertyInfo, EdgeDBPropertyInfo>(
                EdgeDBPropertyMapInfo.Create(expression.Type).Properties
                .Select(x => new KeyValuePair<PropertyInfo, EdgeDBPropertyInfo>(x.PropertyInfo, x)));

            // ctor
            // TODO: custom type converters?
            //if(expression.NewExpression.Arguments is not null && expression.NewExpression.Arguments.Any())
            //    for(int i = 0; i != expression.NewExpression.Arguments.Count; i++)
            //        expressions.Add((expression.NewExpression.Members![i], expression.NewExpression.Arguments[i]));

            // members
            foreach (var binding in expression.Bindings.Where(x => x is MemberAssignment).Cast<MemberAssignment>())
            {
                if (binding.Member is not PropertyInfo propInfo || !map.TryGetValue(propInfo, out var edgedbPropInfo))
                    continue;
                
                expressions.Add((edgedbPropInfo, binding.Expression));
            }

            return InitializationTranslator.Translate(expressions.ToDictionary(x => x.Item1, x => x.Item2), context);
        }
    }
}
