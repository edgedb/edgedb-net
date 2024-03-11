using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Translators.Methods
{
    /// <summary>
    ///     Represents a translator for translating methods within
    ///     the <see cref="Enumerable"/> class.
    /// </summary>
    internal class EnumerableMethodTranslator : MethodTranslator
    {
        /// <inheritdoc/>
        protected override Type TranslatorTargetType => typeof(Enumerable);

        /// <summary>
        ///     Translates the method <see cref="Enumerable.Count{TSource}(IEnumerable{TSource})"/>.
        /// </summary>
        /// <param name="writer">The query string writer to append the translated method to.</param>
        /// <param name="source">The source collection to count.</param>
        /// <returns>The EdgeQL equivalent of the method.</returns>
        [MethodName(nameof(Enumerable.Count))]
        public void Count(QueryWriter writer, TranslatedParameter source)
        {
            if (source.IsScalarArrayType || source.IsScalarType)
                writer.Function("len", source);
            else
                writer.Function("count", source);
        }

        /// <summary>
        ///     Translates the method <see cref="Enumerable.Contains{TSource}(IEnumerable{TSource}, TSource)"/>.
        /// </summary>
        /// <param name="writer">The query string writer to append the translated method to.</param>
        /// <param name="source">The source collection.</param>
        /// <param name="target">The value to locate within the collection.</param>
        /// <returns>The EdgeQL equivalent of the method.</returns>
        [MethodName(nameof(Enumerable.Contains))]
        public void Contains(QueryWriter writer, TranslatedParameter source, TranslatedParameter target)
        {
            if (source.IsScalarArrayType || source.IsScalarType)
                writer.Function("contains", source);
            else
                writer.Append(target).Append(" in ").Append(source);
        }

        /// <summary>
        ///     Translates the method <see cref="Enumerable.ElementAt{TSource}(IEnumerable{TSource}, Index)"/>.
        /// </summary>
        /// <param name="writer">The query string writer to append the translated method to.</param>
        /// <param name="source">The source collection.</param>
        /// <param name="index">The index of the element to get</param>
        /// <returns>The EdgeQL equivalent of the method.</returns>
        [MethodName(nameof(Enumerable.ElementAt))]
        public void ElementAt(QueryWriter writer, TranslatedParameter source, TranslatedParameter index)
            => writer.Function("array_get", source, index);

        /// <summary>
        ///     Translates the method <see cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource})"/>.
        /// </summary>
        /// <param name="writer">The query string writer to append the translated method to.</param>
        /// <param name="source">The source collection.</param>
        /// <param name="filterOrDefault">The default value or expression.</param>
        /// <param name="defaultValue">The default value if the <paramref name="filterOrDefault"/> was a filter.</param>
        /// <returns>The EdgeQL equivalent of the method.</returns>
        [MethodName(nameof(Enumerable.FirstOrDefault))]
        public void FirstOrDefault(QueryWriter writer, TranslatedParameter source, TranslatedParameter filterOrDefault, TranslatedParameter? defaultValue)
        {
            if (filterOrDefault.IsScalarType)
            {
                ElementAt(
                    writer,
                    source,
                    new TranslatedParameter(
                        typeof(long),
                        Expression.Constant(0L),
                        source.Context
                    )
                );
                writer
                    .Append(" ?? ")
                    .Append(filterOrDefault);

                return;
            }

            var name = ((LambdaExpression)filterOrDefault.RawValue).Parameters[0].Name;
            var set = source.IsScalarArrayType ? $"array_unpack({source})" : source.ToString();
            var returnType = ((LambdaExpression)filterOrDefault.RawValue).Parameters[0].Type;

            writer
                .TypeCast(EdgeDBTypeUtils.GetEdgeDBScalarOrTypeName(returnType))
                .Function(
                    "array_get",
                    Value.Of(writer => writer
                        .Function(
                            "array_agg",
                            Value.Of(writer => writer
                                .Wrapped(writer => writer
                                    .Append("select ")
                                    .Assignment(name!, set)
                                    .Append(" filter ")
                                    .Append(filterOrDefault)
                                )
                            )
                        )
                    ),
                    "0"
                );

            if (defaultValue is not null)
                writer
                    .Append(" ?? ")
                    .Append(defaultValue);
        }
    }
}
