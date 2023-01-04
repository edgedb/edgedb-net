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
        protected override Type TransaltorTargetType => typeof(Enumerable);

        /// <summary>
        ///     Translates the method <see cref="Enumerable.Count{TSource}(IEnumerable{TSource})"/>.
        /// </summary>
        /// <param name="source">The source collection to count.</param>
        /// <returns>The EdgeQL equivalent of the method.</returns>
        [MethodName(nameof(Enumerable.Count))]
        public string Count(TranslatedParameter source)
            => (source.IsScalarArrayType || source.IsScalarType) ? $"len({source})" : $"count({source})";

        /// <summary>
        ///     Translates the method <see cref="Enumerable.Contains{TSource}(IEnumerable{TSource}, TSource)"/>.
        /// </summary>
        /// <param name="source">The source collection.</param>
        /// <param name="target">The value to locate within the collection.</param>
        /// <returns>The EdgeQL equivalent of the method.</returns>
        [MethodName(nameof(Enumerable.Contains))]
        public string Contains(TranslatedParameter source, string target)
            => (source.IsScalarArrayType || source.IsScalarType) ? $"contains({source}, {target})" : $"{target} in {source}";

        /// <summary>
        ///     Translates the method <see cref="Enumerable.ElementAt{TSource}(IEnumerable{TSource}, Index)"/>.
        /// </summary>
        /// <param name="source">The source collection.</param>
        /// <param name="index">The index of the element to get</param>
        /// <returns>The EdgeQL equivalent of the method.</returns>
        [MethodName(nameof(Enumerable.ElementAt))]
        public string ElementAt(TranslatedParameter source, string index)
            => $"array_get({source}, {index})";

        /// <summary>
        ///     Translates the method <see cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource})"/>.
        /// </summary>
        /// <param name="source">The source collection.</param>
        /// <param name="filterOrDefault">The default value or expression.</param>
        /// <param name="defaultValue">The default value if the <paramref name="filterOrDefault"/> was a filter.</param>
        /// <returns>The EdgeQL equivalent of the method.</returns>
        [MethodName(nameof(Enumerable.FirstOrDefault))]
        public string FirstOrDefault(TranslatedParameter source, TranslatedParameter filterOrDefault, TranslatedParameter? defaultValue)
        {
            if (filterOrDefault.IsScalarType)
                return $"{ElementAt(source, "0")} ?? {filterOrDefault}";

            // get the parameter name for the filter
            var name = ((LambdaExpression)filterOrDefault.RawValue).Parameters[0].Name;
            var set = source.IsScalarArrayType ? $"array_unpack({source})" : source.ToString();
            var returnType = ((LambdaExpression)filterOrDefault.RawValue).Parameters[0].Type;
            return $"<{EdgeDBTypeUtils.GetEdgeDBScalarOrTypeName(returnType)}>array_get(array_agg((select {name} := {set} filter {filterOrDefault})), 0){(defaultValue != null ? $" ?? {defaultValue}" : String.Empty)}";
        }

    }
}
