using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Translators.Expressions
{
    /// <summary>
    ///     Represents a translator for translating an expression creating a new array and 
    ///     possibly initializing the elements of the new array.
    /// </summary>
    internal class NewArrayExpressionTranslator : ExpressionTranslator<NewArrayExpression>
    {
        /// <inheritdoc/>
        public override string? Translate(NewArrayExpression expression, ExpressionContext context)
        {
            // return out a edgeql set with each element in the dotnet array translated
            var elements = string.Join(", ", expression.Expressions.Select(x => TranslateExpression(x, context)));

            // if its a collection of link-valid types, serialzie it as a set
            if(EdgeDBTypeUtils.IsLink(expression.Type, out _, out _))
                return $"{{ {elements} }}";

            // serialize as a scalar array
            return $"[{elements}]";
        }
    }
}
