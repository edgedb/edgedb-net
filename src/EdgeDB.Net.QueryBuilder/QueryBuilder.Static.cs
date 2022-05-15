using EdgeDB.DataTypes;
using EdgeDB.Operators;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace EdgeDB
{
    public partial class QueryBuilder
    {
        private static readonly Dictionary<ExpressionType, IEdgeQLOperator> _converters;
        private static readonly Dictionary<IEdgeQLOperator, Type> _operators = new();

        private static readonly Dictionary<string, IEdgeQLOperator> _reservedPropertiesOperators = new()
        {
            { "String.Length", new StringLength() },
        };

        private static readonly Dictionary<string, IEdgeQLOperator> _reservedFunctionOperators = new(EdgeQL.FunctionOperators)
        {
            { "ICollection.IndexOf", new GenericFind() },
            { "IEnumerable.IndexOf", new GenericFind() },

            { "ICollection.Contains", new GenericContains() },
            { "IEnumerable.Contains", new GenericContains() },

            { "String.get_Chars", new StringIndex() },
            { "Sring.Substring", new StringSlice() },
        };

        private static readonly Dictionary<string, Func<QueryContext, Expression?, Expression?[], QueryBuilder>> _reservedSubQueryFunctions = new()
        {
            {
                "IEnumerable.OrderBy",
                (ctx, source, param) =>
                {
                    var sourceElement = ConvertExpression(source!, ctx);
                    return new QueryBuilder();
                }
            }
        };

        static QueryBuilder()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.GetInterfaces().Contains(typeof(IEdgeQLOperator)));

            var converters = new Dictionary<ExpressionType, IEdgeQLOperator>();

            foreach (var type in types)
            {
                var inst = (IEdgeQLOperator)Activator.CreateInstance(type)!;

                if (inst.ExpressionType.HasValue && !converters.ContainsKey(inst.ExpressionType.Value))
                    converters.Add(inst.ExpressionType.Value, inst);
            }

            // get the funcs
            var methods = typeof(EdgeQL).GetMethods();

            _operators = methods.Select(x =>
            {
                var att = x.GetCustomAttribute<EquivalentOperator>();
                return att == null ? ((MethodInfo? Info, IEdgeQLOperator? Operator))(null, null) : ((MethodInfo? Info, IEdgeQLOperator? Operator))(x, att.Operator);
            }).Where(x => x.Info != null && x.Operator != null).ToDictionary(x => x.Operator!, x => x.Info!.ReturnType);

            _converters = converters;
        }

        public static Type? ReverseLookupFunction(string funcText)
        {
            // check if its a function
            var funcMatch = Regex.Match(funcText, @"^(\w+)\(");

            if (funcMatch.Success)
            {
                var funcName = funcMatch.Groups[1].Value;
                // lookup in our defined ops for this func
                return _operators.FirstOrDefault(x => Regex.IsMatch(x.Key.EdgeQLOperator, @"^\w+\(") && x.Key.EdgeQLOperator.StartsWith($"{funcName}(")).Value;
            }
            return null;
        }

        internal static (string Query, Dictionary<string, object?> Arguments) SerializeQueryObject<TType>(TType obj, QueryBuilderContext? context = null)
        {
            var props = typeof(TType).GetProperties().Where(x => x.GetCustomAttribute<EdgeDBIgnoreAttribute>() == null && x.GetCustomAttribute<EdgeDBPropertyAttribute>()?.IsComputed == false);
            var propertySet = new List<string>();
            var args = new Dictionary<string, object?>();

            foreach (var prop in props)
            {
                var name = GetPropertyName(prop);
                var result = SerializeProperty(prop.PropertyType, prop.GetValue(obj), IsLink(prop), context);

                if (!(context?.IncludeEmptySets ?? true) && result.Property == "{}")
                    continue;

                propertySet.Add($"{name} := {result.Property}");
                args = args.Concat(result.Arguments).ToDictionary(x => x.Key, x => x.Value); // TODO: optimize?
            }



            return ($"{{ {string.Join(", ", propertySet)} }}", args);
        }

        internal static (string Property, Dictionary<string, object?> Arguments) SerializeProperty<TType>(TType value, bool isLink, QueryBuilderContext? context = null)
            => SerializeProperty(typeof(TType), value, isLink, context);
        internal static (string Property, Dictionary<string, object?> Arguments) SerializeProperty(Type type, object? value, bool isLink, QueryBuilderContext? context = null)
        {
            var args = new Dictionary<string, object?>();
            var queryValue = "";
            var varName = $"p_{Guid.NewGuid().ToString().Replace("-", "")}";

            if (isLink)
            {
                // removed with set.
                //if (value is ISet set && set.IsSubQuery && set.QueryBuilder is QueryBuilder builder)
                //{
                //    var result = builder.Build(context?.Enter(x => x.UseDetached = type == set.GetInnerType()) ?? new());
                //    args = args.Concat(result.Parameters).ToDictionary(x => x.Key, x => x.Value);
                //    queryValue = $"({result.QueryText})";
                //}
                if (value is IEnumerable enm)
                {
                    List<QueryBuilder> values = new();
                    // enumerate object links for mock objects
                    foreach (var val in enm)
                    {
                        if (val is not ISubQueryType sub)
                            throw new InvalidOperationException($"Expected a sub query for object type, but got {val.GetType()}");

                        values.Add(sub.Builder);
                    }

                    var vals = values.Select(x =>
                    {
                        args = x.Arguments.Concat(args).ToDictionary(x => x.Key, x => x.Value);
                        return $"({x})";
                    });

                    queryValue = $"{{ {string.Join(", ", vals)} }}";
                }
                else if (value is ISubQueryType sub)
                {
                    var result = sub.Builder.Build(context?.Enter(x => x.UseDetached = type == sub.Builder.QuerySelectorType) ?? new());
                    args = args.Concat(result.Parameters).ToDictionary(x => x.Key, x => x.Value);
                    queryValue = $"({result.QueryText})";
                }
                else if (value == null)
                {
                    queryValue = "{}"; // TODO: change empty set?
                }
                else throw new ArgumentException("Unresolved link parser");
            }
            else if (value is ISubQueryType sub)
            {
                var result = sub.Builder.Build(context?.Enter(x => x.UseDetached = sub.Builder.QuerySelectorType == type) ?? new());
                args = args.Concat(result.Parameters).ToDictionary(x => x.Key, x => x.Value);
                queryValue = $"({result.QueryText})";
            }
            else if (value is IQueryResultObject obj && (context?.IntrospectObjectIds ?? false))
            {
                // generate a select query
                queryValue = $"(select {GetTypeName(type)} filter .id = <uuid>\"{obj.GetObjectId()}\")";
            }
            else if (value is QueryBuilder builder)
            {
                var result = builder.Build(context?.Enter(x => x.UseDetached = builder.QuerySelectorType == type) ?? new());
                args = args.Concat(result.Parameters).ToDictionary(x => x.Key, x => x.Value);
                queryValue = $"({result.QueryText})";
            }
            else if (value == null)
            {
                queryValue = "{}";// TODO: change empty set?
            }
            else
            {
                queryValue = $"<{PacketSerializer.GetEdgeQLType(type)}>${varName}";
                args.Add(varName, value);
            }

            return (queryValue, args);
        }

        internal static List<string> ParseShapeDefinition(object? shape, Type parentType, bool referenceObject = false)
        {
            List<string> shapeProps = new();

            if (shape == null)
                return shapeProps;

            // extract all props
            var type = shape.GetType();

            var props = type.GetProperties();

            foreach (var prop in props)
            {
                // TODO:
                //var isMultiLink = false; //ReflectionUtils.TryGetRawGeneric(typeof(MultiLink<>), parentType, out var multiLink);

                var referenceProp = parentType.GetProperty(prop.Name) ?? parentType.GetProperty(prop.Name); //(isMultiLink ? multiLink!.GenericTypeArguments[0]!.GetProperty(prop.Name) : ;

                if (referenceProp == null)
                    continue;

                var propName = GetPropertyName(referenceProp);

                if (prop.PropertyType == typeof(bool))
                {
                    if ((bool)prop.GetValue(shape)!)
                        shapeProps.Add(propName);
                }
                else if (prop.PropertyType.IsAssignableTo(typeof(QueryBuilder)))
                {
                    var val = (QueryBuilder)prop.GetValue(shape)!;

                    var query = val.Build(new QueryBuilderContext().Enter(x => x.ExplicitShapeDefinition = true));

                    shapeProps.Add($"{propName}:{query.QueryText}");
                }
                else
                {
                    // assume anon type
                    var val = ParseShapeDefinition(prop.GetValue(shape), referenceProp.PropertyType, referenceObject);
                    shapeProps.Add($"{propName}: {{ {string.Join(", ", val)} }}");
                }
            }

            return shapeProps;
        }

        internal static List<string> ParseShapeDefinition(LambdaExpression lambda, QueryBuilderContext builderContext)
        {
            List<string> props = new();

            var body = lambda.Body;

            if (body is not NewExpression bodyExpression)
                throw new ArgumentException($"Cannot infer shape from {body.NodeType}");

            var type = lambda.Body.Type;

            var anonProperties = type.GetProperties();

            for (int i = 0; i != anonProperties.Length; i++)
            {
                var prop = anonProperties[i];
                var param = bodyExpression.Arguments[i];

                switch (param)
                {
                    case ConstantExpression constant:
                        // TODO: extend to non booleans in the future?

                        if (constant.Value is not bool val)
                            throw new InvalidDataException($"Expected boolean but got {constant.Type}");

                        if (val) props.Add($"{GetPropertyName(prop)}");
                        break;
                    case MethodCallExpression mce:
                        // TODO: handle weird method calls
                        var source = mce.Arguments[0];

                        var exp = ConvertExpression(mce, new QueryContext
                        {
                            AllowSubQueryGeneration = true,
                            BuilderContext = builderContext.Enter(x => x.ExplicitShapeDefinition = true),
                        });

                        break;

                    default:
                        break;
                }
            }


            return props;
        }

        internal static List<string> ParseShapeDefinition(QueryBuilderContext context, bool referenceObject = false, params Expression[] shape)
        {
            List<string> props = new();

            foreach (var exp in shape)
            {
                var selector = exp;

                LambdaExpression? lam = null;

                if (selector is LambdaExpression lamd)
                {
                    selector = lamd.Body;
                    lam = lamd;

                    context.ParentQueryTypeName = lamd.Parameters[0].Name;
                    context.ParentQueryType = lamd.Parameters[0].Type;

                    if (lamd.Body.Type.Name.StartsWith("<>f__AnonymousType"))
                    {
                        // anon type: parse that
                        props.AddRange(ParseShapeDefinition(lamd, context));
                        continue;
                    }
                }

                if (selector is MemberExpression mbs)
                {
                    List<Expression> innerExpression = new();

                    void AddExp(MemberExpression m)
                    {
                        innerExpression.Add(m);

                        if (m.Expression is not null and MemberExpression mb)
                            AddExp(mb);
                    }

                    AddExp(mbs);

                    innerExpression.Reverse();

                    if (mbs.Type.GetCustomAttribute<EdgeDBTypeAttribute>() != null)
                    {
                        props.Add($"{mbs.Member.GetCustomAttribute<EdgeDBPropertyAttribute>()?.Name ?? mbs.Member.Name}: {{ {string.Join(", ", ParseShapeDefinition(context, referenceObject, new Expression[] { mbs.Expression! }))} }}");
                    }

                    var name = RecurseNameLookup(mbs);

                    if (lam != null)
                    {
                        name = referenceObject
                            ? name[lam.Parameters[0].Name!.Length..]
                            : name.Substring(lam.Parameters[0].Name!.Length + 1, name.Length - 1 - lam.Parameters[0].Name!.Length);
                        props.Add(name);
                    }
                }
                else if (selector is MethodCallExpression mc)
                {
                    // allow select only
                    if (mc.Method.Name != "Select")
                    {
                        throw new ArgumentException("Only Select method is allowed on property selectors");
                    }

                    // create a dynamic version of this method
                    var funcInner = mc.Arguments[1].Type.GenericTypeArguments[0];
                    var funcSelect = mc.Arguments[1].Type.GenericTypeArguments[1];
                    var method = typeof(QueryBuilder).GetRuntimeMethods().First(x => x.Name == nameof(ParseShapeDefinition)).MakeGenericMethod(funcInner, funcSelect);

                    // make the array arg
                    var arr = Array.CreateInstance(typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(funcInner, funcSelect)), 1);
                    arr.SetValue(mc.Arguments[1], 0);

                    props.Add($"{GetTypeName(funcInner)}: {{ {string.Join(", ", (List<string>)method.Invoke(null, new object[] { referenceObject, arr })!)} }}");
                }
                else if (selector is UnaryExpression unary && unary.Type == typeof(object) && unary.Operand is MemberExpression mb && lam != null)
                {
                    var name = RecurseNameLookup(mb);

                    name = referenceObject && (!name.Contains(".@"))
                        ? name[lam.Parameters[0].Name!.Length..]
                        : name.Substring(lam.Parameters[0].Name!.Length + 1, name.Length - 1 - lam.Parameters[0].Name!.Length);

                    props.Add(name);
                }
                else throw new KeyNotFoundException("Cannot resolve converter");
            }

            return props;
        }

        internal static (List<string> Properties, List<KeyValuePair<string, object?>> Arguments) GetTypePropertyNames(Type t, QueryBuilderContext context, ArgumentAggregationContext? aggregationContext = null)
        {
            List<string> returnProps = new();
            var props = t.GetProperties().Where(x => x.GetCustomAttribute<EdgeDBIgnoreAttribute>() == null);
            var instance = Activator.CreateInstance(t);
            var args = new List<KeyValuePair<string, object?>>();
            // get inner props on types
            foreach (var prop in props)
            {
                var name = prop.GetCustomAttribute<EdgeDBPropertyAttribute>()?.Name ?? prop.Name;
                var type = prop.PropertyType;

                if (ReflectionUtils.IsSubclassOfRawGeneric(typeof(ComputedValue<>), type))
                {
                    if (!context.AllowComputedValues)
                        continue;

                    // its a computed value with a query, expose it
                    var val = (IComputedValue)prop.GetValue(instance)!;
                    returnProps.Add($"{name} := ({val.Builder})");
                    args = val.Builder!.Arguments;
                    continue;
                }

                if (TryGetEnumerableType(prop.PropertyType, out var i) && i.GetCustomAttribute<EdgeDBTypeAttribute>() != null)
                    type = i;

                var edgeqlType = type.GetCustomAttribute<EdgeDBTypeAttribute>();

                if (edgeqlType != null)
                {
                    if (type == t && aggregationContext?.PropertyType == type)
                    {
                        continue;
                    }

                    if (context.MaxAggregationDepth.HasValue && context.MaxAggregationDepth.Value == aggregationContext?.Depth)
                        continue;

                    var result = GetTypePropertyNames(type, context, aggregationContext?.Enter(type) ?? new ArgumentAggregationContext(type));
                    args.AddRange(result.Arguments);
                    returnProps.Add($"{name}: {{ {string.Join(", ", result.Properties)} }}");
                }
                else
                {
                    returnProps.Add(name);
                }
            }

            return (returnProps, args);
        }

        internal class ArgumentAggregationContext
        {
            public Type PropertyType { get; }
            public ArgumentAggregationContext? Parent { get; private set; }
            public int Depth { get; set; }

            public ArgumentAggregationContext(Type propType)
            {
                PropertyType = propType;
                Depth = 0;
            }

            public ArgumentAggregationContext Enter(Type propType)
            {
                return new ArgumentAggregationContext(propType)
                {
                    Parent = this,
                    Depth = Depth + 1
                };
            }
        }

        // TODO: add node checks when in Char context, using int converters while in char context will result in the int being converted to a character.
        internal static (string Filter, Dictionary<string, object?> Arguments) ConvertExpression(Expression s, QueryContext context)
        {
            if (s is MemberInitExpression init)
            {
                var result = new List<(string Filter, Dictionary<string, object?> Arguments)>();
                foreach (MemberAssignment binding in init.Bindings)
                {
                    var innerContext = context.Enter(binding.Expression, modifier: x => x.BindingType = binding.Member.GetMemberType());
                    var value = ConvertExpression(binding.Expression, innerContext);
                    var name = binding.Member.GetCustomAttribute<EdgeDBPropertyAttribute>()?.Name ?? binding.Member.Name;
                    result.Add(($"{name}{(innerContext.IncludeSetOperand ? " :=" : "")} {value.Filter}", value.Arguments));
                }

                return (string.Join(", ", result.Select(x => x.Filter)), result.SelectMany(x => x.Arguments).ToDictionary(x => x.Key, x => x.Value));
            }

            if (s is BinaryExpression bin)
            {
                // compute left and right
                var left = ConvertExpression(bin.Left, context.Enter(bin.Left));
                var right = ConvertExpression(bin.Right, context.Enter(bin.Right));

                // reset char context
                context.IsCharContext = false;

                // get converter 
                return _converters.TryGetValue(s.NodeType, out var conv)
                    ? ((string Filter, Dictionary<string, object?> Arguments))(conv.Build(left.Filter, right.Filter), left.Arguments.Concat(right.Arguments).ToDictionary(x => x.Key, x => x.Value))
                    : throw new NotSupportedException($"Couldn't find operator for {s.NodeType}");

            }

            if (s is UnaryExpression una)
            {
                // TODO: nullable converts?

                // get the value
                var val = ConvertExpression(una.Operand, context);

                // cast only if not char
                var edgeqlType = una.Operand.Type == typeof(char) ? "str" : PacketSerializer.GetEdgeQLType(una.Type);

                // set char context 
                context.IsCharContext = una.Operand.Type == typeof(char);

                return edgeqlType == null
                    ? throw new NotSupportedException($"No edgeql type map found for type {una.Type}")
                    : ((string Filter, Dictionary<string, object?> Arguments))($"<{edgeqlType}>{val.Filter}", val.Arguments);
            }

            if (s is MethodCallExpression mc)
            {
                // check for query builder
                if (TryResolveQueryBuilder(mc, out var innerBuilder))
                {
                    var result = innerBuilder!.Build(context.BuilderContext?.Enter(x =>
                    {
                        x.LimitToOne = !TryGetEnumerableType(context.BindingType ?? mc.Type, out var _);

                    }) ?? new());

                    return ($"({result.QueryText})", result.Parameters.ToDictionary(x => x.Key, x => x.Value));
                }


                List<(string Filter, Dictionary<string, object?> Arguments)>? arguments = new();
                Dictionary<long, string> parameterMap = new();

                // check if we have a reserved operator for it
                if (_reservedFunctionOperators.TryGetValue($"{mc.Method.DeclaringType!.Name}.{mc.Method.Name}", out IEdgeQLOperator? op) || (mc.Method.DeclaringType?.GetInterfaces().Any(i => _reservedFunctionOperators.TryGetValue($"{i.Name}.{mc.Method.Name}", out op)) ?? false))
                {
                    // add the object as a param
                    var objectInst = mc.Object;
                    if (objectInst == null && !context.AllowStaticOperators)
                        throw new ArgumentException("Cannot use static methods that require an instance to build");
                    else if (objectInst != null)
                    {
                        var inst = ConvertExpression(objectInst, context.Enter(objectInst));
                        arguments.Add(inst);
                    }
                }
                else if (mc.Method.DeclaringType == typeof(EdgeQL))
                {
                    // get the equivilant operator
                    op = mc.Method.GetCustomAttribute<EquivalentOperator>()?.Operator;

                    // check for parameter map
                    parameterMap = new Dictionary<long, string>(mc.Method.GetCustomAttributes<ParameterMap>().ToDictionary(x => x.Index, x => x.Name));
                }
                else if (_reservedSubQueryFunctions.TryGetValue($"{mc.Method.DeclaringType!.Name}.{mc.Method.Name}", out var factory))
                {
                    // get source
                    var source = mc.Arguments[0];
                    var builder = factory(context, source, mc.Arguments.Skip(1).ToArray());

                    var subQuery = builder.Build(context.BuilderContext!);
                    return (subQuery.QueryText, subQuery.Parameters.ToDictionary(x => x.Key, x => x.Value)); // TODO: dict init
                }

                if (op == null)
                    throw new NotSupportedException($"Couldn't find operator for method {mc.Method}");

                // parse the arguments
                arguments.AddRange(mc.Arguments.SelectMany((x, i) =>
                {
                    return x is NewArrayExpression newArr
                        ? newArr.Expressions.Select((x, i) => ConvertExpression(x, context.Enter(x, i)))
                        : (IEnumerable<(string Filter, Dictionary<string, object?> Arguments)>)(new (string Filter, Dictionary<string, object?> Arguments)[] { ConvertExpression(x, context.Enter(x)) })!;
                }));

                // add our parameter map
                if (parameterMap.Any())
                {
                    var genericMethod = mc.Method.GetGenericMethodDefinition();
                    var genericTypeArgs = mc.Method.GetGenericArguments();
                    var genericDict = genericMethod.GetGenericArguments().Select((x, i) => new KeyValuePair<string, Type>(x.Name, genericTypeArgs[i])).ToDictionary(x => x.Key, x => x.Value);
                    foreach (var item in parameterMap)
                    {
                        if (genericDict.TryGetValue(item.Value, out var strongType))
                        {
                            // convert the strong type
                            var typename = PacketSerializer.GetEdgeQLType(strongType) ?? GetTypeName(strongType);

                            // insert into arguments
                            arguments.Insert((int)item.Key, (typename, new()));
                        }
                    }
                }

                try
                {
                    string builtOperator = op.Build(arguments.Select(x => x.Filter).ToArray());

                    switch (op)
                    {
                        case LinksAddLink or LinksRemoveLink:
                            {
                                context.IncludeSetOperand = false;
                            }
                            break;
                        case VariablesReference:
                            {
                                context.BuilderContext?.AddTrackedVariable(builtOperator);
                            }
                            break;
                        default:
                            builtOperator = $"({builtOperator})";
                            break;
                    }

                    return (builtOperator, arguments.SelectMany(x => x.Arguments).ToDictionary(x => x.Key, x => x.Value));
                }
                catch (Exception x)
                {
                    throw new NotSupportedException($"Failed to convert {mc.Method} to a EdgeQL expression", x);
                }
            }

            if (s is MemberExpression mbs && s.NodeType == ExpressionType.MemberAccess)
            {
                if (mbs.Expression is ConstantExpression innerConstant)
                {
                    if (IsEdgeQLType(innerConstant.Type))
                    {
                        // assume its a reference to another property and use the self reference context
                        var name = mbs.Member.GetCustomAttribute<EdgeDBPropertyAttribute>()?.Name ?? mbs.Member.Name;
                        return ($".{name}", new());
                    }

                    object? value = null;
                    Dictionary<string, object?> arguments = new();

                    switch (mbs.Member.MemberType)
                    {
                        case MemberTypes.Field:
                            value = ((FieldInfo)mbs.Member).GetValue(innerConstant.Value);
                            break;
                        case MemberTypes.Property:
                            value = ((PropertyInfo)mbs.Member).GetValue(innerConstant.Value);
                            break;
                    }

                    arguments.Add(mbs.Member.Name, value);

                    var edgeqlType = PacketSerializer.GetEdgeQLType(mbs.Type);

                    return edgeqlType == null
                        ? throw new NotSupportedException($"No edgeql type map found for type {mbs.Type}")
                        : ((string Filter, Dictionary<string, object?> Arguments))($"<{edgeqlType}>${mbs.Member.Name}", arguments);
                }
                // TODO: optimize this
                else if (mbs.Expression is MemberExpression innermbs && _reservedPropertiesOperators.TryGetValue($"{innermbs.Type.Name}.{mbs.Member.Name}", out var op))
                {
                    // convert the entire expression with the func
                    var ts = RecurseNameLookup(mbs, true);
                    if (ts.StartsWith($"{context.ParameterName}."))
                    {
                        return (op.Build(ts.Substring(context.ParameterName!.Length, ts.Length - context.ParameterName.Length)), new());
                    }
                }
                else
                {
                    // check for variable access with recursion
                    // tostring it and check the starter accesser for our parameter
                    var ts = RecurseNameLookup(mbs);
                    if (ts.StartsWith($"{context.ParameterName}."))
                    {
                        return (ts.Substring(context.ParameterName!.Length, ts.Length - context.ParameterName.Length), new());
                    }

                    if (TryResolveOperator(mbs, out var opr, out var exp) && opr is VariablesReference)
                    {
                        if (exp == null || opr == null)
                            throw new InvalidOperationException("Got faulty operator resolve results");

                        var varName = ConvertExpression(exp, context.Enter(exp));

                        var param = Expression.Parameter(exp.Method.ReturnType, "x");
                        var newExp = mbs.Update(param);
                        var func = Expression.Lambda(newExp, param);

                        var accessors = ConvertExpression(func.Body, new QueryContext
                        {
                            Body = func.Body,
                            ParameterName = "x",
                            ParameterType = exp.Method.ReturnType
                        });

                        context.BuilderContext?.AddTrackedVariable($"{varName.Filter}{accessors.Filter}");

                        // TODO: optimize dict
                        return ($"{varName.Filter}{accessors.Filter}", varName.Arguments.Concat(accessors.Arguments).ToDictionary(x => x.Key, x => x.Value));
                    }

                    throw new NotSupportedException($"Unknown handler for member access: {mbs}");
                }
            }

            if (s is ConstantExpression constant && s.NodeType == ExpressionType.Constant)
            {
                return (ParseArgument(constant.Value, context), new());
            }

            if (s is NewArrayExpression newArr)
            {
                IEnumerable<(string Filter, Dictionary<string, object?> Arguments)>? values;
                // check if its a 'params' array
                if (context.ParameterIndex.HasValue && context.Parent?.Body is MethodCallExpression callExpression)
                {
                    var p = callExpression.Method.GetParameters();

                    if (p[context.ParameterIndex.Value].GetCustomAttribute<ParamArrayAttribute>() != null)
                    {
                        // return joined by ,
                        values = newArr.Expressions.Select((x, i) => ConvertExpression(x, context.Enter(x, i)));

                        return (string.Join(", ", values.Select(x => x.Filter)), values.SelectMany(x => x.Arguments).ToDictionary(x => x.Key, x => x.Value));
                    }
                }

                // return normal array 
                values = newArr.Expressions.Select((x, i) => ConvertExpression(x, context.Enter(x, i)));

                return ($"[{string.Join(", ", values.Select(x => x.Filter))}]", values.SelectMany(x => x.Arguments).ToDictionary(x => x.Key, x => x.Value));
            }

            return ("", new());
        }

        internal static bool TryResolveQueryBuilder(MethodCallExpression mc, out QueryBuilder? builder)
        {
            builder = null;

            if (mc.Object is not MethodCallExpression obj)
                return false;

            while (obj is MethodCallExpression innermc && innermc.Object != null && innermc.Object is MethodCallExpression innerInnermc && (!obj?.Type.IsAssignableTo(typeof(QueryBuilder)) ?? true))
            {
                obj = innerInnermc;
            }

            if (obj?.Type.IsAssignableTo(typeof(QueryBuilder)) ?? false)
            {
                // execute it
                builder = Expression.Lambda<Func<QueryBuilder>>(obj).Compile()();
                return true;
            }

            return false;

        }

        internal static bool TryResolveOperator(MemberExpression mc, out IEdgeQLOperator? edgeQLOperator, out MethodCallExpression? expression)
        {
            edgeQLOperator = null;
            expression = null;

            Expression? currentExpression = mc;

            while (currentExpression != null)
            {
                if (currentExpression is MethodCallExpression mcs && mcs.Method.DeclaringType == typeof(EdgeQL) && mcs.Method.Name == nameof(EdgeQL.Var))
                {
                    edgeQLOperator = mcs.Method.GetCustomAttribute<EquivalentOperator>()!.Operator;
                    expression = mcs;
                    return true;
                }

                if (currentExpression is MemberExpression mcin)
                    currentExpression = mcin.Expression;
                else
                    break;
            }

            return false;

        }

        internal static string RecurseNameLookup(MemberExpression expression, bool skipStart = false)
        {
            List<string?> tree = new();

            if (!skipStart)
                tree.Add(GetPropertyName(expression.Member));

            if (expression.Expression is MemberExpression innerExp)
                tree.Add(RecurseNameLookup(innerExp));
            if (expression.Expression is ParameterExpression param)
                tree.Add(param.Name);

            tree.Reverse();
            return string.Join('.', tree);
        }

        internal static string ParseArgument(object? arg, QueryContext context)
        {
            if (arg is string str)
                return context.IsVariableReference ? str : $"\"{str}\"";

            if (arg is char chr)
                return $"\"{chr}\"";

            if (context.IsCharContext && arg is int c)
            {
                return $"\"{char.ConvertFromUtf32(c)}\"";
            }

            if (arg is Type t)
            {
                return PacketSerializer.GetEdgeQLType(t) ?? GetTypeName(t) ?? t.Name;
            }

            if (arg != null)
            {
                var type = arg.GetType();

                if (type.IsEnum)
                {
                    // check for the serialization method attribute
                    var att = type.GetCustomAttribute<EnumSerializerAttribute>();
                    return att != null
                        ? att.Method switch
                        {
                            SerializationMethod.Lower => $"\"{arg.ToString()?.ToLower()}\"",
                            SerializationMethod.Numeric => Convert.ChangeType(arg, type.BaseType ?? typeof(int)).ToString() ?? "{}",
                            _ => "{}"
                        }
                        : Convert.ChangeType(arg, type.BaseType ?? typeof(int)).ToString() ?? "{}";
                }
            }


            // empy set for null
            return arg?.ToString() ?? "{}";
        }

        internal static bool IsEdgeQLType(Type t)
            => t.GetCustomAttribute<EdgeDBTypeAttribute>() != null;

        internal static string GetTypeName(Type t)
            => t.GetCustomAttribute<EdgeDBTypeAttribute>()?.Name ?? t.Name;

        internal static string GetPropertyName(MemberInfo t)
        {
            var name = t.GetCustomAttribute<EdgeDBPropertyAttribute>()?.Name ?? t.Name;

            //if (ReflectionUtils.IsSubclassOfRawGeneric(typeof(MultiLink<>), t.DeclaringType))
            //{
            //    name = $"@{name}";
            //}

            return name;
        }

        internal static string GetTypePrefix(Type t)
        {
            var edgeqlType = PacketSerializer.GetEdgeQLType(t);

            if (edgeqlType == null)
                throw new NotSupportedException($"No edgeql type map found for type {t}");

            return $"<{edgeqlType}>";
        }

        internal static bool TryGetEnumerableType(Type t, out Type type)
        {
            type = t;

            if (t.Name == typeof(IEnumerable<>).Name)
            {
                type = t.GenericTypeArguments[0];
                return true;
            }

            if (t.GetInterfaces().Any(x => x.Name == typeof(IEnumerable<>).Name))
            {
                var i = t.GetInterface(typeof(IEnumerable<>).Name)!;
                type = i.GenericTypeArguments[0];
                return true;
            }

            return false;
        }

        internal static bool IsLink(PropertyInfo? info)
        {
            if (info == null)
                return false;

            return
                (info.GetCustomAttribute<EdgeDBPropertyAttribute>()?.IsLink ?? false) ||
                info.PropertyType.GetCustomAttribute<EdgeDBTypeAttribute>() != null ||
                (TryGetEnumerableType(info.PropertyType, out var inner) && inner.GetCustomAttribute<EdgeDBTypeAttribute>() != null);
        }

        internal static Type CreateMockedType(Type mock)
        {
            if (mock.IsValueType || mock.IsSealed)
                throw new InvalidOperationException($"Cannot create mocked type from {mock}");

            var tb = ReflectionUtils.GetTypeBuilder($"SubQuery{mock.Name}_{Guid.NewGuid().ToString().Replace("-", "")}",
                   TypeAttributes.Public |
                   TypeAttributes.Class |
                   TypeAttributes.AutoClass |
                   TypeAttributes.AnsiClass |
                   TypeAttributes.BeforeFieldInit |
                   TypeAttributes.AutoLayout);

            tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
            var get = typeof(ISubQueryType).GetMethod("get_Builder");
            var set = typeof(ISubQueryType).GetMethod("set_Builder");
            ReflectionUtils.CreateProperty(tb, "Builder", typeof(QueryBuilder), get, set);
            tb.SetParent(mock);
            tb.AddInterfaceImplementation(typeof(ISubQueryType));

            Type objectType = tb.CreateType()!;

            return objectType;
        }
    }

    public interface ISubQueryType
    {
        QueryBuilder Builder { get; set; }
    }
}
