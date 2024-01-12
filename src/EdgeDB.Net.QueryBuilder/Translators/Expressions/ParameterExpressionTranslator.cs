using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Translators.Expressions
{
    /// <summary>
    ///     Represents a translator for translating a parameter within a lambda function.
    /// </summary>
    /// <remarks>
    ///     This translator is only called when a parameter is directly referenced, normally
    ///     A parameter reference is accessed which will cause a <c>.x</c> to be added where as
    ///     this translator will just serialize the parameters name.
    /// </remarks>
    internal class ParameterExpressionTranslator : ExpressionTranslator<ParameterExpression>
    {
        /// <inheritdoc/>
        public override void Translate(ParameterExpression expression, ExpressionContext context, QueryStringWriter writer)
        {
            writer.Append(expression.Name!);
        }
    }
}
