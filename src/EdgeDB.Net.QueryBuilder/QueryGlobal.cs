using EdgeDB.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents a globally defined variables contained within a 'WITH' statement.
    /// </summary>
    internal class QueryGlobal
    {
        /// <summary>
        ///     Gets the value that was included in the 'WITH' statement.
        /// </summary>
        public object? Value { get; init; }

        /// <summary>
        ///     Gets the object reference that the <see cref="Value"/> represents.
        ///     For example the following code
        ///     <code>
        ///     QueryBuilder.Insert(new Person {..., Friend = new Person {...}})
        ///     </code>
        ///     would cause the nested person object to be converted to a <see cref="QueryGlobal"/>
        ///     and this property would be the actual reference to that person instance.
        /// </summary>
        public object? Reference { get; init; }

        /// <summary>
        ///     Gets the name of the global.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        ///     Constructs a new <see cref="QueryGlobal"/>.
        /// </summary>
        /// <param name="name">The name of the global.</param>
        /// <param name="value">The value which will be the assignment of this global.</param>
        public QueryGlobal(string name, object? value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        ///     Constructs a new <see cref="QueryGlobal"/>.
        /// </summary>
        /// <param name="name">The name of the global.</param>
        /// <param name="value">The value which will be the assignment of this global.</param>
        /// <param name="reference">The refrence object that caused this global to be created.</param>
        public QueryGlobal(string name, object? value, object? reference)
        {
            Name = name;
            Value = value;
            Reference = reference;
        }

        public Value[] Compile(IQueryBuilder source, QueryWriter writer, CompileContext? context = null, SchemaInfo? info = null)
        {
            return Value switch
            {
                // if its a query builder, build it and add it as a sub-query.
                IQueryBuilder queryBuilder => writer.Span(writer =>
                    writer.Wrapped(writer => queryBuilder.WriteTo(writer, source))),
                // if its a sub query that requires introspection, build it and add it.
                SubQuery {RequiresIntrospection: true} when info is null => throw new InvalidOperationException(
                    "Cannot build without introspection! A node requires query introspection."),
                SubQuery {RequiresIntrospection: true} subQuery => writer.Span(writer => subQuery.Build(info, writer)),
                _ => writer.Span(writer => QueryUtils.ParseObject(writer, Value))
            };
        }
    }
}
