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
        public static IQueryBuilder<dynamic, QueryContext<dynamic, TVariables>> With<TVariables>(TVariables variables)
            => QueryBuilder<dynamic>.With(variables);

        /// <inheritdoc cref="IQueryBuilder{TType, TContext}.With{TVariables}(TVariables)"/>
        public static IQueryBuilder<dynamic, QueryContext<dynamic, TVariables>> With<TVariables>(Expression<Func<QueryContext<dynamic>, TVariables>> variables)
            => QueryBuilder<dynamic>.With(variables);
    }

    public partial class QueryBuilder<TType>
    {
        new public static IQueryBuilder<TType, QueryContext<TType, TVariables>> With<TVariables>(TVariables variables)
            => new QueryBuilder<TType, TVariables>().With(variables);

        new public static IQueryBuilder<TType, QueryContext<TType, TVariables>> With<TVariables>(Expression<Func<QueryContext<TType>, TVariables>> variables)
            => new QueryBuilder<TType, TVariables>().With(variables);
    }

    public partial class QueryBuilder<TType, TContext>
    {
        public QueryBuilder<TType, QueryContext<TType, TVariables>> With<TVariables>(Expression<Func<QueryContext<TType>, TVariables>> variables)
        {
            if (variables is null)
                throw new NullReferenceException("Variables cannot be null");

            // check if TVariables is an anonymous type
            if (!typeof(TVariables).IsAnonymousType())
                throw new ArgumentException("Variables must be an anonymous type");

            // pull the initialization expression
            var initializations = InitializationTranslator.PullInitializationExpression(variables.Body);

            // add each as a global
            foreach (var initialization in initializations)
            {
                _queryGlobals.Add(new QueryGlobal(initialization.Key.Name, initialization.Value, variables));
            }

            return EnterNewContext<QueryContext<TType, TVariables>>();
        }

        public QueryBuilder<TType, QueryContext<TType, TVariables>> With<TVariables>(TVariables variables)
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
                    _queryVariables.Add(varName, value);
                    _queryGlobals.Add(new QueryGlobal(property.Name, new SubQuery($"<{scalarInfo}>${varName}")));
                }
                else if (property.PropertyType.IsAssignableTo(typeof(IQueryBuilder)))
                {
                    // add it as a sub-query
                    _queryGlobals.Add(new QueryGlobal(property.Name, value));
                }
                else if (
                    EdgeDBTypeUtils.IsLink(property.PropertyType, out var isMultiLink, out var innerType)
                    && !isMultiLink
                    && QueryObjectManager.TryGetObjectId(value, out var id))
                {
                    _queryGlobals.Add(new QueryGlobal(property.Name, new SubQuery($"(select {property.PropertyType.GetEdgeDBTypeName()} filter .id = <uuid>'{id}')")));
                }
                else if (ReflectionUtils.IsSubclassOfRawGeneric(typeof(JsonReferenceVariable<>), property.PropertyType))
                {
                    // serialize and add as global and variable
                    var referenceValue = property.PropertyType.GetProperty("Value")!.GetValue(value);
                    var jsonVarName = QueryUtils.GenerateRandomVariableName();
                    _queryVariables.Add(jsonVarName, DataTypes.Json.Serialize(referenceValue));
                    _queryGlobals.Add(new QueryGlobal(property.Name, new SubQuery($"<json>${jsonVarName}"), value));
                }
                else
                    throw new InvalidOperationException($"Cannot serialize {property.Name}: No serialization strategy found for {property.PropertyType}");
            }

            return EnterNewContext<QueryContext<TType, TVariables>>();
        }

        IQueryBuilder<TType, QueryContext<TType, TVariables>> IQueryBuilder<TType, TContext>.With<TVariables>(TVariables variables) => With(variables);
        IQueryBuilder<TType, QueryContext<TType, TVariables>> IQueryBuilder<TType, TContext>.With<TVariables>(Expression<Func<QueryContext<TType>, TVariables>> variables) => With(variables);
    }
}
