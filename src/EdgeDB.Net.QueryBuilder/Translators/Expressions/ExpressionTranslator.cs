using EdgeDB.QueryNodes;
using EdgeDB.Translators;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents an abstract translator that can translate the given <typeparamref name="TExpression"/>.
    /// </summary>
    /// <typeparam name="TExpression">The expression type that this translator can translate.</typeparam>
    internal abstract class ExpressionTranslator<TExpression> : ExpressionTranslator
        where TExpression : Expression
    {
        /// <summary>
        ///     Translate the given <typeparamref name="TExpression"/> into the EdgeQL equivalent.
        /// </summary>
        /// <param name="expression">The expression to translate.</param>
        /// <param name="context">The context for the translation.</param>
        /// <param name="writer">The query string builder to populate with the translated expression.</param>
        public abstract void Translate(TExpression expression, ExpressionContext context, QueryWriter writer);

        /// <summary>
        ///     Overrides the default translation method to call the generic one.
        /// </summary>
        /// <inheritdoc/>
        public override void Translate(Expression expression, ExpressionContext context, QueryWriter writer)
            => Translate((TExpression)expression, context, writer);
    }

    /// <summary>
    ///     Represents a translator capable of translating expressions to EdgeQL.
    /// </summary>
    internal abstract class ExpressionTranslator
    {
        /// <summary>
        ///     The collection of expression (key) and translators (value).
        /// </summary>
        private static readonly Dictionary<Type, ExpressionTranslator> _translators = new();

        /// <summary>
        ///     Statically initializes the translator, setting <see cref="_translators"/>.
        /// </summary>
        static ExpressionTranslator()
        {
            var types = Assembly.GetExecutingAssembly().DefinedTypes;

            // load current translators
            var translators = types.Where(x => x.BaseType?.Name == "ExpressionTranslator`1");

            foreach(var translator in translators)
            {
                _translators[translator.BaseType!.GenericTypeArguments[0]] = (ExpressionTranslator)Activator.CreateInstance(translator)!;
            }
        }

        public static string? UnsafeExpressionAsString(Expression expression)
        {
            if (expression is ConstantExpression constantExpression &&
                constantExpression.Type.IsAssignableTo(typeof(string)))
                return (string?)constantExpression.Value;


            var expressionResult = Expression
                .Lambda(expression)
                .Compile()
                .DynamicInvoke();

            if (expressionResult is not string strResult)
                throw new ArgumentException(
                    $"Expected expression {expression} to evaluate to a string, but " +
                    $"got {expressionResult?.GetType().ToString() ?? "NULL"}"
                );

            return strResult;
        }

        public static WriterProxy Proxy(Expression expression, ExpressionContext expressionContext, string? label = null)
        {
            if (label is not null)
            {
                return writer =>
                    writer.LabelVerbose(
                        label,
                        Defer.This(() => $"Translation of {expression}"),
                        Value.Of(writer => ContextualTranslate(expression, expressionContext, writer))
                    );
            }

            return writer => ContextualTranslate(expression, expressionContext, writer);
        }

        protected static WriterProxy[] Proxy(ExpressionContext context, params Expression[] expressions)
        {
            var proxies = new WriterProxy[expressions.Length];

            for (var i = 0; i != expressions.Length; i++)
            {
                proxies[i] = Proxy(expressions[i], context);
            }

            return proxies;
        }

        /// <summary>
        ///     Translate the given expression into the EdgeQL equivalent.
        /// </summary>
        /// <param name="expression">The expression to translate.</param>
        /// <param name="context">The context for the translation.</param>
        /// <param name="writer">The query string builder to populate with the translated expression.</param>
        /// <returns>The string form of the expression.</returns>
        public abstract void Translate(Expression expression, ExpressionContext context, QueryWriter writer);

        /// <summary>
        ///     Translates a lambda expression into the EdgeQL equivalent.
        /// </summary>
        /// <remarks>
        ///     This function <i>can</i> add globals and query variables.
        /// </remarks>
        /// <param name="expression">The expression to translate.</param>
        /// <param name="queryArguments">The collection of query arguments.</param>
        /// <param name="nodeContext">The context of the calling node.</param>
        /// <param name="globals">The collection of globals.</param>
        /// <param name="writer">The query string builder to populate with the translated expression.</param>
        public static void Translate(
            LambdaExpression expression,
            IDictionary<string, object?> queryArguments,
            NodeContext nodeContext,
            List<QueryGlobal> globals,
            QueryWriter writer)
        {
            var context = new ExpressionContext(nodeContext, expression, queryArguments, globals);
            TranslateExpression(expression.Body, context, writer);
        }

        /// <summary>
        ///     Translates a lambda expression into the edgeql equivalent.
        /// </summary>
        /// <remarks>
        ///     This function <i>can</i> add globals and query variables.
        /// </remarks>
        /// <param name="expression">The expression to translate.</param>
        /// <param name="context">The translation context.</param>
        /// <param name="writer">The query string builder to populate with the translated expression.</param>
        public static void Translate(LambdaExpression expression, ExpressionContext context, QueryWriter writer)
            => TranslateExpression(expression.Body, context, writer);

        /// <summary>
        ///     Translates a sub expression into its edgeql equivalent.
        /// </summary>
        /// <param name="expression">The expression to translate.</param>
        /// <param name="context">The current context of the calling translator.</param>
        /// <param name="writer">The query string builder to populate with the translated expression.</param>
        /// <exception cref="NotSupportedException">No translator was found for the given expression.</exception>
        protected static void TranslateExpression(
            Expression expression,
            ExpressionContext context,
            QueryWriter writer)
        {
            // special fallthru for lambda functions
            if (expression is LambdaExpression lambda)
            {
                _translators[typeof(LambdaExpression)].Translate(lambda, context, writer);
                return;
            }

            // since some expression classes a private, this while loop will
            // find the first base class that isn't private and use that class to find a translator.
            var expType = expression.GetType();
            while (!expType.IsPublic)
                expType = expType.BaseType!;

            // if we can find a translator for the expression type, use it.
            if (_translators.TryGetValue(expType, out var translator))
            {
                writer.LabelVerbose(
                    expType.Name,
                    Defer.This(() => $"Translated form of '{expression}'"),
                    Value.Of(writer => translator.Translate(expression, context, writer))
                );
                return;
            }

            throw new NotSupportedException(
                $"Failed to find translator for expression type: {expType.Name}.{expression.NodeType}"
            );
        }

        /// <summary>
        ///     Translates a given expression with the provided expression context.
        /// </summary>
        /// <remarks>
        ///     This method requires <see cref="ExpressionContext"/> and should only be called
        ///     by child translators that depend on expression translators.
        /// </remarks>
        /// <param name="expression">The expression to translate.</param>
        /// <param name="context">The current context of the calling translator.</param>
        /// <param name="writer">The query string builder to populate with the translated expression.</param>
        /// <exception cref="NotSupportedException">No translator was found for the given expression.</exception>
        internal static void ContextualTranslate(
            Expression expression,
            ExpressionContext context,
            QueryWriter writer)
            => TranslateExpression(expression, context, writer);
    }
}
