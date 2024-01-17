using EdgeDB.QueryNodes.Contexts;
using EdgeDB.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    /// <summary>
    ///     Represents a generic root query node.
    /// </summary>
    /// <typeparam name="TContext">The context type for the node.</typeparam>
    internal abstract class QueryNode<TContext> : QueryNode
        where TContext : NodeContext
    {
        /// <summary>
        ///     Constructs a new query node with the given builder.
        /// </summary>
        /// <param name="builder">The node builder used to build this node.</param>
        protected QueryNode(NodeBuilder builder) : base(builder) { }

        /// <summary>
        ///     Gets the context for this node.
        /// </summary>
        internal new TContext Context
            => (TContext)Builder.Context;
    }

    /// <summary>
    ///     Represents an abstract root query node.
    /// </summary>
    internal abstract class QueryNode
    {
        /// <summary>
        ///     Gets the context used to translate expressions for this node.
        /// </summary>
        public NodeTranslationContext NodeTranslationContext { get; }

        /// <summary>
        ///     Gets whether or not this node was automatically generated.
        /// </summary>
        public bool IsAutoGenerated
            => Builder.IsAutoGenerated;

        /// <summary>
        ///     Gets or sets whether or not this node requires introspection to build.
        /// </summary>
        public virtual bool RequiresIntrospection
        {
            get => _requiresIntrospection || SubNodes.Any(x => x.RequiresIntrospection);
            set => _requiresIntrospection = value;
        }

        /// <summary>
        ///     Gets or sets the schema introspection data.
        /// </summary>
        public SchemaInfo? SchemaInfo { get; set; }

        /// <summary>
        ///     The operating type within the context of the query builder.
        /// </summary>
        public readonly Type OperatingType;

        /// <summary>
        ///     Gets a collection of child nodes.
        /// </summary>
        internal List<QueryNode> SubNodes { get; } = new();

        /// <summary>
        ///     Gets the parent node that created this node.
        /// </summary>
        internal QueryNode? Parent { get; set; }

        /// <summary>
        ///     Gets a collection of global variables this node references.
        /// </summary>
        internal List<QueryGlobal> ReferencedGlobals { get; } = new();

        /// <summary>
        ///     Gets the context for this node.
        /// </summary>
        internal NodeContext Context
            => Builder.Context;

        /// <summary>
        ///     The builder used to build this node.
        /// </summary>
        internal readonly NodeBuilder Builder;

        private bool _requiresIntrospection;

        /// <summary>
        ///     Constructs a new query node with the given builder.
        /// </summary>
        /// <param name="builder">the builder used to build this node.</param>
        public QueryNode(NodeBuilder builder)
        {
            Builder = builder;
            OperatingType = GetOperatingType();
            NodeTranslationContext = new(this);
        }

        /// <summary>
        ///     Visits the current node, completing the first phase of this nodes build process.
        /// </summary>
        /// <remarks>
        ///     This function modifies <see cref="RequiresIntrospection"/>,
        ///     <see cref="SchemaInfo"/> should be populated for the final build step,
        ///     <see cref="FinalizeQuery"/>.
        /// </remarks>
        public virtual void Visit(){}

        /// <summary>
        ///     Finalizes the nodes query, completing the second and final phase of this
        ///     nodes build process by writing the nodes query string to the writer.
        /// </summary>
        public abstract void FinalizeQuery(QueryStringWriter writer);

        /// <summary>
        ///     Sets a query variable with the given name.
        /// </summary>
        /// <param name="name">The name of the variable to set.</param>
        /// <param name="value">The value of the variable to set.</param>
        protected void SetVariable(string name, object? value)
        {
            Builder.QueryVariables[name] = value;
        }

        /// <summary>
        ///     Sets a query global with the given name and reference.
        /// </summary>
        /// <param name="name">The name of the global to set.</param>
        /// <param name="value">The value of the global to set.</param>
        /// <param name="reference">The reference of the global to set.</param>
        protected void SetGlobal(string name, object? value, object? reference)
        {
            var global = new QueryGlobal(name, value, reference);
            Builder.QueryGlobals.Add(global);
            ReferencedGlobals.Add(global);
        }

        /// <summary>
        ///     Gets or adds a global with the given reference and value.
        /// </summary>
        /// <param name="reference">The reference of the global.</param>
        /// <param name="value">The value to add if no global exists with the given reference.</param>
        /// <returns>The name of the global.</returns>
        protected string GetOrAddGlobal(object? reference, object? value)
        {
            var global = Builder.QueryGlobals.FirstOrDefault(x => x.Value == value);
            if (global != null)
                return global.Name;
            var name = QueryUtils.GenerateRandomVariableName();
            SetGlobal(name, value, reference);
            return name;
        }

        /// <summary>
        ///     Gets the current operating type in the context of the query builder.
        /// </summary>
        internal Type GetOperatingType()
            => Context.CurrentType.IsAssignableTo(typeof(IJsonVariable))
                ? Context.CurrentType.GenericTypeArguments[0]
                : Context.CurrentType;

        /// <summary>
        ///     Translates a given lambda expression into EdgeQL.
        /// </summary>
        /// <remarks>
        ///     This function tracks the translations globals and stores them in
        ///     this nodes <see cref="ReferencedGlobals"/>.
        /// </remarks>
        /// <param name="expression">The expression to translate.</param>
        /// <param name="writer">The query string writer to append the translated expression to.</param>
        protected void TranslateExpression(LambdaExpression expression, QueryStringWriter writer)
        {
            using var consumer = NodeTranslationContext.CreateContextConsumer(expression);
            ExpressionTranslator.Translate(expression, consumer, writer);
        }

        /// <summary>
        ///     Translates a given expression into EdgeQL.
        /// </summary>
        /// <remarks>
        ///     This function tracks the translations globals and stores them in
        ///     this nodes <see cref="ReferencedGlobals"/>.
        /// </remarks>
        /// <param name="root">The root delegate that this expression is apart of.</param>
        /// <param name="expression">The expression to translate.</param>
        /// <param name="writer">The query string writer to write the translated edgeql.</param>
        /// <returns>The EdgeQL version of the expression.</returns>
        protected void TranslateExpression(LambdaExpression root, Expression expression, QueryStringWriter writer)
        {
            using var consumer = NodeTranslationContext.CreateContextConsumer(root);
            ExpressionTranslator.ContextualTranslate(expression, consumer, writer);
        }

        internal void ReplaceSubqueryAsLiteral(QueryStringWriter writer, QueryGlobal global, Action<QueryGlobal, QueryStringWriter> compile)
        {
            if (!writer.TryGetLabeled(global.Name, out var markers))
                return;

            foreach (var marker in markers)
            {
                marker.Replace(writer =>
                {
                    compile(global, writer);
                });
            }



            // var index = writer.IndexOf(global.Name);
            //
            // if (index is -1)
            //     throw new InvalidOperationException("Global could not be found within the query string");
            //
            // string? cached = null;
            //
            // while (index is not -1)
            // {
            //     if (cached is null)
            //     {
            //         var globalWriter = writer.GetPositionalWriter(index);
            //         compile(global, globalWriter);
            //         cached = globalWriter.ToString();
            //     }
            //     else
            //     {
            //         writer.Insert(index, cached);
            //     }
            //
            //     index = writer.IndexOf(global.Name);
            // }
        }
    }
}
