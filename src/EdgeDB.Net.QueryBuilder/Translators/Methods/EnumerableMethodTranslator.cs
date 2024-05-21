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
        public override Type TranslatorTargetType => typeof(Enumerable);

        [MethodName(nameof(Enumerable.All))]
        public void All(QueryWriter writer, TranslatedParameter source, TranslatedParameter func)
        {
            if (func.RawValue is not LambdaExpression lambda)
                throw new ArgumentException("Expected a lambda function", nameof(func));

            // any reference to the source is replaced with the source itself.
            func.Context.ParameterAliases[lambda.Parameters[0]] = source;

            writer.Function(
                "all",
                Defer.This(() => "Enumerable.All translation"),
                new FunctionMetadata("std::all"),
                func
            );
        }

        [MethodName(nameof(Enumerable.Any))]
        public void Any(QueryWriter writer, TranslatedParameter source, TranslatedParameter? func)
        {
            if (func is null)
            {
                writer.Append("exists ", source);
                return;
            }

            if (func.RawValue is not LambdaExpression lambda)
                throw new ArgumentException("Expected a lambda function", nameof(func));

            // any reference to the source is replaced with the source itself.
            func.Context.ParameterAliases[lambda.Parameters[0]] = source;

            writer.Function(
                "any",
                Defer.This(() => "Enumerable.Any translation"),
                new FunctionMetadata("std::any"),
                func
            );
        }

        [MethodName(nameof(Enumerable.Append))]
        public void Append(QueryWriter writer, TranslatedParameter source, TranslatedParameter other)
            => writer.Append(source, " union ", other);

        [MethodName(nameof(Enumerable.AsEnumerable))]
        public void AsEnumerable(QueryWriter writer, TranslatedParameter source)
        {} // TODO: maybe do array -> set?

        [MethodName(nameof(Enumerable.Average))]
        public void Average(QueryWriter writer, TranslatedParameter source, TranslatedParameter? func)
        {
            if (func is null)
            {
                writer.Function(
                    "mean",
                    Defer.This(() => "Enumerable.Average translation"),
                    new FunctionMetadata(
                        "std::mean",
                        Info.OfMethod<EdgeQL>(nameof(EdgeQL.Mean))
                    ),
                    source
                );
                return;
            }

            if (func.RawValue is not LambdaExpression lambda)
                throw new ArgumentException("Expected a lambda function", nameof(func));

            // any reference to the source is replaced with the source itself.
            func.Context.ParameterAliases[lambda.Parameters[0]] = source;

            writer.Function(
                "mean",
                Defer.This(() => "Enumerable.Average translation"),
                new FunctionMetadata(
                    "std::mean",
                    Info.OfMethod<EdgeQL>(nameof(EdgeQL.Mean))
                ),
                func
            );
        }

        [MethodName(nameof(Enumerable.Cast))]
        public void Cast(QueryWriter writer, MethodCallExpression method, TranslatedParameter source)
        {
            var type = method.Method.GetGenericArguments()[0];

            if (!EdgeDBTypeUtils.TryGetScalarType(type, out var info))
                throw new ArgumentException($"No scalar type information found for {type}", nameof(method));

            writer.TypeCast(info.ToString()).Append(source);
        }

        [MethodName(nameof(Enumerable.Concat))]
        public void Concat(QueryWriter writer, TranslatedParameter source, TranslatedParameter other)
            => writer.Append(source, " ++ ", other);

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
        ///     Translates the method <see cref="Enumerable.Count{TSource}(IEnumerable{TSource})"/>.
        /// </summary>
        /// <param name="writer">The query string writer to append the translated method to.</param>
        /// <param name="source">The source collection to count.</param>
        /// <param name="mapper">The mapping function of what to count.</param>
        /// <returns>The EdgeQL equivalent of the method.</returns>
        [MethodName(nameof(Enumerable.Count))]
        public void Count(QueryWriter writer, TranslatedParameter source, TranslatedParameter? mapper)
        {
            if (mapper is null)
            {
                if (source.IsScalarArrayType || source.IsScalarType)
                    writer.Function(
                        "len",
                        Defer.This(() => "Scalar length of Enumerable.Count"),
                        new FunctionMetadata(
                            "std::len",
                            Info.OfMethod<EdgeQL>(nameof(EdgeQL.Len))
                        ),
                        source
                    );
                else
                    writer.Function(
                        "count",
                        Defer.This(() => "count from Enumerable.Count"),
                        new FunctionMetadata(
                            "std::count",
                            Info.OfMethod<EdgeQL>(nameof(EdgeQL.Count))
                        ),
                        source
                    );

                return;
            }

            if (mapper.RawValue is not LambdaExpression lambda)
                throw new ArgumentException("Expected a lambda function", nameof(mapper));

            // any reference to the source is replaced with the source itself.
            mapper.Context.ParameterAliases[lambda.Parameters[0]] = source;

            writer.Function(
                "count",
                Defer.This(() => "Enumerable.Count mapping translation"),
                new FunctionMetadata(
                    "std::count",
                    Info.OfMethod<EdgeQL>(nameof(EdgeQL.Count))
                ),
                mapper
            );
        }

        [MethodName(nameof(Enumerable.DefaultIfEmpty))]
        public void DefaultIfEmpty(QueryWriter writer, TranslatedParameter source, TranslatedParameter? defaultParam)
        {
            if (defaultParam is null)
                writer.Append(source);
            else
                writer.Append(source, " ?? ", defaultParam);
        }

        [MethodName(nameof(Enumerable.Distinct))]
        public void Distinct(QueryWriter writer, TranslatedParameter source)
        {
            writer.Append("distinct ", source);
        }

        /// <summary>
        ///     Translates the method <see cref="Enumerable.ElementAt{TSource}(IEnumerable{TSource}, Index)"/>.
        /// </summary>
        /// <param name="writer">The query string writer to append the translated method to.</param>
        /// <param name="source">The source collection.</param>
        /// <param name="index">The index of the element to get</param>
        /// <returns>The EdgeQL equivalent of the method.</returns>
        [MethodName(nameof(Enumerable.ElementAt))]
        [MethodName(nameof(Enumerable.ElementAtOrDefault))]
        public void ElementAt(QueryWriter writer, TranslatedParameter source, TranslatedParameter index)
        {
            // TODO: figure out set detection, since array_get doesn't work on sets.
            writer.Function("array_get", source, index);
        }

        [MethodName(nameof(Enumerable.Empty))]
        public void Empty(QueryWriter writer, MethodCallExpression method)
        {
            if (!EdgeDBTypeUtils.TryGetScalarType(method.Method.GetGenericArguments()[0], out var info))
                throw new ArgumentException("Expected scalar type", nameof(method));

            writer.TypeCast(info.ToString()).Append("{}");
        }

        [MethodName(nameof(Enumerable.Except))]
        public void Except(QueryWriter writer, TranslatedParameter source, TranslatedParameter other)
        {
            writer.Append(source, " except ", other);
        }

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
                                    new GlobalMetadata(global),
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
                                    new GlobalMetadata(global, "int64"),
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
