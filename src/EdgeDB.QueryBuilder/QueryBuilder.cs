using EdgeDB.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EdgeDB
{ 
    public partial class QueryBuilder
    {
        internal List<QueryNode> QueryNodes = new();
        internal QueryNode CurrentNode
            => QueryNodes.LastOrDefault() ?? new QueryNode() { Type = QueryExpressionType.Start };

        public List<KeyValuePair<string, object?>> Arguments { get; set; } = new();

        public static QueryBuilder<TType> Select<TType>() 
        {
            return new QueryBuilder<TType>().Select<TType>();
        }
        public static QueryBuilder<TType> Select<TType>(params Expression<Func<TType, object?>>[] properties)
        {
            return new QueryBuilder<TType>().Select(properties);
        }

        //public static QueryBuilder Insert<TType>() { }
        //public static QueryBuilder Update<TType>() { }
        //public static QueryBuilder Delete<TType>() { }
        //public static QueryBuilder With<TType>() { }
        //public static QueryBuilder For<TType>() { }

        internal QueryBuilder<TType> BuildStrongTyped<TType>()
        {
            return new QueryBuilder<TType>(QueryNodes);
        }

        /// <summary>
        ///     Turns this query builder into a edgeql representation.
        /// </summary>
        /// <returns>A edgeql query.</returns>
        public override string ToString()
        {
            return string.Join(" ", QueryNodes.Select(x => x.Build()));
        }

        /// <summary>
        ///     Turns this query builder into a edgeql representation where each 
        ///     statement is seperated by newlines.
        /// </summary>
        /// <returns>A prettified version of the current query.</returns>
        public string ToPrettyString()
        {
            return string.Join("\n", QueryNodes);
        }
    }

    public class QueryBuilder<TType> : QueryBuilder
    {
        public QueryBuilder(List<QueryNode>? query = null)
        {
            QueryNodes = query ?? new List<QueryNode>();
        }

        public new QueryBuilder<TSelect> Select<TSelect>(params Expression<Func<TSelect, object?>>[] properties)
        {
            var props = ParsePropertySelectors(selectors: properties);
            return SelectInternal<TSelect>(props);
        }

        public QueryBuilder<TType> Select()
        {
            var props = GetTypePropertyNames(typeof(TType)).ToArray();
            return SelectInternal<TType>(props ?? Array.Empty<string>());
        }

        public new QueryBuilder<TSelect> Select<TSelect>()
        {
            var props = GetTypePropertyNames(typeof(TSelect)).ToArray();
            return SelectInternal<TSelect>(props ?? Array.Empty<string>());
        }

        internal QueryBuilder<TSelect> SelectInternal<TSelect>(IEnumerable<string> properties)
        {
            AssertValid(QueryExpressionType.Select);
            var typename = GetTypeName(typeof(TSelect));
            EnterRootNode($"select {typename} {{ {string.Join(", ", properties)} }}", QueryExpressionType.Select);
            if (typeof(TSelect) == typeof(TType))
                return (this as QueryBuilder<TSelect>)!;
            return BuildStrongTyped<TSelect>();
        }

        public QueryBuilder<TType> Filter(Expression<Func<TType, bool>> filter)
        {
            var context = new QueryContext<TType, bool>(filter);
            var builtFilter = ConvertExpression(filter.Body, context);
            Arguments.AddRange(builtFilter.Arguments);
            EnterNode($"filter {builtFilter.Filter}", QueryExpressionType.Filter);
            return this;
        }

        public QueryBuilder<TType> OrderBy(Expression<Func<TType, object?>> selector, NullPlacement? nullPlacement = null)
            => OrderByInternal("asc", selector, nullPlacement);

        public QueryBuilder<TType> OrderByDescending(Expression<Func<TType, object?>> selector, NullPlacement? nullPlacement = null)
            => OrderByInternal("desc", selector, nullPlacement);

        internal QueryBuilder<TType> OrderByInternal(string direction, Expression<Func<TType, object?>> selector, NullPlacement? nullPlacement = null)
        {
            AssertValid(QueryExpressionType.OrderBy);
            var builtSelector = ParsePropertySelectors(true, selector).FirstOrDefault();
            string orderByExp = "";
            if(CurrentNode.Type == QueryExpressionType.OrderBy)
                orderByExp += $"then {builtSelector} {direction}";
            else
                orderByExp += $"order by {builtSelector} {direction}";

            if (nullPlacement.HasValue)
                orderByExp += $" empty {nullPlacement.Value.ToString().ToLower()}";

            EnterNode(orderByExp, QueryExpressionType.OrderBy);
            return this;
        }

        public QueryBuilder<TType> Offset(ulong count)
        {
            AssertValid(QueryExpressionType.Offset);
            EnterNode($"offset {count}", QueryExpressionType.Offset);
            return this;
        }

        public QueryBuilder<TType> Limit(ulong count)
        {
            AssertValid(QueryExpressionType.Limit);
            EnterNode($"limit {count}", QueryExpressionType.Limit);
            return this;
        }

        public QueryBuilder<TType> For(Set<TType> set, Expression<Func<QueryBuilder<TType>, QueryBuilder>> iterator)
        {
            AssertValid(QueryExpressionType.For);
            var builder = new QueryBuilder<TType>();
            var builtIterator = iterator.Compile()(builder);

            EnterNode($"for {iterator.Parameters[0].Name} in {set}", QueryExpressionType.For);
            EnterNode($"union", QueryExpressionType.Union).AddChild(EnterNode(builtIterator.ToString(), builtIterator.QueryNodes.First().Type), x => $" ( {x} ) ");
            Arguments.AddRange(builtIterator.Arguments);
            return this;
        }

        public QueryBuilder<TType> Insert(TType value, params Expression<Func<TType, object?>>[] unlessConflictOn)
        {
            AssertValid(QueryExpressionType.Insert);
            var obj = SerializeQueryObject(value);
            EnterRootNode($"insert {GetTypeName(typeof(TType))} {obj.Property}", QueryExpressionType.Insert);
            Arguments.AddRange(obj.Arguments);
            if (unlessConflictOn.Any())
            {
                var props = ParsePropertySelectors(true, unlessConflictOn);

                if (props.Count > 1)
                {
                    EnterNode($"unless conflict on ({string.Join(", ", props)})", QueryExpressionType.UnlessConflictOn);
                }
                else
                    EnterNode($"unless conflict on {props[0]}", QueryExpressionType.UnlessConflictOn);
            }

            return this;
        }

        public QueryBuilder<TType> Update(TType obj, Expression<Func<TType, bool>>? filter = null)
        {
            AssertValid(QueryExpressionType.Update);
            EnterNode($"update {GetTypeName(typeof(TType))}", QueryExpressionType.Update);
            if (filter != null)
            {
                Filter(filter);
            }
            var serializedObj = SerializeQueryObject(obj);
            EnterNode($"set {serializedObj.Property}", QueryExpressionType.Set);
            Arguments.AddRange(serializedObj.Arguments);
            return this;
        }

        public QueryBuilder<TType> Update(Expression<Func<TType, TType>> builder, Expression<Func<TType, bool>>? filter = null)
        {
            AssertValid(QueryExpressionType.Update);
            var serializedObj = ConvertExpression(builder.Body, new QueryContext<TType, TType>(builder));
            EnterNode($"update {GetTypeName(typeof(TType))}", QueryExpressionType.Update);
            if (filter != null)
            {
                Filter(filter);
            }
            EnterNode($"set {{ {serializedObj.Filter} }}", QueryExpressionType.Set);
            Arguments.AddRange(serializedObj.Arguments);
            return this;
        }

        public QueryBuilder<TType> Delete()
        {
            AssertValid(QueryExpressionType.Delete);
            EnterNode($"delete {GetTypeName(typeof(TType))}", QueryExpressionType.Delete);
            return this;
        }

        public QueryBuilder<TType> With(string moduleName)
        {
            AssertValid(QueryExpressionType.With);
            EnterNode($"with module {moduleName}", QueryExpressionType.With);
            return this;
        }

        public QueryBuilder<TType> Transaction(Expression<Func<QueryBuilder<TType>, QueryBuilder>> query, IsolationMode? isolation = null,
            AccessMode? accessMode = null, bool? deferrable = null)
        {
            AssertValid(QueryExpressionType.Transaction);
            var builder = new QueryBuilder<TType>();
            var builtQuery = query.Compile()(builder);

            var transactionString = "start transaction";

            if (isolation.HasValue)
            {
                transactionString += $" isolation {ToEnumString(isolation.Value)}";
            }

            if (accessMode.HasValue)
            {
                transactionString += $" {accessMode.Value}";
            }

            if (deferrable.HasValue)
            {
                transactionString += deferrable.Value switch
                {
                    true => " deferrable",
                    false => " not deferrable"
                };
            }

            EnterNode($"{transactionString};", QueryExpressionType.Transaction);
            EnterNode($"{builtQuery};", QueryExpressionType.Start);
            EnterNode($"rollback;", QueryExpressionType.Rollback);
            return this;
        }

        public QueryBuilder<TType> StartTransaction(IsolationMode? isolation = null,
            AccessMode? accessMode = null, bool? deferrable = null)
        {
            AssertValid(QueryExpressionType.Transaction);
            List<string> transactionArgs = new();

            if (isolation.HasValue)
            {
                transactionArgs.Add($"isolation {ToEnumString(isolation.Value)}");
            }

            if (accessMode.HasValue)
            {
                transactionArgs.Add($"{ToEnumString(accessMode.Value)}");
            }

            if (deferrable.HasValue)
            {
                transactionArgs.Add($"{(deferrable.Value ? "deferrable" : "not deferrable")}");
            }

            EnterNode($"start transaction{(transactionArgs.Any() ? $" {string.Join(", ", transactionArgs)}" : "")};", QueryExpressionType.Transaction);
            return this;
        }

        public QueryBuilder<TType> EndTransaction()
        {
            EnterNode("rollback;", QueryExpressionType.Rollback);
            return this;
        }

        public QueryBuilder<TType> Commit()
        {

            EnterNode("commit;", QueryExpressionType.Commit);
            return this;
        }

        //public QueryBuilder<TType> DeclareSavepoint(string name)
        //{

        //}

        private QueryNode EnterRootNode(string q, QueryExpressionType nodeType)
        {
            var node = new QueryNode
            {
                Query = q,
                Type = nodeType
            };
            QueryNodes.Add(node);
            return node;
        }

        private QueryNode EnterNode(string q, QueryExpressionType nodeType)
        {
            var node = new QueryNode
            {
                Query = q,
                Type = nodeType
            };
            CurrentNode.AddChild(node, (x) => x);
            return node;
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
            if (!_validExpressions[currentExpression].Contains(CurrentNode.Type))
            {
                throw new InvalidQueryOperationException(currentExpression, _validExpressions[currentExpression]);
            }
        }

        private Dictionary<QueryExpressionType, QueryExpressionType[]> _validExpressions = new()
        {
            { QueryExpressionType.With, new QueryExpressionType[] { QueryExpressionType.With} },
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

        public IEnumerable<TType> BuildSubQuery()
            => (Set<TType>)this;
    }

    public class QueryNode
    {
        public QueryExpressionType Type { get; set; }
        public List<QueryNode> Children { get; set; } = new();
        public QueryNode? Parent { get; set; }
        public string? Query { get; set; }
        internal Func<string, string>? BuildFunc { get; set; }
        

        public void AddChild(QueryNode node, Func<string, string> buildFunc)
        {
            node.BuildFunc = buildFunc;
            node.Parent = this;
            Children.Add(node);
        }

        public string Build()
        {
            return $"{(BuildFunc != null ? BuildFunc(Query!) : Query)} {(Children.Any() ? string.Join(" ", Children.Select(x => x.Build())) : "")}";
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
