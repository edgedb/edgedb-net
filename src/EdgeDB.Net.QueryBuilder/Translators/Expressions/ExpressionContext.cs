using EdgeDB.QueryNodes;
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
    ///     Represents context used by an <see cref="ExpressionTranslator"/>.
    /// </summary>
    internal class ExpressionContext
    {
        /// <summary>
        ///     Gets the calling nodes context.
        /// </summary>
        public NodeContext NodeContext { get; }

        /// <summary>
        ///     Gets the root lambda function that is currently being translated.
        /// </summary>
        public LambdaExpression RootExpression { get; set; }
        
        /// <summary>
        ///     Gets a collection of method parameters within the <see cref="RootExpression"/>.
        /// </summary>
        public Dictionary<string, Type> Parameters { get; }

        /// <summary>
        ///     Gets or sets whether or not to serialize string without quotes.
        /// </summary>
        public bool StringWithoutQuotes { get; set; }

        /// <summary>
        ///     Gets or sets the current type scope. This is used when verifying shaped.
        /// </summary>
        public Type? LocalScope { get; set; }

        /// <summary>
        ///     Gets or sets whether or not the current expressions is or is within a shape.
        /// </summary>
        public bool IsShape { get; set; }

        /// <summary>
        ///     Gets or sets whether or not the current expression has an initialization 
        ///     operator, ex: ':=, +=, -='.
        /// </summary>
        public bool HasInitializationOperator { get; set; }

        /// <summary>
        ///     Gets or sets whether or not to include a self reference.
        ///     Ex: <see langword="true"/>: '.name', <see langword="false"/>: 'name'
        /// </summary>
        /// 
        public bool IncludeSelfReference { get; set; } = true;

        /// <summary>
        ///     Gets whether or not the current expression tree is a free object.
        /// </summary>
        public bool IsFreeObject
            => NodeContext is SelectContext selectContext && selectContext.IsFreeObject;

        /// <summary>
        ///     Gets the query node requesting the translation; otherwise <see langword="null"/>
        ///     if the translation was not requested by a query node.
        /// </summary>
        public QueryNode? Node { get; }

        /// <summary>
        ///     The collection of query variables.
        /// </summary>
        internal readonly IDictionary<string, object?> QueryArguments;

        /// <summary>
        ///     The collection of query globals.
        /// </summary>
        internal readonly List<QueryGlobal> Globals;

        /// <summary>
        ///     Constructs a new <see cref="ExpressionContext"/>.
        /// </summary>
        /// <param name="context">The calling nodes context.</param>
        /// <param name="rootExpression">The root lambda expression.</param>
        /// <param name="queryArguments">The query arguments collection.</param>
        /// <param name="globals">The query global collection.</param>
        public ExpressionContext(NodeContext context, LambdaExpression rootExpression, 
            IDictionary<string, object?> queryArguments, List<QueryGlobal> globals,
            QueryNode? node = null)
        {
            Node = node;
            RootExpression = rootExpression;
            QueryArguments = queryArguments;
            NodeContext = context;
            Globals = globals;

            Parameters = rootExpression.Parameters.ToDictionary(x => x.Name!, x => x.Type);
        }

        /// <summary>
        ///     Adds a query variable.
        /// </summary>
        /// <param name="value">The value of the variable</param>
        /// <returns>The randomly generated name of the variable.</returns>
        public string AddVariable(object? value)
        {
            var name = QueryUtils.GenerateRandomVariableName();
            QueryArguments[name] = value;
            return name;
        }

        /// <summary>
        ///     Sets a query variable with the given name.
        /// </summary>
        /// <param name="name">The name of the query variable.</param>
        /// <param name="value">The value of the query variable.</param>
        public void SetVariable(string name, object? value)
            => QueryArguments[name] = value;

        /// <summary>
        ///     Attempts to fetch a query global by reference.
        /// </summary>
        /// <param name="reference">The reference of the global.</param>
        /// <param name="global">The out parameter containing the global.</param>
        /// <returns>
        ///     <see langword="true"/> if a global could be found with the reference; 
        ///     otherwise <see langword="false"/>.
        /// </returns>
        public bool TryGetGlobal(object? reference, [MaybeNullWhen(false)]out QueryGlobal global)
        {
            global = Globals.FirstOrDefault(x => x.Reference == reference);
            return global != null;
        }

        /// <summary>
        ///     Attempts to fetch a query global by reference.
        /// </summary>
        /// <param name="name">The name of the global.</param>
        /// <param name="global">The out parameter containing the global.</param>
        /// <returns>
        ///     <see langword="true"/> if a global could be found with the reference; 
        ///     otherwise <see langword="false"/>.
        /// </returns>
        public bool TryGetGlobal(string? name, [MaybeNullWhen(false)] out QueryGlobal global)
        {
            global = Globals.FirstOrDefault(x => x.Name == name);
            return global != null;
        }

        /// <summary>
        ///     Gets or adds a global with the given reference and value.
        /// </summary>
        /// <param name="reference">The reference of the global.</param>
        /// <param name="value">The value to add if no global exists with the given reference.</param>
        /// <returns>The name of the global.</returns>
        public string GetOrAddGlobal(object? reference, object? value)
        {
            if (reference is not null && TryGetGlobal(reference, out var global))
                return global.Name;
           
            var name = QueryUtils.GenerateRandomVariableName();
            SetGlobal(name, value, reference);
            return name;
        }

        /// <summary>
        ///     Sets a global with the given name, value, and reference.
        /// </summary>
        /// <param name="name">The name of the global to set.</param>
        /// <param name="value">The value of the global to set.</param>
        /// <param name="reference">The reference of the global to set.</param>
        public void SetGlobal(string name, object? value, object? reference)
        {
            var global = new QueryGlobal(name, value, reference);
            Globals.Add(global);
        }

        public void AddChildQuery(SubQuery query)
        {
            var name = QueryUtils.GenerateRandomVariableName();
            NodeContext.ChildQueries.Add(name, query);
        }

        /// <summary>
        ///     Enters a new context with the given modification delegate.
        /// </summary>
        /// <param name="func">The modifying delegate.</param>
        /// <returns>The new modified context.</returns>
        public ExpressionContext Enter(Action<ExpressionContext> func)
        {
            var exp = (ExpressionContext)MemberwiseClone();
            func(exp);
            return exp;
        }
    }
}
