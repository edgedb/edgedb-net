using EdgeDB.QueryNodes;
using EdgeDB.Translators.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public static partial class QueryBuilder
    {
        /// <inheritdoc cref="IQueryBuilder{TType, TContext}.With{TVariables}(TVariables)"/>
        public static IQueryBuilder<dynamic, QueryContextVars<TVariables>> With<TVariables>(TVariables variables)
            => new QueryBuilder<dynamic, QueryContextVars<TVariables>>()
                .With(variables).EnterNewContext<QueryContextVars<TVariables>>();

        /// <inheritdoc cref="IQueryBuilder{TType, TContext}.With{TVariables}(TVariables)"/>
        public static IQueryBuilder<dynamic, QueryContextVars<TVariables>> With<TVariables>(
            Expression<Func<QueryContext, TVariables>> variables)
            => new QueryBuilder<dynamic, QueryContextVars<TVariables>>()
                .With(variables).EnterNewContext<QueryContextVars<TVariables>>(); // QueryBuilder<dynamic>.With(variables);
    }

    public partial class QueryBuilder<TType>
    {
        public new static IQueryBuilder<TType, QueryContextSelfVars<TType, TVariables>> With<TVariables>(TVariables variables)
            => new QueryBuilder<TType, QueryContextSelfVars<TType, TVariables>>().With(variables);

        public static IQueryBuilder<TType, QueryContextSelfVars<TType, TVariables>> With<TVariables>(Expression<Func<QueryContextSelf<TType>, TVariables>> variables)
            => new QueryBuilder<TType, QueryContextSelfVars<TType, TVariables>>().WithInternal<TVariables>(variables);
    }

    public partial class QueryBuilder<TType, TContext>
    {
        internal QueryBuilder<TType, QueryContextSelfVars<TType, TVariables>> WithInternal<TVariables>(
            LambdaExpression variables)
        {
            if (variables is null)
                throw new NullReferenceException("Variables cannot be null");

            // check if TVariables is an anonymous type
            if (!typeof(TVariables).IsAnonymousType())
                throw new ArgumentException("Variables must be an anonymous type");

            AddNode<WithNode>(new WithContext(typeof(TType)) {ValuesExpression = variables});

            return EnterNewContext<QueryContextSelfVars<TType, TVariables>>();
        }

        public QueryBuilder<TType, QueryContextSelfVars<TType, TVariables>> With<TVariables>(
            Expression<Func<QueryContext, TVariables>> variables)
            => WithInternal<TVariables>(variables);

        public QueryBuilder<TType, QueryContextSelfVars<TType, TVariables>> With<TVariables>(TVariables variables)
        {
            if (variables is null)
                throw new NullReferenceException("Variables cannot be null");

            // check if TVariables is an anonymous type
            if (!typeof(TVariables).IsAnonymousType())
                throw new ArgumentException("Variables must be an anonymous type");

            // add the properties to our query variables & globals
            foreach (var property in typeof(TVariables).GetProperties())
            {
                var value = property.GetValue(variables);
                // if its scalar, just add it as a query variable
                if (EdgeDBTypeUtils.TryGetScalarType(property.PropertyType, out var scalarInfo))
                {
                    var varName = QueryUtils.GenerateRandomVariableName();
                    QueryVariables.Add(varName, value);
                    QueryGlobals.Add(
                        new QueryGlobal(
                            property.Name,
                            new SubQuery(writer => writer
                                .QueryArgument(scalarInfo.ToString(), varName)
                            )
                        )
                    );
                }
                else if (property.PropertyType.IsAssignableTo(typeof(IQueryBuilder)))
                {
                    // add it as a sub-query
                    QueryGlobals.Add(new QueryGlobal(property.Name, value));
                }
                // TODO: revisit references
                //else if (
                //    EdgeDBTypeUtils.IsLink(property.PropertyType, out var isMultiLink, out var innerType)
                //    && !isMultiLink
                //    && QueryObjectManager.TryGetObjectId(value, out var id))
                //{
                //    _queryGlobals.Add(new QueryGlobal(property.Name, new SubQuery($"(select {property.PropertyType.GetEdgeDBTypeName()} filter .id = <uuid>'{id}')")));
                //}
                else if (ReflectionUtils.IsSubclassOfRawGeneric(typeof(JsonReferenceVariable<>), property.PropertyType))
                {
                    // serialize and add as global and variable
                    var referenceValue = property.PropertyType.GetProperty("Value")!.GetValue(value);
                    var jsonVarName = QueryUtils.GenerateRandomVariableName();
                    QueryVariables.Add(jsonVarName, DataTypes.Json.Serialize(referenceValue));
                    QueryGlobals.Add(new QueryGlobal(property.Name, new SubQuery(writer => writer
                        .QueryArgument("json", jsonVarName)
                    ), value));
                }
                else
                    throw new InvalidOperationException($"Cannot serialize {property.Name}: No serialization strategy found for {property.PropertyType}");
            }

            return EnterNewContext<QueryContextSelfVars<TType, TVariables>>();
        }

        IQueryBuilder<TType, QueryContextSelfVars<TType, TVariables>> IQueryBuilder<TType, TContext>.With<TVariables>(TVariables variables) => With(variables);
        IQueryBuilder<TType, QueryContextSelfVars<TType, TVariables>> IQueryBuilder<TType, TContext>.With<TVariables>(Expression<Func<QueryContextSelf<TType>, TVariables>> variables) => WithInternal<TVariables>(variables);
    }
}
