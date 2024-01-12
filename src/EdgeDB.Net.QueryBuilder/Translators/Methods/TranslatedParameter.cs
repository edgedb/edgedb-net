using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Translators.Methods
{
    /// <summary>
    ///     Represents a parameter used within a method translator.
    /// </summary>
    internal class TranslatedParameter
    {
        /// <summary>
        ///     Gets the type original type of the parameter.
        /// </summary>
        public Type ParameterType { get; }

        /// <summary>
        ///     Gets the translated value of the parameter.
        /// </summary>
        public QueryStringWriter.Proxy ValueProxy { get; }

        /// <summary>
        ///     Gets the raw expression of the parameter.
        /// </summary>
        public Expression RawValue { get; }

        /// <summary>
        ///     Gets whether or not the parameter type is a scalar array.
        /// </summary>
        public bool IsScalarArrayType
            => EdgeDBTypeUtils.TryGetScalarType(ParameterType, out var info) && info.IsArray;

        /// <summary>
        ///     Gets whether or not the parameter is a scalar type.
        /// </summary>
        public bool IsScalarType
            => EdgeDBTypeUtils.TryGetScalarType(ParameterType, out _);

        /// <summary>
        ///     Gets whether or not the parameter is a valid link type.
        /// </summary>
        public bool IsLinkType
            => EdgeDBTypeUtils.IsLink(ParameterType, out _, out _);

        /// <summary>
        ///     Gets whether or not the parameter is a valid multi-link type.
        /// </summary>
        public bool IsMultiLinkType
            => EdgeDBTypeUtils.IsLink(ParameterType, out var isMulti, out _) && isMulti;

        /// <summary>
        ///     Constructs a new <see cref="TranslatedParameter"/>.
        /// </summary>
        /// <param name="type">The type of the parameter.</param>
        /// <param name="value">The proxy to translate the value of the parameter.</param>
        /// <param name="raw">The raw expression of the parameter.</param>
        public TranslatedParameter(Type type, QueryStringWriter.Proxy value, Expression raw)
        {
            ParameterType = type;
            ValueProxy = value;
            RawValue = raw;
        }

        public void WriteTo(QueryStringWriter writer)
        {
            ValueProxy(writer);
        }

        public static implicit operator QueryStringWriter.Value(TranslatedParameter param) => new(param.ValueProxy);
        public static implicit operator QueryStringWriter.FunctionArg(TranslatedParameter param) => new(param.ValueProxy);
    }
}
