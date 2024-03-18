﻿using EdgeDB.Translators.Methods;
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

        /// <summary>
        ///     Marks this method as a valid method used to translate a <see cref="MethodCallExpression"/>.
        /// </summary>
        /// <param name="methodName">The name of the method that this method can translate.</param>
        public MethodNameAttribute(string methodName)
        {
            MethodName = methodName;
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
        protected override Type TranslatorTargetType => typeof(TBase);
    }

    internal abstract class MethodTranslator
    {
        /// <summary>
        ///     Gets the base type that contains the methods the current translator can
        ///     translate.
        /// </summary>
        protected abstract Type TranslatorTargetType { get; }

        /// <summary>
        ///     The static dictionary containing all of the method translators.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, List<MethodTranslator>> _translators = new();

        /// <summary>
        ///     The dictionary containing all of the methods that the current translator
        ///     can translate.
        /// </summary>
        private ConcurrentDictionary<string, MethodInfo> _methodTranslators;

        /// <summary>
        ///     Constructs a new <see cref="MethodTranslator"/> and populates
        ///     <see cref="_methodTranslators"/>.
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
            _methodTranslators = new(tempDict);
        }

        internal virtual bool CanTranslate(Type type) => type == TranslatorTargetType;

        /// <summary>
        ///     Statically initializes the abstract method translator and populates
        ///     <see cref="_translators"/>.
        /// </summary>
        static MethodTranslator()
        {
            var types = Assembly.GetExecutingAssembly().DefinedTypes;

            // load current translators
            var translators = types.Where(x =>
                x.BaseType?.Name == "MethodTranslator`1" ||
                (x.BaseType == typeof(MethodTranslator) && x.Name != "MethodTranslator`1"));

            // iterate over the translators and initialize them and store them in the translators
            // dictionary
            foreach (var translator in translators)
            {
                var inst = (MethodTranslator)Activator.CreateInstance(translator)!;
                _translators.GetOrAdd(inst.TranslatorTargetType, _ => new()).Add(inst);
            }
        }

        internal static bool TryGetTranslator<T>(string name, [NotNullWhen(true)] out MethodTranslator? translator)
            where T : MethodTranslator
        {
            translator = null;

            if (!_translators.TryGetValue(typeof(T), out var translators))
                return false;

            return (translator = translators.FirstOrDefault(x => x._methodTranslators.TryGetValue(name, out _))) is not
                null;
        }

        internal static bool TryGetTranslator(MethodCallExpression methodCall,
            [MaybeNullWhen(false)] out MethodTranslator translator)
        {
            var type = methodCall.Method.DeclaringType;
            List<MethodTranslator>? translators = null;

            while (type != null && !_translators.TryGetValue(type, out translators))
            {
                type = type.BaseType;
            }

            if (translators is null)
            {
                translator = null;
                return false;
            }

            translators.AddRange(_translators
                .FirstOrDefault(x => x.Value.Any(y => y.CanTranslate(methodCall.Method.DeclaringType!)))
                .Value?.Where(x => x.CanTranslate(methodCall.Method.DeclaringType!)).ToArray() ?? Array.Empty<MethodTranslator>());

            translator = translators
                .FirstOrDefault(x => x._methodTranslators.TryGetValue(methodCall.Method.Name, out _));

            return translator is not null;
        }

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
                Value.Of(writer => translator.Translate(writer, methodCall, context))
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
        protected Value OptionalArg(TranslatedParameter? arg)
        {
            if (arg is null)
                return Value.Empty;
            else
                return arg;
        }

        /// <summary>
        ///     Finds and executes a translator method for the given <see cref="MethodCallExpression"/>.
        /// </summary>
        /// <param name="writer">The query string writer to write the translated method call to.</param>
        /// <param name="methodCall">The expression to translate.</param>
        /// <param name="context">The context of the expression.</param>
        /// <exception cref="NotSupportedException">
        ///     No translator could be found for the given method.
        /// </exception>
        public void Translate(QueryWriter writer, MethodCallExpression methodCall, ExpressionContext context)
        {
            // try to get a method for translating the expression
            if (!_methodTranslators.TryGetValue(methodCall.Method.Name, out var methodInfo))
                throw new NotSupportedException(
                    $"Cannot use method {methodCall.Method} as there is no translator for it");

            // get the parameters of the method and check if it references an instance parameter
            var methodParameters = methodInfo.GetParameters();
            var instanceParam = methodParameters.Length >= 2 && methodParameters[1].Name == "instance"
                ? methodParameters[0]
                : null;
            var hasInstanceReference = instanceParam is not null;

            if (methodParameters.Length <= 0)
                throw new InvalidOperationException("Malformed method translator, expecting at least 1 argument");

            if (methodParameters[0].ParameterType != typeof(QueryWriter))
                throw new InvalidOperationException(
                    $"Malformed method translator, expecting first parameter to be a {nameof(QueryWriter)}"
                );

            // slice out the query string writer
            methodParameters = methodParameters[1..];

            // slice the original parameters array to exclude the instance parameter if its defined
            if (hasInstanceReference)
                methodParameters = methodParameters[1..];

            var parsedParameters = new object?[methodParameters.Length];

            // iterate over the parameters and parse them.
            var methodCallArgsIndex = 0;
            for (var i = 0; i != methodParameters.Length; i++)
            {
                var parameterInfo = methodParameters[i];

                // if the current parameter is marked with the ParamArray attribute, set
                // its value to the remaining arguments to the expression and break out of the loop
                if (parameterInfo.GetCustomAttribute<ParamArrayAttribute>() != null)
                {
                    parsedParameters[i] = methodCall.Arguments.Skip(methodCallArgsIndex)
                        .Select(x => new TranslatedParameter(x.Type, x, context)).ToArray();

                    break;
                }
                else if (parameterInfo.ParameterType == typeof(ExpressionContext))
                {
                    // set the context
                    parsedParameters[i] = context;
                }
                else if (parameterInfo.Name == "method" && parameterInfo.ParameterType == typeof(MethodCallExpression))
                {
                    parsedParameters[i] = methodCall;
                }
                else if (methodCall.Arguments.Count > methodCallArgsIndex)
                {
                    parsedParameters[i] = new TranslatedParameter(
                        methodCall.Arguments[methodCallArgsIndex].Type,
                        methodCall.Arguments[methodCallArgsIndex],
                        context
                    );
                    methodCallArgsIndex++;
                }
                else // get the default value for the parameter type
                {
                    parsedParameters[i] = ReflectionUtils.GetDefault(parameterInfo.ParameterType);
                }
            }

            // if its an instance reference, recreate our parsed array to include the instance parameter
            // and set the instance parameter to the translated expression
            if (hasInstanceReference)
            {
                var newParameters = new object?[methodParameters.Length + 1];
                parsedParameters.CopyTo(newParameters, 1);

                newParameters[0] = methodCall.Object is not null
                    ? new TranslatedParameter(
                        methodCall.Object.Type,
                        methodCall.Object,
                        context)
                    : null;

                parsedParameters = newParameters;
            }

            var finalParameters = new object?[parsedParameters.Length + 1];
            finalParameters[0] = writer;
            parsedParameters.CopyTo(finalParameters, 1);

            methodInfo.Invoke(this, finalParameters);
        }
    }
}
