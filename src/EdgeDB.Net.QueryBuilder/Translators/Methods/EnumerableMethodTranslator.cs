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

        [MethodName(nameof(Enumerable.Select))]
        public void Select(QueryWriter writer, MethodCallExpression method, TranslatedParameter source, TranslatedParameter expressive, ExpressionContext context)
        {
            if (expressive.RawValue is not LambdaExpression lambda)
                throw new NotSupportedException("The expressive operand of 'Ref' must be a lambda function");

            // we handle Select(Func<X, int, Y>) differently
            if (method.Method.GetParameters()[1].ParameterType.GenericTypeArguments.Length == 3)
            {
                // we globalize the source in an `enumerate` call and then prefix any reference to `X` with GLOBAL.1
                // and any reference to the index as GLOBAL.0
                var enumerationName = QueryUtils.GenerateRandomVariableName();

                var global = context.SetGlobal(enumerationName, new SubQuery(writer => writer
                    .Function(
                        "std::enumerate",
                        source
                    )
                ), null);

                expressive.Context = expressive.Context.Enter(x =>
                {
                    x.ParameterAliases.Add(lambda.Parameters[0], Value.Of(
                            writer => writer
                                .Marker(
                                    MarkerType.GlobalReference,
                                    enumerationName,
                                    Defer.This(() => "Enumeration global reference element for Enumerable.Select translator"),
                                    new GlobalReferenceMetadata(global),
                                    Value.Of(writer => writer
                                        .Append(enumerationName, ".1.")
                                    )
                                )
                        )
                    );
                    x.ParameterAliases.Add(lambda.Parameters[1], Value.Of(
                            writer => writer
                                .Marker(
                                    MarkerType.GlobalReference,
                                    enumerationName,
                                    Defer.This(() => "Enumeration global reference index for Enumerable.Select translator"),
                                    new GlobalReferenceMetadata(global, "int64"),
                                    Value.Of(writer => writer
                                        .Append(enumerationName, ".0")
                                    )
                                )
                        )
                    );
                    x.IncludeSelfReference = true;
                });

            }
            else
            {
                expressive.Context = expressive.Context.Enter(x =>
                {
                    x.ParameterPrefixes.Add(lambda.Parameters[0], writer => writer.Append(source, '.'));
                    x.IncludeSelfReference = true;
                });
            }

            writer.Append(expressive);
        }

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
                writer.Function("std::len", source);
            else
                writer.Function("std::count", source);
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
                writer.Function("std::contains", source);
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
