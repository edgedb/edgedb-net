using EdgeDB.Operators;
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
        public abstract void Translate(TExpression expression, ExpressionContext context, QueryStringWriter writer);

        /// <summary>
        ///     Overrides the default translation method to call the generic one.
        /// </summary>
        /// <inheritdoc/>
        public override void Translate(Expression expression, ExpressionContext context, QueryStringWriter writer)
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
        ///     An array of all edgeql operators.
        /// </summary>
        private static readonly IEdgeQLOperator[] _operators;

        /// <summary>
        ///     A collection of expression types (key) and operators (value).
        /// </summary>
        private static readonly Dictionary<ExpressionType, IEdgeQLOperator> _expressionOperators;

        /// <summary>
        ///     Statically initializes the translator, setting <see cref="_translators"/>,
        ///     <see cref="_operators"/>, and <see cref="_expressionOperators"/>.
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

            // load operators
            _operators = types.Where(x => x.ImplementedInterfaces.Any(x => x == typeof(IEdgeQLOperator))).Select(x => (IEdgeQLOperator)Activator.CreateInstance(x)!).ToArray();

            // set the expression operators
            _expressionOperators = _operators.Where(x => x.Expression is not null).DistinctBy(x => x.Expression).ToDictionary(x => (ExpressionType)x.Expression!, x => x);
        }

        public static QueryStringWriter.Proxy Proxy(Expression expression, ExpressionContext context)
        {
            return writer => ContextualTranslate(expression, context, writer);
        }

        protected static QueryStringWriter.Proxy[] Proxy(ExpressionContext context, params Expression[] expressions)
        {
            var proxies = new QueryStringWriter.Proxy[expressions.Length];

            for (var i = 0; i != expressions.Length; i++)
            {
                proxies[i] = Proxy(expressions[i], context);
            }

            return proxies;
        }

        /// <summary>
        ///     Attempts to get a <see cref="IEdgeQLOperator"/> for the given <see cref="ExpressionType"/>.
        /// </summary>
        /// <param name="type">The expression type to get the operator for.</param>
        /// <param name="edgeqlOperator">The out parameter containing the operator if found.</param>
        /// <returns>
        ///     <see langword="true"/> if an operator was found for the given
        ///     <see cref="ExpressionType"/>; otherwise <see langword="false"/>.
        /// </returns>
        protected static bool TryGetExpressionOperator(
            ExpressionType type,
            [MaybeNullWhen(false)] out IEdgeQLOperator edgeqlOperator)
            => _expressionOperators.TryGetValue(type, out edgeqlOperator);

        /// <summary>
        ///     Translate the given expression into the EdgeQL equivalent.
        /// </summary>
        /// <param name="expression">The expression to translate.</param>
        /// <param name="context">The context for the translation.</param>
        /// <param name="writer">The query string builder to populate with the translated expression.</param>
        /// <returns>The string form of the expression.</returns>
        public abstract void Translate(Expression expression, ExpressionContext context, QueryStringWriter writer);

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
            QueryStringWriter writer)
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
        public static void Translate(LambdaExpression expression, ExpressionContext context, QueryStringWriter writer)
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
            QueryStringWriter writer)
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
                translator.Translate(expression, context, writer);
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
            QueryStringWriter writer)
            => TranslateExpression(expression, context, writer);
    }
}
