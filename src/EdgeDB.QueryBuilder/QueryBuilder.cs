using EdgeDB.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EdgeDB
{ 
    public partial class QueryBuilder
    {
        internal List<QueryNode> QueryNodes = new();

        internal QueryExpressionType PreviousNodeType
            => CurrentRootNode.Children.LastOrDefault().Type;

        internal QueryNode CurrentRootNode
            => QueryNodes.LastOrDefault() ?? new QueryNode() { Type = QueryExpressionType.Start };

        public List<KeyValuePair<string, object?>> Arguments { get; set; } = new();

        #region Static keyword proxies
        public static QueryBuilder<TResult> Select<TResult>(Expression<Func<TResult>> selector)
        {
            return new QueryBuilder<TResult>().Select(selector);   
        }
        public static QueryBuilder<TType> Select<TType>() 
        {
            return new QueryBuilder<TType>().Select<TType>();
        }
        public static QueryBuilder<TType> Select<TType>(params Expression<Func<TType, object?>>[] properties)
        {
            return new QueryBuilder<TType>().Select(properties);
        }

        public static QueryBuilder<TType> Insert<TType>(TType value, params Expression<Func<TType, object?>>[] unlessConflictOn) 
        {
            return new QueryBuilder<TType>().Insert(value, unlessConflictOn);
        }
        public static QueryBuilder<TType> Update<TType>(TType obj) 
        {
            return new QueryBuilder<TType>().Update(obj);
        }
        public static QueryBuilder<TType> Update<TType>(Expression<Func<TType, TType>> builder)
        {
            return new QueryBuilder<TType>().Update(builder);
        }

        public static QueryBuilder<TType> Delete<TType>() 
        {
            return new QueryBuilder<TType>().Delete();
        }
        public static QueryBuilder<TType> With<TType>(string moduleName) 
        {
            return new QueryBuilder<TType>().With(moduleName);
        }
        public static QueryBuilder<TType> With<TType>(string name, TType value)
        {
            return new QueryBuilder<TType>().With(name, value);
        }
        public static QueryBuilder<object> With(params (string Name, object? Value)[] variables)
        {
            return new QueryBuilder<object>().With(variables);
        }

        public static QueryBuilder<TType> For<TType>(Set<TType> set, Expression<Func<QueryBuilder<TType>, QueryBuilder>> iterator) 
        {
            return new QueryBuilder<TType>().For(set, iterator);
        }

        #endregion

        /// <summary>
        ///     Turns this query builder into a edgeql representation.
        /// </summary>
        /// <returns>A edgeql query.</returns>
        public override string? ToString()
        {
            return Build().QueryText;
        }

        /// <summary>
        ///     Turns this query builder into a edgeql representation where each 
        ///     statement is seperated by newlines.
        /// </summary>
        /// <returns>A prettified version of the current query.</returns>
        public string ToPrettyString()
        {
            return Build().Prettify();
        }

        public BuiltQuery Build()
        {
            return Build(new());
        }

        internal BuiltQuery Build(QueryBuilderContext config)
        {
            var results = QueryNodes.Select(x => x.Build(config)).ToArray();
            return new BuiltQuery
            {
                Parameters = results.SelectMany(x => x.Parameters),
                QueryText = string.Join(" ", results.Select(x => x.QueryText))
            };
        }
    }

    public class QueryBuilder<TType> : QueryBuilder
    {
        public QueryBuilder() : this(null) { }
        internal QueryBuilder(List<QueryNode>? query = null)
        {
            QueryNodes = query ?? new List<QueryNode>();
        }



        internal QueryBuilder<TTarget> ConvertTo<TTarget>()
        {
            if (typeof(TTarget) == typeof(TType))
                return (this as QueryBuilder<TTarget>)!;
            return new QueryBuilder<TTarget>(QueryNodes);
        }

        public new QueryBuilder<TTarget> Select<TTarget>(Expression<Func<TTarget>> selector)
        {
            EnterRootNode(QueryExpressionType.Select, (QueryNode node, ref QueryBuilderContext context) =>
            {
                var query = ConvertExpression(selector.Body, new QueryContext<TTarget>(selector));
                node.Query = $"select {query.Filter}";
                node.AddArguments(query.Arguments);
            });

            if (typeof(TTarget) == typeof(TType))
                return (this as QueryBuilder<TTarget>)!;
            return ConvertTo<TTarget>();
        }

        public new QueryBuilder<TTarget> Select<TTarget>(params Expression<Func<TTarget, object?>>[] properties)
        {
            return SelectInternal<TTarget>(context =>
            {
                if (context.DontSelectProperties)
                    return null;

                return (ParsePropertySelectors(selectors: properties), null);
            });
        }

        public QueryBuilder<TType> Select()
        {
            return Select<TType>();
        }

        public new QueryBuilder<TTarget> Select<TTarget>()
        {
            return SelectInternal<TTarget>(context =>
            {
                if (context.DontSelectProperties)
                {
                    return null;
                }
                var result = GetTypePropertyNames(typeof(TTarget));

                return (result.Properties.ToArray() ?? Array.Empty<string>(), result.Arguments);
            });
        }

        internal QueryBuilder<TTarget> SelectInternal<TTarget>(Func<QueryBuilderContext, (IEnumerable<string>? Properties, IEnumerable<KeyValuePair<string, object?>>? Arguments)?> argumentBuilder)
        {
            EnterRootNode(QueryExpressionType.Select, (QueryNode node, ref QueryBuilderContext context) =>
            {
                var selectArgs = argumentBuilder(context);

                IEnumerable<string>? properties = selectArgs?.Properties;
                IEnumerable<KeyValuePair<string, object?>>? args = selectArgs?.Arguments;

                node.Query = $"select {(context.UseDetachedSelects ? "detached " : "")}{GetTypeName(typeof(TTarget))}{(properties != null ? $" {{ {string.Join(", ", properties)} }}" : "")}";
                if (context.UseDetachedSelects && PreviousNodeType != QueryExpressionType.Limit)
                    node.AddChild(QueryExpressionType.Limit, (ref QueryBuilderContext _) => new BuiltQuery { QueryText = "limit 1" });
                
                if (args != null)
                    node.AddArguments(args);
            });
            return ConvertTo<TTarget>();
        }


        public QueryBuilder<TType> Filter(Expression<Func<TType, bool>> filter)
            => Filter<TType>(filter);
        public QueryBuilder<TTarget> Filter<TTarget>(Expression<Func<TTarget, bool>> filter)
        {
            EnterNode(QueryExpressionType.Filter, (ref QueryBuilderContext builderContext) =>
            {
                var context = new QueryContext<TTarget, bool>(filter);
                var builtFilter = ConvertExpression(filter.Body, context);

                return new BuiltQuery
                {
                    Parameters = builtFilter.Arguments,
                    QueryText = $"filter {builtFilter.Filter}"
                };
            });
            return ConvertTo<TTarget>();
        }

        public QueryBuilder<TType> OrderBy(Expression<Func<TType, object?>> selector, NullPlacement? nullPlacement = null)
            => OrderByInternal("asc", selector, nullPlacement);

        public QueryBuilder<TType> OrderByDescending(Expression<Func<TType, object?>> selector, NullPlacement? nullPlacement = null)
            => OrderByInternal("desc", selector, nullPlacement);

        internal QueryBuilder<TType> OrderByInternal(string direction, Expression<Func<TType, object?>> selector, NullPlacement? nullPlacement = null)
        {
            AssertValid(QueryExpressionType.OrderBy);
            EnterNode(QueryExpressionType .OrderBy, (ref QueryBuilderContext context) =>
            {
                var builtSelector = ParsePropertySelectors(true, selector).FirstOrDefault();
                string orderByExp = "";
                if (CurrentRootNode.Type == QueryExpressionType.OrderBy)
                    orderByExp += $"then {builtSelector} {direction}";
                else
                    orderByExp += $"order by {builtSelector} {direction}";

                if (nullPlacement.HasValue)
                    orderByExp += $" empty {nullPlacement.Value.ToString().ToLower()}";

                return new BuiltQuery
                {
                    QueryText = orderByExp
                };
            });
            return this;
        }

        public QueryBuilder<TType> Offset(ulong count)
        {
            AssertValid(QueryExpressionType.Offset);
            EnterNode(QueryExpressionType .Offset, (ref QueryBuilderContext context) =>
            {
                return new BuiltQuery
                {
                    QueryText = $"offset {count}"
                };
            }); return this;
        }

        public QueryBuilder<TType> Limit(ulong count)
        {
            AssertValid(QueryExpressionType.Limit);
            EnterNode(QueryExpressionType.Limit, (ref QueryBuilderContext context) =>
            {
                return new BuiltQuery
                {
                    QueryText = $"limit {count}"
                };
            });
            return this;
        }

        public QueryBuilder<TType> For(Set<TType> set, Expression<Func<QueryBuilder<TType>, QueryBuilder>> iterator)
        {
            EnterRootNode(QueryExpressionType.For, (QueryNode node, ref QueryBuilderContext context) =>
            {
                var builder = new QueryBuilder<TType>();
                var builtIterator = iterator.Compile()(builder);

                node.Query = $"for {iterator.Parameters[0].Name} in {GetTypeName(typeof(TType))}";
                node.AddChild(QueryExpressionType.Union, (ref QueryBuilderContext innerContext) =>
                {
                    var result = builtIterator.Build(innerContext);
                    result.QueryText = $"union ({result.QueryText})";
                    return result;
                });
            });
            return this;
        }

        public new QueryBuilder<TTarget> Insert<TTarget>(TTarget value, params Expression<Func<TTarget, object?>>[] unlessConflictOn)
        {
            EnterRootNode(QueryExpressionType.Insert, (QueryNode node, ref QueryBuilderContext context) =>
            {
                context.DontSelectProperties = true;
                var obj = SerializeQueryObject(value, context);
                node.Query = $"insert {GetTypeName(typeof(TTarget))} {obj.Property}";
                node.AddArguments(obj.Arguments);

                if (unlessConflictOn.Any())
                {
                    var props = ParsePropertySelectors(true, unlessConflictOn);
                    node.AddChild(QueryExpressionType.UnlessConflictOn, (ref QueryBuilderContext innerContext) =>
                    {
                        return new BuiltQuery
                        {
                            QueryText = props.Count > 1
                                ? $"unless conflict on ({string.Join(", ", props)})"
                                : $"unless conflict on {props[0]}"
                        };
                    });
                }
            });

            return ConvertTo<TTarget>();
        }

        public QueryBuilder<TType> UnlessConflictOn(params Expression<Func<TType, object?>>[] selectors)
        {
            EnterNode(QueryExpressionType.UnlessConflictOn, (ref QueryBuilderContext innerContext) =>
            {
                var props = ParsePropertySelectors(true, selectors);

                return new BuiltQuery
                {
                    QueryText = props.Count > 1
                        ? $"unless conflict on ({string.Join(", ", props)})"
                        : $"unless conflict on {props[0]}"
                };
            });

            return this;
        }

        public new QueryBuilder<TTarget> Update<TTarget>(TTarget obj)
        {
            EnterRootNode(QueryExpressionType.Update, (QueryNode node, ref QueryBuilderContext context) =>
            {
                node.Query = $"update {GetTypeName(typeof(TTarget))}";
                var serializedObj = SerializeQueryObject(obj);
                node.AddChild(QueryExpressionType.Set, (ref QueryBuilderContext innerContext) =>
                {
                    return new BuiltQuery
                    {
                        QueryText = $"set {serializedObj.Property}",
                        Parameters = serializedObj.Arguments,
                    };
                });
            });
            return ConvertTo<TTarget>();
        }

        public new QueryBuilder<TTarget> Update<TTarget>(Expression<Func<TTarget, TTarget>> builder)
        {
            EnterRootNode(QueryExpressionType.Update, (QueryNode node, ref QueryBuilderContext context) =>
            {
                var serializedObj = ConvertExpression(builder.Body, new QueryContext<TTarget, TTarget>(builder) { AllowStaticOperators = true });
                
                node.Query = $"update {GetTypeName(typeof(TTarget))}";
                node.AddChild(QueryExpressionType.Set, (ref QueryBuilderContext innerContext) =>
                {
                    return new BuiltQuery
                    {
                        QueryText = $"set {{ {serializedObj.Filter} }}",
                        Parameters = serializedObj.Arguments
                    };
                });
            });

            return ConvertTo<TTarget>();
        }

        public QueryBuilder<TType> Delete()
        {
            EnterRootNode(QueryExpressionType.Delete, (QueryNode node, ref QueryBuilderContext context) =>
            {
                node.Query = $"delete {GetTypeName(typeof(TType))}";
            });
            return this;
        }

        public QueryBuilder<TType> With(string moduleName)
        {
            EnterRootNode(QueryExpressionType.With, (QueryNode node, ref QueryBuilderContext context) =>
            {
                node.Query = $"with module {moduleName}";
            });
            return this;
        }

        public new QueryBuilder<TType> With(params (string Name, object? Value)[] variables)
        {
            EnterRootNode(QueryExpressionType.With, (QueryNode node, ref QueryBuilderContext context) =>
            {
                List<string> statements = new();

                context.IntrospectObjectIds = true;
                foreach (var item in variables)
                {
                    var converted = SerializeProperty(item.Value?.GetType() ?? typeof(object), item.Value, false, context);
                    node.AddArguments(converted.Arguments);

                    statements.Add($"{item.Name} := {converted.Property}");
                }

                node.Query = $"with {string.Join(", ", statements)}";
            });

            return this;
        }

        public new QueryBuilder<TTarget> With<TTarget>(string name, TTarget value)
        {
            if(PreviousNodeType == QueryExpressionType.With)
            {
                EnterNode(QueryExpressionType.With, (ref QueryBuilderContext context) =>
                {
                    context.IntrospectObjectIds = true;
                    var converted = SerializeProperty(value, false, context);

                    return new BuiltQuery
                    {
                        QueryText = $", {name} := {converted.Property}",
                        Parameters = converted.Arguments
                    };
                });
            }
            else
            {
                EnterRootNode(QueryExpressionType.With, (QueryNode node, ref QueryBuilderContext context) =>
                {
                    context.IntrospectObjectIds = true;
                    var converted = SerializeProperty(value, false, context);
                    node.AddArguments(converted.Arguments);
                    node.Query = $"with {name} := {converted.Property}";
                });
            }

            return ConvertTo<TTarget>();
        }

        private QueryNode EnterRootNode(QueryExpressionType type, RootNodeBuilder builder)
        {
            AssertValid(type);
            var node = new QueryNode(type, builder);
            QueryNodes.Add(node);
            return node;
        }

        private void EnterNode(QueryExpressionType type, ChildNodeBuilder builder)
        {
            CurrentRootNode.AddChild(type, builder);
        }

        private string ToEnumString<TEnum>(TEnum value) where TEnum : notnull
        {
            return Regex.Replace(value.ToString()!, @"(.[A-Z])", m =>
            {
                return $"{m.Groups[1].Value[0]} {m.Groups[1].Value[1]}";
            }).ToLower();
        }

        private void AssertValid(QueryExpressionType currentExpression)
        {
            if (!_validExpressions[currentExpression].Contains(CurrentRootNode.Type))
            {
                throw new InvalidQueryOperationException(currentExpression, _validExpressions[currentExpression]);
            }
        }

        private Dictionary<QueryExpressionType, QueryExpressionType[]> _validExpressions = new()
        {
            { QueryExpressionType.With, new QueryExpressionType[] { QueryExpressionType.With, QueryExpressionType.Start } },
            { QueryExpressionType.Select, new QueryExpressionType[] { QueryExpressionType.With, QueryExpressionType.Start} },
            { QueryExpressionType.OrderBy, new QueryExpressionType[] { QueryExpressionType.Delete, QueryExpressionType.Filter, QueryExpressionType.Select} },
            { QueryExpressionType.Offset, new QueryExpressionType[] { QueryExpressionType.Delete, QueryExpressionType.OrderBy, QueryExpressionType.Select, QueryExpressionType.Filter } },
            { QueryExpressionType.Limit, new QueryExpressionType[] { QueryExpressionType.Delete, QueryExpressionType.OrderBy, QueryExpressionType.Select, QueryExpressionType.Filter, QueryExpressionType.Offset } },
            { QueryExpressionType.For, new QueryExpressionType[] {  QueryExpressionType.With, QueryExpressionType.Start} },
            { QueryExpressionType.Insert, new QueryExpressionType[] { QueryExpressionType.With, QueryExpressionType.Start} },
            { QueryExpressionType.Update, new QueryExpressionType[] { QueryExpressionType.With, QueryExpressionType.Start} },
            { QueryExpressionType.Delete, new QueryExpressionType[] { QueryExpressionType.With, QueryExpressionType.Start} },
            { QueryExpressionType.Transaction, new QueryExpressionType[] { QueryExpressionType.Start} }
            
        };

        public static implicit operator Set<TType>(QueryBuilder<TType> v) => new Set<TType>(v.ToString(), v.Arguments.ToDictionary(x => x.Key, x => x.Value));
        public static implicit operator ComputedValue<TType>(QueryBuilder<TType> v) => new ComputedValue<TType>(default, v);
        
        public Set<TType> SubQuerySet()
            => (Set<TType>)this;

        public TType SubQuery()
        {
            var obj = (ISubQueryType)Activator.CreateInstance(CreateMockedType(typeof(TType)))!;
            obj.Builder = this;
            return (TType)obj;
        }
    }

    internal delegate BuiltQuery ChildNodeBuilder(ref QueryBuilderContext context);
    internal delegate void RootNodeBuilder(QueryNode node, ref QueryBuilderContext context);


    internal class QueryNode
    {
        public QueryExpressionType Type { get; set; }
        public List<(ChildNodeBuilder Builder, QueryExpressionType Type)> Children { get; set; } = new();
        public QueryNode? Parent { get; set; }
        public string? Query { get; set; }
        public IEnumerable<KeyValuePair<string, object?>> Arguments { get; set; } = new Dictionary<string, object?>();

        private readonly object _lock = new();

        private readonly RootNodeBuilder? _builder;

        public QueryNode() { }

        public QueryNode(QueryExpressionType type, RootNodeBuilder builder)
        {
            _builder = builder;
            Type = type;
        }

        public void AddChild(QueryExpressionType type, ChildNodeBuilder builder)
            => Children.Add((builder, type));

        public void AddArguments(IEnumerable<KeyValuePair<string, object?>> args)
        {
            lock (_lock)
            {
                Arguments = Arguments.Concat(args);
            }
        }

        public BuiltQuery Build(QueryBuilderContext config)
        {
            // remove current arguments incase of building twice
            Arguments = new Dictionary<string, object?>();
            if(_builder != null)
                _builder.Invoke(this, ref config);

            var result = $"{Query}";

            if (Children.Any())
            {
                var results = Children.Select(x => x.Builder.Invoke(ref config)).ToArray();
                result += $" {string.Join(" ", results.Select(x => x.QueryText))}";
                AddArguments(results.SelectMany(x => x.Parameters));
            }

            return new BuiltQuery
            {
                Parameters = Arguments,
                QueryText = result
            };
        }
    }

    public enum QueryExpressionType 
    { 
        Start,
        Select,
        Insert,
        Update,
        Delete,
        With,
        For,
        Filter,
        OrderBy,
        Offset,
        Limit,
        Set,
        Transaction,
        Union,
        UnlessConflictOn,
        Rollback,
        Commit
    }

    public enum IsolationMode
    {
        /// <summary>
        ///     All statements of the current transaction can only see data changes committed before the first query 
        ///     or data-modification statement was executed in this transaction. If a pattern of reads and writes among
        ///     concurrent serializable transactions would create a situation which could not have occurred for any serial
        ///     (one-at-a-time) execution of those transactions, one of them will be rolled back with a serialization_failure error.
        /// </summary>
        Serializable,
        /// <summary>
        ///     All statements of the current transaction can only see data committed before the first query or data-modification 
        ///     statement was executed in this transaction.
        /// </summary>
        /// <remarks>
        ///     This is the default isolation mode.
        /// </remarks>
        RepeatableRead
    }

    public enum AccessMode
    {
        /// <summary>
        ///     Sets the transaction access mode to read/write.
        /// </summary>
        /// <remarks>
        ///     This is the default transaction access mode.
        /// </remarks>
        ReadWrite,

        /// <summary>
        ///     Sets the transaction access mode to read-only. Any data modifications with insert, update, or delete
        ///     are disallowed. Schema mutations via DDL are also disallowed.
        /// </summary>
        ReadOnly
    }

    public enum NullPlacement
    {
        First,
        Last,
    }
}
