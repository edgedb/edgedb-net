using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace EdgeDB.Translators.Methods;

internal static class MethodTranslatorLookupTable
{
    internal readonly struct Handle
    {
        public readonly MethodTranslator Translator;
        private readonly MethodInfo _translatorMethod;

        public Handle(MethodTranslator translator, MethodInfo translatorMethod)
        {
            Translator = translator;
            _translatorMethod = translatorMethod;
        }

        public WriterProxy Proxy(MethodCallExpression expression, ExpressionContext context)
        {
            var method = _translatorMethod;
            var translator = Translator;
            return writer => Translate(writer, translator, method, expression, context);
        }

        public void Translate(QueryWriter writer, MethodCallExpression expression, ExpressionContext context)
            => Translate(writer, Translator, _translatorMethod, expression, context);

        private static void Translate(QueryWriter writer, MethodTranslator translator, MethodInfo translatorMethod, MethodCallExpression methodCall, ExpressionContext context)
        {
            // TODO: this initialization logic can be refactored and extrapolated into a constructor.

            // get the parameters of the method and check if it references an instance parameter
            var methodParameters = translatorMethod.GetParameters();
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
                else if (parameterInfo.ParameterType == typeof(MethodInfo))
                {
                    parsedParameters[i] = methodCall.Method;
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

            writer.LabelVerbose(
                $"method_translation_{translator.GetType().Name}",
                Defer.This(() => $"Translator type is {translator} picked for {methodCall.Method}"),
                Value.Of(_ => translatorMethod.Invoke(translator, finalParameters))
            );
        }
    }

    private static readonly ConcurrentDictionary<MethodInfo, Handle> _quickLookupTable;

    private static readonly Dictionary<Type, List<MethodTranslator>> _translatorsByTargetType;
    private static readonly List<MethodTranslator> _translators;

    static MethodTranslatorLookupTable()
    {
        _translators = new();
        _translatorsByTargetType = new();
        _quickLookupTable = new();

        var types = Assembly.GetExecutingAssembly().GetTypes();

        // load current translators
        var translators = types.Where(x =>
            x.BaseType?.Name == "MethodTranslator`1" ||
            (x.BaseType == typeof(MethodTranslator) && x.Name != "MethodTranslator`1"));

        foreach (var translator in translators)
        {
            var instance = (MethodTranslator)Activator.CreateInstance(translator)!;
            var translatorTargetMethods = instance.TranslatorTargetType.GetMethods();

            var methods = translator.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            foreach (var method in methods)
            {
                var targetAttributes = method.GetCustomAttributes<MethodNameAttribute>() as MethodNameAttribute[];

                if(targetAttributes is null)
                    continue;

                foreach (var targetAttribute in targetAttributes)
                {
                    var targetMethods = translatorTargetMethods.Where(x => x.Name == targetAttribute.MethodName);

                    if (targetAttribute.IsParameterAware)
                    {
                        targetMethods = targetMethods
                            .Where(x => IsTranslationMatch(method, x));
                    }

                    foreach (var targetMethod in targetMethods)
                    {
                        _quickLookupTable.TryAdd(targetMethod, new Handle(instance, method));
                    }
                }
            }

            _translators.Add(instance);

            if (!_translatorsByTargetType.TryGetValue(instance.TranslatorTargetType, out var translatorsByType))
                translatorsByType = _translatorsByTargetType[instance.TranslatorTargetType] = new();

            translatorsByType.Add(instance);
            //_translatorsByTargetType.Add(instance.TranslatorTargetType, instance);
        }
    }

    public static bool TryGetTranslator(MethodInfo target, [MaybeNullWhen(false)] out Handle translator)
    {
        if (_quickLookupTable.TryGetValue(target, out translator))
            return true;

        return TrySearchAndCacheTranslator(target, out translator);
    }

    private static bool TrySearchAndCacheTranslator(MethodInfo target, [MaybeNullWhen(false)] out Handle translator)
    {
        var baseType = target.DeclaringType!;

        if (_translatorsByTargetType.TryGetValue(baseType, out var translators))
        {
            var targetTranslator = translators
                .SelectMany(x => x.MethodTranslators.Select(y => (methodTranslatorInfo: y, translators: x)))
                .FirstOrDefault(x => x.methodTranslatorInfo.Key == target.Name &&
                            IsTranslationMatch(x.methodTranslatorInfo.Value, target));

            if (targetTranslator.translators is not null)
            {
                translator = _quickLookupTable[target] = new Handle(
                    targetTranslator.translators,
                    targetTranslator.methodTranslatorInfo.Value
                );
                return true;
            }
        }

        foreach (var methodTranslator in _translators)
        {
            if(!methodTranslator.CanTranslate(baseType))
                continue;

            if (methodTranslator.MethodTranslators.TryGetValue(target.Name, out var info))
            {
                translator = _quickLookupTable[target] = new Handle(
                    methodTranslator,
                    info
                );
                return true;
            }
        }

        translator = default;
        return false;
    }

    private static bool IsTranslationMatch(MethodInfo translator, MethodInfo target)
    {
        var parameters = target.GetParameters();
        var translatorMethodAttributes = translator.GetCustomAttributes<MethodNameAttribute>() as MethodNameAttribute[];

        if (translatorMethodAttributes is null)
            return false;

        return translatorMethodAttributes
            .Any(attr => parameters.Length == attr.MethodParameters.Length &&
                parameters
                    .Select((x, i) => attr.MethodParameters[i] == x.ParameterType)
                    .Aggregate((a, b) => a && b)
            );
    }
}
