using EdgeDB.Operators;
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
        public override string? Translate(NewExpression expression, ExpressionContext context)
            => InitializationTranslator.Translate(expression.Members!.Select((x, i) 
                => (x, expression.Arguments[i])
            ).ToDictionary(x => x.x, x => x.Item2), context);
    }
}
