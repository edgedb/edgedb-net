using EdgeDB.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public class QueryBuilder
    {
        private static Dictionary<ExpressionType, IEdgeQLOperator> _converters;

        private static Dictionary<string, IEdgeQLOperator> _reservedPropertiesOperators = new()
        {
            { "String.Length", new Len() },
        };

        private static Dictionary<string, IEdgeQLOperator> _reservedFunctionOperators = new()
        {
            { "ICollection.IndexOf", new Find() },
            { "IEnumerable.IndexOf", new Find() },

            { "ICollection.Contains", new Contains() },
            { "IEnumerable.Contains", new Contains() },

            { "String.get_Chars", new Operators.Index() },
            { "Sring.Substring", new Slice() },

            { "ICollection.Concat", new Concat() },
            { "IEnumerable.Concat", new Concat() },
        };

        static QueryBuilder()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.GetInterfaces().Contains(typeof(IEdgeQLOperator)));

            var converters = new Dictionary<ExpressionType, IEdgeQLOperator>();

            foreach (var type in types)
            {
                var inst = (IEdgeQLOperator)Activator.CreateInstance(type)!;

                if(inst.Operator.HasValue)
                    converters.Add(inst.Operator.Value, inst);
            }

            _converters = converters;
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
        private static (string Filter, Dictionary<string, object?> Arguments) ConvertExpression<TInner, TReturn>(Expression s, QueryContext<TInner, TReturn> context)
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
                var left = ConvertExpression(bin.Left, context);
                var right = ConvertExpression(bin.Right, context);

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

                // check if we have a reserved operator for it
                if(_reservedFunctionOperators.TryGetValue($"{mc.Method.DeclaringType!.Name}.{mc.Method.Name}", out op) || (mc.Method.DeclaringType?.GetInterfaces().Any(i => _reservedFunctionOperators.TryGetValue($"{i.Name}.{mc.Method.Name}", out op)) ?? false)) { }
                else if(mc.Method.DeclaringType == typeof(EdgeQL))
                {
                    // get the equivilant operator
                    op = mc.Method.GetCustomAttribute<EquivalentOperator>()?.Operator;
                }

                if (op == null)
                    throw new NotSupportedException($"Couldn't find operator for method {mc.Method}");

                // parse the arguments
                var arguments = mc.Arguments.Select(x => ConvertExpression(x, context));

                var instName = mc.Object != null ? ConvertExpression(mc.Object!, context).Filter : arguments.First().Filter;

                try
                {
                    return (op.Build(new string[] { instName }.Concat(arguments.Skip(mc.Object != null ? 0 : 1).Select(x => x.Filter)).ToArray()), arguments.SelectMany(x => x.Arguments).ToDictionary(x => x.Key, x => x.Value));
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
                    var ts = RecurseNameLookup(mbs);
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

            return ("", new());
        }

        private static string RecurseNameLookup(MemberExpression expression)
        {
            List<string?> tree = new();

            tree.Add(expression.Member.GetCustomAttribute<EdgeDBProperty>()?.Name ?? expression.Member.Name);

            if (expression.Expression is MemberExpression innerExp)
                tree.Add(RecurseNameLookup(innerExp));
            if (expression.Expression is ParameterExpression param)
                tree.Add(param.Name);

            tree.Reverse();
            return string.Join('.', tree);
        }

        private static string ParseArgument<TInner, TReturn>(object? arg, QueryContext<TInner, TReturn> context)
        {
            if(arg is string str)
                return $"\"{str}\"";

            if (arg is char chr)
                return $"\"{chr}\"";

            if(context.IsCharContext && arg is int c)
            {
                return $"\"{char.ConvertFromUtf32(c)}\"";
            }

            // empy set for null
            return arg?.ToString() ?? "{}";
        }

        private static string GetTypeName(Type t)
            => t.GetCustomAttribute<EdgeDBType>()?.Name ?? t.Name;

        private static string GetPropertyName(PropertyInfo t)
            => t.GetCustomAttribute<EdgeDBProperty>()?.Name ?? t.Name;

        private static string GetTypePrefix(Type t)
        {
            var edgeqlType = PacketSerializer.GetEdgeQLType(t);

            if (edgeqlType == null)
                throw new NotSupportedException($"No edgeql type map found for type {t}");

            return $"<{edgeqlType}>";
        }
    }
}
