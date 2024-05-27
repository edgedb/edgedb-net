using EdgeDB.Translators.Methods;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Translators
{
    /// <summary>
    ///     Marks this method as a valid method used to translate a <see cref="MethodCallExpression"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    internal class MethodNameAttribute : Attribute
    {
        /// <summary>
        ///     The method name that the current target can translate.
        /// </summary>
        internal readonly string MethodName;

        internal readonly Type[] MethodParameters;

        internal readonly bool IsParameterAware;

        /// <summary>
        ///     Marks this method as a valid method used to translate a <see cref="MethodCallExpression"/>.
        /// </summary>
        /// <param name="methodName">The name of the method that this method can translate.</param>
        /// <param name="methodParameters">The parameters as types of the method.</param>
        public MethodNameAttribute(string methodName, params Type[] methodParameters)
        {
            MethodName = methodName;
            MethodParameters = methodParameters;
            IsParameterAware = methodParameters.Length > 0;
        }

        public MethodNameAttribute(string methodName, bool isParameterAware)
        {
            MethodName = methodName;
            MethodParameters = Array.Empty<Type>();
            IsParameterAware = isParameterAware;
        }
    }

    /// <summary>
    ///     Represents a base method translator for a given type <typeparamref name="TBase"/>.
    /// </summary>
    /// <typeparam name="TBase">
    ///     The base type containing the methods that this translator can translate.
    /// </typeparam>
    internal abstract class MethodTranslator<TBase> : MethodTranslator
    {
        /// <inheritdoc/>
        public override Type TranslatorTargetType => typeof(TBase);
    }

    internal abstract class MethodTranslator
    {
        /// <summary>
        ///     Gets the base type that contains the methods the current translator can
        ///     translate.
        /// </summary>
        public abstract Type TranslatorTargetType { get; }

        /// <summary>
        ///     The dictionary containing all of the methods that the current translator
        ///     can translate.
        /// </summary>
        internal ConcurrentDictionary<string, MethodInfo> MethodTranslators;

        /// <summary>
        ///     Constructs a new <see cref="MethodTranslator"/> and populates
        ///     <see cref="MethodTranslators"/>.
        /// </summary>
        public MethodTranslator()
        {
            // get all methods within the current type that have at least one MethodName attribute
            var methods = GetType().GetMethods().Where(x =>
                x.GetCustomAttributes().Any(x => x.GetType() == typeof(MethodNameAttribute)));

            var tempDict = new Dictionary<string, MethodInfo>();

            // iterate over the methods and add them to the temp dictionary
            foreach (var method in methods)
            {
                foreach (var att in method.GetCustomAttributes().Where(x => x is MethodNameAttribute))
                {
                    tempDict.Add(((MethodNameAttribute)att).MethodName, method);
                }
            }

            // create a new concurrent dictionary from our temp one
            MethodTranslators = new(tempDict);
        }

        internal virtual bool CanTranslate(Type type) => type == TranslatorTargetType;

        internal static bool TryGetTranslator(MethodCallExpression methodCall,
            out MethodTranslatorLookupTable.Handle handle)
            => MethodTranslatorLookupTable.TryGetTranslator(methodCall.Method, out handle);

        /// <summary>
        ///     Translates the given <see cref="MethodCallExpression"/> into a edgeql equivalent expression.
        /// </summary>
        /// <param name="writer">The query string writer to write the translated method to.</param>
        /// <param name="methodCall">The method call expression to translate.</param>
        /// <param name="context">The current context for the method call expression.</param>
        /// <exception cref="NotSupportedException">No translator could be found for the given method expression.</exception>
        public static void TranslateMethod(QueryWriter writer, MethodCallExpression methodCall,
            ExpressionContext context)
        {
            if(!TryGetTranslator(methodCall, out var translator))
                throw new NotSupportedException($"Cannot use method {methodCall.Method} as there is no translator for it");

            writer.LabelVerbose(
                $"method_translation_{translator.GetType().Name}",
                Defer.This(() => $"Translator type is {translator} picked for {methodCall.Method}"),
                Token.Of(writer => translator.Translate(writer, methodCall, context))
            );
        }

        /// <summary>
        ///     Includes an argument if its <see langword="not"/> <see langword="null"/>.
        /// </summary>
        /// <param name="arg">The argument to include.</param>
        /// <returns>
        ///     The argument with the prefix if its <see langword="not"/> <see langword="null"/>;
        ///     otherwise an empty string.
        /// </returns>
        protected Token OptionalArg(TranslatedParameter? arg)
        {
            if (arg is null || arg.IsNullValue)
                return Token.Empty;
            else
                return arg;
        }
    }
}
