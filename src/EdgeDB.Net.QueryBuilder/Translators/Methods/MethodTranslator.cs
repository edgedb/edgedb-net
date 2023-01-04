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
        protected override Type TransaltorTargetType => typeof(TBase);
    }

    internal abstract class MethodTranslator
    {
        /// <summary>
        ///     Gets the base type that contains the methods the current translator can
        ///     translate.
        /// </summary>
        protected abstract Type TransaltorTargetType { get; }

        /// <summary>
        ///     The static dictionary containing all of the method translators.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, MethodTranslator> _translators = new();

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
            var methods = GetType().GetMethods().Where(x => x.GetCustomAttributes().Any(x => x.GetType() == typeof(MethodNameAttribute)));

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
        
        /// <summary>
        ///     Statically initializes the abstract method translator and populates 
        ///     <see cref="_translators"/>.
        /// </summary>
        static MethodTranslator()
        {
            var types = Assembly.GetExecutingAssembly().DefinedTypes;

            // load current translators
            var translators = types.Where(x => x.BaseType?.Name == "MethodTranslator`1" || (x.BaseType == typeof(MethodTranslator) && x.Name != "MethodTranslator`1"));

            // iterate over the translators and initialize them and store them in the translators
            // dictionary
            foreach (var translator in translators)
            {
                var inst = (MethodTranslator)Activator.CreateInstance(translator)!;
                _translators[inst.TransaltorTargetType] = inst;
            }
        }

        /// <summary>
        ///     Attempts to translate the given <see cref="MethodCallExpression"/> into a edgeql equivalent expression.
        /// </summary>
        /// <param name="methodCall">The method call expression to translate.</param>
        /// <param name="context">The current context for the method call expression.</param>
        /// <param name="translatedMethod">The out result containing the translated method.</param>
        /// <returns>
        ///     <see langword="true"/> if the <paramref name="methodCall"/> was translated; otherwise <see langword="false"/>.
        /// </returns>
        public static bool TryTranslateMethod(MethodCallExpression methodCall, ExpressionContext context, [MaybeNullWhen(false)] out string translatedMethod)
        {
            translatedMethod = null;
            try
            {
                translatedMethod = TranslateMethod(methodCall, context);
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        ///     Translates the given <see cref="MethodCallExpression"/> into a edgeql equivalent expression.
        /// </summary>
        /// <param name="methodCall">The method call expression to translate.</param>
        /// <param name="context">The current context for the method call expression.</param>
        /// <returns>
        ///     The translated expression.
        /// </returns>
        /// <exception cref="NotSupportedException">No translator could be found for the given method expression.</exception>
        public static string TranslateMethod(MethodCallExpression methodCall, ExpressionContext context)
        {
            var type = methodCall.Method.DeclaringType;
            MethodTranslator? translator = null;

            while(type != null && !_translators.TryGetValue(type, out translator))
            {
                type = type.BaseType;
            }

            if(type is null || translator is null)
                throw new NotSupportedException($"Cannot use method {methodCall.Method} as there is no translator for it");

            return translator.Translate(methodCall, context);
        }

        /// <summary>
        ///     Includes an argument if its <see langword="not"/> <see langword="null"/>.
        /// </summary>
        /// <param name="arg">The argument to include.</param>
        /// <param name="prefix">The prefix of the argument.</param>
        /// <returns>
        ///     The argument with the prefix if its <see langword="not"/> <see langword="null"/>;
        ///     otherwise an empty string.
        /// </returns>
        protected string OptionalArg(string? arg, string prefix = ", ")
        {
            if (arg is null)
                return string.Empty;
            else
                return $"{prefix}{arg}";
        }

        /// <summary>
        ///     Finds and executes a translater method for the given <see cref="MethodCallExpression"/>.
        /// </summary>
        /// <param name="methodCall">The expression to translate.</param>
        /// <param name="context">The context of the expression.</param>
        /// <returns>The translated version of the method call.</returns>
        /// <exception cref="NotSupportedException">
        ///     No translator could be found for the given method.
        /// </exception>
        protected string Translate(MethodCallExpression methodCall, ExpressionContext context)
        {
            // try to get a method for translating the expression
            if (!_methodTranslators.TryGetValue(methodCall.Method.Name, out var methodInfo))
                throw new NotSupportedException($"Cannot use method {methodCall.Method} as there is no translator for it");

            // get the parameters of the method and check if it references an instance parameter
            var methodParameters = methodInfo.GetParameters();
            var instanceParam = methodParameters.FirstOrDefault()?.Name == "instance" ? methodParameters[0] : null;
            var hasInstanceReference = instanceParam is not null;

            // slice the origional parameters array to exlude the instance parameter if its defined
            if (hasInstanceReference)
                methodParameters = methodParameters[1..];

            // create a new object[] that will contain our parameters for calling the translator method
            object?[] parsedParameters = new object?[methodParameters.Length];

            // iterate over the parameters and parse them.
            for (int i = 0; i != methodParameters.Length; i++)
            {
                var parameterInfo = methodParameters[i];

                // if the current parameter is marked with the ParamArray attribute, set
                // its value to the remaining arguments to the expression and break out of the loop
                if (parameterInfo.GetCustomAttribute<ParamArrayAttribute>() != null)
                {
                    parsedParameters[i] = methodCall.Arguments.Skip(i).Select(x
                        => ExpressionTranslator.ContextualTranslate(x, context)
                    ).ToArray();
                    break;

                }
                else if (methodCall.Arguments.Count > i) 
                {
                    // translate the argument expression
                    var translated = ExpressionTranslator.ContextualTranslate(methodCall.Arguments[i], context);

                    // if the type is a TranslatedParameter, construct a new one and set it in the parsed
                    // parameter array
                    if (parameterInfo.ParameterType == typeof(TranslatedParameter))
                    {
                        parsedParameters[i] = new TranslatedParameter(methodCall.Arguments[i].Type, translated, methodCall.Arguments[i]);
                    }
                    else // fallthru and just set the translated parameter
                        parsedParameters[i] = translated;
                    
                }
                else if (parameterInfo.HasDefaultValue)
                {
                    // set the default value for the parameter
                    parsedParameters[i] = parameterInfo.DefaultValue;
                }
                else if (parameterInfo.ParameterType == typeof(ExpressionContext))
                {
                    // set the context
                    parsedParameters[i] = context;
                }
                else // get the default value for the parameter type
                    parsedParameters[i] = ReflectionUtils.GetDefault(parameterInfo.ParameterType);
            }

            // if its an instance reference, recreate our parsed array to include the instance parameter
            // and set the instance parameter to the translated expression
            if (hasInstanceReference)
            {
                var newParameters = new object?[methodParameters.Length + 1];
                parsedParameters.CopyTo(newParameters, 1);

                newParameters[0] = methodCall.Object is not null
                    ? instanceParam?.ParameterType == typeof(TranslatedParameter) 
                        ? new TranslatedParameter(methodCall.Object.Type, ExpressionTranslator.ContextualTranslate(methodCall.Object, context), methodCall.Object) 
                        : ExpressionTranslator.ContextualTranslate(methodCall.Object, context)
                    : null;

                parsedParameters = newParameters;
            }

            // invoke the translator method and return its results
            return (string)methodInfo.Invoke(this, parsedParameters)!;
        }
    }
}
