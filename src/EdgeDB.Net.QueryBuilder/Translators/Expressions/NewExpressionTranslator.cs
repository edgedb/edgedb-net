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
    ///     Represents a translator for translating an expression with a constructor call.
    /// </summary>
    internal class NewExpressionTranslator : ExpressionTranslator<NewExpression>
    {
        /// <inheritdoc/>
        public override void Translate(NewExpression expression, ExpressionContext context, QueryStringWriter writer)
        {
            var map = new Dictionary<PropertyInfo, EdgeDBPropertyInfo>(
                EdgeDBPropertyMapInfo.Create(expression.Type).Properties
                .Select(x => new KeyValuePair<PropertyInfo, EdgeDBPropertyInfo>(x.PropertyInfo, x)));

            List<(EdgeDBPropertyInfo, Expression)> expressions = new();

            for(int i = 0; i != expression.Members!.Count; i++)
            {
                var binding = expression.Members[i];
                var value = expression.Arguments[i];

                if (binding is not PropertyInfo propInfo || !map.TryGetValue(propInfo, out var edgedbPropInfo))
                    continue;

                expressions.Add((edgedbPropInfo, value));
            }

            InitializationTranslator.Translate(
                expressions, context, writer
            );
        }
    }
}
