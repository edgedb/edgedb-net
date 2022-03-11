using EdgeDB.DataTypes;
using EdgeDB.Operators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EdgeDB
{
    public partial class QueryBuilder
    {
        private static Dictionary<ExpressionType, IEdgeQLOperator> _converters;
        private static Dictionary<IEdgeQLOperator, Type> _operators = new();

        private static Dictionary<string, IEdgeQLOperator> _reservedPropertiesOperators = new()
        {
            { "String.Length", new EdgeDB.Operators.StringLength() },
        };

        private static Dictionary<string, IEdgeQLOperator> _reservedFunctionOperators = new(EdgeQL.FunctionOperators)
        {
            { "ICollection.IndexOf", new GenericFind() },
            { "IEnumerable.IndexOf", new GenericFind() },

            { "ICollection.Contains", new GenericContains() },
            { "IEnumerable.Contains", new GenericContains() },

            { "String.get_Chars", new StringIndex() },
            { "Sring.Substring", new StringSlice() },
        };

        static QueryBuilder()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.GetInterfaces().Contains(typeof(IEdgeQLOperator)));

            var converters = new Dictionary<ExpressionType, IEdgeQLOperator>();

            foreach (var type in types)
            {
                var inst = (IEdgeQLOperator)Activator.CreateInstance(type)!;

                if (inst.Operator.HasValue && !converters.ContainsKey(inst.Operator.Value))
                    converters.Add(inst.Operator.Value, inst);
            }

            // get the funcs
            var methods = typeof(EdgeQL).GetMethods();

            _operators = methods.Select<MethodInfo, (MethodInfo? Info, IEdgeQLOperator? Operator)>(x =>
            {
                var att = x.GetCustomAttribute<EquivalentOperator>();
                if (att == null)
                    return (null, null);

                return (x, att.Operator);
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

        public static BuiltQuery BuildInsertQuery<TInner>(TInner obj)
        {
            var context = new QueryContext<TInner, bool>();

            var props = typeof(TInner).GetProperties().Where(x => x.GetCustomAttribute<EdgeDBIgnore>() == null);

            Dictionary<string, (string, object?)> propertySet = new();

            foreach(var prop in props)
            {
                var name = GetPropertyName(prop);
                var value = prop.GetValue(obj);

                propertySet.Add($"{name} := {GetTypePrefix(prop.PropertyType)}$p_{name}", ($"p_{name}", value));
            }

            return new BuiltQuery
            {
                Parameters = propertySet.Values.ToDictionary(x => x.Item1, x => x.Item2),
                QueryText = $"insert {GetTypeName(typeof(TInner))} {{ {string.Join(", ", propertySet.Keys)} }}"
            };
        }

        public static BuiltQuery BuildUpsertQuery<TInner>(TInner obj, Expression<Func<TInner, object?>> constraint)
        {
            var context = new QueryContext<TInner, object?>(constraint);

            var builtPredicate = ConvertExpression(constraint.Body, context);

            var typeName = GetTypeName(typeof(TInner));

            var props = typeof(TInner).GetProperties().Where(x => x.GetCustomAttribute<EdgeDBIgnore>() == null);

            Dictionary<(string Name, string VarName), (string VarName, object? Value)> propertySet = new();

            foreach (var prop in props)
            {
                var name = GetPropertyName(prop);
                var value = prop.GetValue(obj);

                propertySet.Add((name, $"{GetTypePrefix(prop.PropertyType)}$p_{name}"), ($"p_{name}", value));
            }

            return new BuiltQuery
            {
                Parameters = propertySet.Values.ToDictionary(x => x.Item1, x => x.Item2),
                QueryText = 
                    $"with {string.Join(", ", propertySet.Select(x => $"{x.Key.Name} := {x.Key.VarName}"))} " +
                    $"insert {typeName} {{ {string.Join(", ", propertySet.Keys.Select(x => $"{x.Name} := {x.Name}"))} }} " +
                    $"unless conflict on {builtPredicate.Filter} " +
                    $"else ( " +
                    $"update {typeName} set {{ {string.Join(", ", propertySet.Keys.Select(x => $"{x.Name} := {x.Name}"))} }}" +
                    $")"
            };
        }

        internal static (string Property, Dictionary<string, object?> Arguments) SerializeQueryObject<TType>(TType obj)
        {
            var props = typeof(TType).GetProperties().Where(x => x.GetCustomAttribute<EdgeDBIgnore>() == null);
            var propertySet = new List<string>();
            var args = new Dictionary<string, object?>();

            foreach (var prop in props)
            {
                var name = GetPropertyName(prop);
                var varName = $"p_{name}";
                var value = prop.GetValue(obj);
                var queryValue = "";


                if (value is ISet set && set.IsSubQuery)
                {
                    args = set.Arguments!.Concat(args).ToDictionary(x => x.Key, x => x.Value);
                    queryValue = $"({set.Query})";
                }
                else
                    queryValue = $"<{PacketSerializer.GetEdgeQLType(prop.PropertyType)}>${varName}";

                propertySet.Add($"{name} := {queryValue}");
                args.Add(varName, value);
            }

            return ($"{{ {string.Join(", ", propertySet)} }}", args);
        }

        internal static List<string> ParsePropertySelectors<TInner, TSelect>(bool referenceObject = false, params Expression<Func<TInner, TSelect>>[] selectors)
        {
            List<string> props = new();

            foreach (var selector in selectors)
            {
                if (selector.Body is MemberExpression mbs)
                {
                    var name = RecurseNameLookup(mbs);

                    if (referenceObject)
                        name = name.Substring(selector.Parameters[0].Name!.Length, name.Length - selector.Parameters[0].Name!.Length);
                    else
                        name = name.Substring(selector.Parameters[0].Name!.Length + 1, name.Length - 1 - selector.Parameters[0].Name!.Length);
                    props.Add(name);
                }
                if(selector.Body is MethodCallExpression mc)
                {
                    // allow select only
                    if(mc.Method.Name != "Select")
                    {
                        throw new ArgumentException("Only Select method is allowed on property selectors");
                    }

                    // create a dynamic version of this method
                    var funcInner = mc.Arguments[1].Type.GenericTypeArguments[0];
                    var funcSelect = mc.Arguments[1].Type.GenericTypeArguments[1];
                    var method = typeof(QueryBuilder).GetRuntimeMethods().First(x => x.Name == nameof(ParsePropertySelectors)).MakeGenericMethod(funcInner, funcSelect);

                    // make the array arg
                    var arr = Array.CreateInstance(typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(funcInner, funcSelect)), 1);
                    arr.SetValue(mc.Arguments[1], 0);

                    props.Add($"{GetTypeName(funcInner)}: {{ {string.Join(", ", (List<string>)method.Invoke(null, new object[] { referenceObject, arr })!)} }}");
                }
            }

            return props;
        }

        internal static List<string> GetTypePropertyNames(Type t)
        {
            List<string> returnProps = new List<string>();
            var props = t.GetProperties();

            // get inner props on types
            foreach(var prop in props)
            {
                var name = prop.GetCustomAttribute<EdgeDBProperty>()?.Name ?? prop.Name;
                var type = prop.PropertyType;

                if (TryGetEnumerableType(prop.PropertyType, out var i) && i.GetCustomAttribute<EdgeDBType>() != null)
                    type = i;

                var edgeqlType = type.GetCustomAttribute<EdgeDBType>();

                if (edgeqlType != null)
                {
                    returnProps.Add($"{name}: {{ {string.Join(", ", GetTypePropertyNames(type))} }}");
                }
                else
                {
                    returnProps.Add(name);
                }
            }

            return returnProps;
        }

        public static BuiltQuery BuildUpdateQuery<TInner>(TInner obj, Expression<Func<TInner, bool>>? predicate = null, params Expression<Func<TInner, object?>>[] selectors)
        {
            predicate ??= x => true;

            var typeName = GetTypeName(typeof(TInner));

            var context = new QueryContext<TInner, bool>(predicate);
            var args = ConvertExpression(predicate.Body!, context);

            Dictionary<string, (string, object?)> parsedProps = new();

            if (selectors.Any())
            {
                foreach(var selector in selectors)
                {
                    if(selector.Body is not MemberExpression mbs)
                    {
                        throw new ArgumentException("Property selector must referemce the type argument");
                    }

                    var name = RecurseNameLookup(mbs);

                    // remove reference name and '.'
                    name = name.Substring(selector.Parameters[0].Name!.Length + 1, name.Length - 1 - selector.Parameters[0].Name!.Length);

                    object? value = null;
                    Type? type = null;
                    switch (mbs.Member.MemberType)
                    {
                        case MemberTypes.Field:
                            value = ((FieldInfo)mbs.Member).GetValue(obj);
                            type = ((FieldInfo)mbs.Member).FieldType;
                            break;
                        case MemberTypes.Property:
                            value = ((PropertyInfo)mbs.Member).GetValue(obj);
                            type = ((PropertyInfo)mbs.Member).PropertyType;
                            break;
                    }

                    parsedProps[$"{name} := {GetTypePrefix(type!)}$p_{name}"] = ($"p_{name}", value);
                }
            }
            else
            {
                var props = typeof(TInner).GetProperties().Where(x => x.GetCustomAttribute<EdgeDBIgnore>() == null);

                foreach(var prop in props)
                {
                    var name = GetPropertyName(prop);
                    var value = prop.GetValue(obj);

                    parsedProps[$"{name} := {GetTypePrefix(prop.PropertyType)}$p_{name}"] = ($"p_{name}", value);
                }
            }

            return new BuiltQuery
            {
                QueryText = $"update {typeName} filter {args.Filter} set {{ {string.Join(", ", parsedProps.Keys)} }}",
                Parameters = args.Arguments.Concat(parsedProps.Select(x => x.Value).ToDictionary(x => x.Item1, x => x.Item2)).ToDictionary(x => x.Key, x => x.Value)
            };
        }

        public static BuiltQuery BuildUpdateQuery<TInner>(Expression<Func<TInner, TInner>> builder, Expression<Func<TInner, bool>>? predicate = null)
        {
            predicate ??= x => true;

            var objectBuilder = ConvertExpression(builder.Body, new QueryContext<TInner, TInner>(builder));

            var typeName = GetTypeName(typeof(TInner));

            var context = new QueryContext<TInner, bool>(predicate);
            var args = ConvertExpression(predicate.Body!, context);

            return new BuiltQuery 
            {
                QueryText = $"update {typeName} filter {args.Filter} set {{ {objectBuilder.Filter} }}",
                Parameters = args.Arguments.Concat(objectBuilder.Arguments).ToDictionary(x => x.Key, x => x.Value)
            };
        }

        public static BuiltQuery BuildSelectQuery<TInner>(Expression<Func<TInner, bool>> selector)
        {
            var context = new QueryContext<TInner, bool>(selector);
            var args = ConvertExpression(context.Body!, context);

            // by default return all fields
            var typename = GetTypeName(typeof(TInner));
            var fields = typeof(TInner).GetProperties().Select(x => x.GetCustomAttribute<EdgeDBProperty>()?.Name ?? x.Name);
            var queryText = $"select {typename} {{ {string.Join(", ", fields)} }} filter {args.Filter}";

            return new BuiltQuery
            {
                QueryText = queryText,
                Parameters = args.Arguments
            };
        }

        // TODO: add node checks when in Char context, using int converters while in char context will result in the int being converted to a character.
        internal static (string Filter, Dictionary<string, object?> Arguments) ConvertExpression<TInner, TReturn>(Expression s, QueryContext<TInner, TReturn> context)
        {
            if(s is MemberInitExpression init)
            {
                var result = new List<(string Filter, Dictionary<string, object?> Arguments)>();
                foreach (MemberAssignment binding in init.Bindings)
                {
                    var value = ConvertExpression(binding.Expression, context);
                    var name = binding.Member.GetCustomAttribute<EdgeDBProperty>()?.Name ?? binding.Member.Name;
                    result.Add(($"{name} := {value.Filter}", value.Arguments));
                }

                return (string.Join(", ", result.Select(x => x.Filter)), result.SelectMany(x => x.Arguments).ToDictionary(x => x.Key, x => x.Value));
            }

            if(s is BinaryExpression bin)
            {
                // compute left and right
                var left = ConvertExpression(bin.Left, context.Enter(bin.Left));
                var right = ConvertExpression(bin.Right, context.Enter(bin.Right));

                // reset char context
                context.IsCharContext = false;

                // get converter 
                if (_converters.TryGetValue(s.NodeType, out var conv))
                {
                    return (conv.Build(left.Filter, right.Filter), left.Arguments.Concat(right.Arguments).ToDictionary(x => x.Key, x => x.Value));
                }
                else throw new NotSupportedException($"Couldn't find operator for {s.NodeType}");

            }

            if(s is UnaryExpression una)
            {
                // TODO: nullable converts?

                // get the value
                var val = ConvertExpression(una.Operand, context);

                // cast only if not char
                var edgeqlType = una.Operand.Type == typeof(char) ? "str" : PacketSerializer.GetEdgeQLType(una.Type);

                // set char context 
                context.IsCharContext = una.Operand.Type == typeof(char);

                if(edgeqlType == null)
                    throw new NotSupportedException($"No edgeql type map found for type {una.Type}");

                return ($"<{edgeqlType}>{val.Filter}", val.Arguments);
            } 

            if(s is MethodCallExpression mc)
            {
                IEdgeQLOperator? op = null;

                List<(string Filter, Dictionary<string, object?> Arguments)>? arguments = new();
                Dictionary<long, string> parameterMap = new();

                // check if we have a reserved operator for it
                if(_reservedFunctionOperators.TryGetValue($"{mc.Method.DeclaringType!.Name}.{mc.Method.Name}", out op) || (mc.Method.DeclaringType?.GetInterfaces().Any(i => _reservedFunctionOperators.TryGetValue($"{i.Name}.{mc.Method.Name}", out op)) ?? false)) 
                {
                    // add the object as a param
                    if (mc.Object == null)
                        throw new ArgumentException("Cannot use static methods that require an instance to build");

                    var inst = ConvertExpression(mc.Object, context.Enter(mc.Object));
                    arguments.Add(inst);
                }
                else if(mc.Method.DeclaringType == typeof(EdgeQL))
                {
                    // get the equivilant operator
                    op = mc.Method.GetCustomAttribute<EquivalentOperator>()?.Operator;

                    // check for parameter map
                    parameterMap = new Dictionary<long, string>(mc.Method.GetCustomAttributes<ParameterMap>().ToDictionary(x => x.Index, x => x.Name));
                }

                if (op == null)
                    throw new NotSupportedException($"Couldn't find operator for method {mc.Method}");

                // parse the arguments
                arguments.AddRange(mc.Arguments.SelectMany((x, i) =>
                {
                    if (x is NewArrayExpression newArr)
                    {
                        return newArr.Expressions.Select((x, i) => ConvertExpression(x, context.Enter(x, i)));
                    }

                    return new (string Filter, Dictionary<string, object?> Arguments)[] { ConvertExpression(x, context.Enter(x)) };
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
                    return ($"({op.Build(arguments.Select(x => x.Filter).ToArray())})", arguments.SelectMany(x => x.Arguments).ToDictionary(x => x.Key, x => x.Value));
                }
                catch(Exception x)
                {
                    throw new NotSupportedException($"Failed to convert {mc.Method} to a EdgeQL expression", x);
                }
            }

            if (s is MemberExpression mbs && s.NodeType == ExpressionType.MemberAccess)
            {
                if (mbs.Expression is ConstantExpression innerConstant)
                {
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

                    if (edgeqlType == null)
                        throw new NotSupportedException($"No edgeql type map found for type {mbs.Type}");

                    return ($"<{edgeqlType}>${mbs.Member.Name}", arguments);
                }
                // TODO: optimize this
                else if(mbs.Expression is MemberExpression innermbs && _reservedPropertiesOperators.TryGetValue($"{innermbs.Type.Name}.{mbs.Member.Name}", out var op))
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
                    // tostring it and check the starter accesser for our parameter
                    var ts = RecurseNameLookup(mbs);
                    if (ts.StartsWith($"{context.ParameterName}."))
                    {
                        return (ts.Substring(context.ParameterName!.Length, ts.Length - context.ParameterName.Length), new());
                    }

                    throw new NotSupportedException($"Unknown handler for member access: {mbs}");
                }
            }

            if (s is ConstantExpression constant && s.NodeType == ExpressionType.Constant)
            {
                return (ParseArgument(constant.Value, context), new());
            }

            if(s is NewArrayExpression newArr)
            {
                IEnumerable<(string Filter, Dictionary<string, object?> Arguments)>? values;
                // check if its a 'params' array
                if (context.ParameterIndex.HasValue && context.Parent?.Body is MethodCallExpression callExpression)
                {
                    var p = callExpression.Method.GetParameters();
                    
                    if(p[context.ParameterIndex.Value].GetCustomAttribute<ParamArrayAttribute>() != null)
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

        internal static string RecurseNameLookup(MemberExpression expression, bool skipStart = false)
        {
            List<string?> tree = new();

            if(!skipStart)
                tree.Add(expression.Member.GetCustomAttribute<EdgeDBProperty>()?.Name ?? expression.Member.Name);

            if (expression.Expression is MemberExpression innerExp)
                tree.Add(RecurseNameLookup(innerExp));
            if (expression.Expression is ParameterExpression param)
                tree.Add(param.Name);

            tree.Reverse();
            return string.Join('.', tree);
        }

        internal static string ParseArgument<TInner, TReturn>(object? arg, QueryContext<TInner, TReturn> context)
        {
            if(arg is string str)
                return $"\"{str}\"";

            if (arg is char chr)
                return $"\"{chr}\"";

            if(context.IsCharContext && arg is int c)
            {
                return $"\"{char.ConvertFromUtf32(c)}\"";
            }

            if(arg is Type t)
            {
                return PacketSerializer.GetEdgeQLType(t) ?? GetTypeName(t) ?? t.Name;
            }

            if(arg != null)
            {
                var type = arg.GetType();

                if (type.IsEnum)
                {
                    // check for the serialization method attribute
                    var att = type.GetCustomAttribute<EnumSerializer>();
                    if(att != null)
                    {
                        return att.Method switch
                        {
                            SerializationMethod.Lower => $"\"{arg.ToString()?.ToLower()}\"",
                            SerializationMethod.Numeric => Convert.ChangeType(arg, type.BaseType ?? typeof(int)).ToString() ?? "{}",
                            _ => "{}"
                        };
                    }
                    else
                    {
                        return Convert.ChangeType(arg, type.BaseType ?? typeof(int)).ToString() ?? "{}";
                    }
                }
            }


            // empy set for null
            return arg?.ToString() ?? "{}";
        }

        internal static string GetTypeName(Type t)
            => t.GetCustomAttribute<EdgeDBType>()?.Name ?? t.Name;

        internal static string GetPropertyName(PropertyInfo t)
            => t.GetCustomAttribute<EdgeDBProperty>()?.Name ?? t.Name;

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

            if(t.Name == typeof(IEnumerable<>).Name)
            {
                type = t.GenericTypeArguments[0];
                return true;
            }

            if(t.GetInterfaces().Any(x => x.Name == typeof(IEnumerable<>).Name))
            {
                var i = t.GetInterface(typeof(IEnumerable<>).Name);
                type = i.GenericTypeArguments[0];
                return true;
            }

            return false;
        }
    }
}
