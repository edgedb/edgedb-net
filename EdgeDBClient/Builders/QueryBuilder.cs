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

        static QueryBuilder()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.GetInterfaces().Contains(typeof(IEdgeQLOperator)));

            var converters = new Dictionary<ExpressionType, IEdgeQLOperator>();

            foreach (var type in types)
            {
                var inst = (IEdgeQLOperator)Activator.CreateInstance(type)!;

                converters.Add(inst.Operator, inst);
            }

            _converters = converters;
        }


        public static BuiltQuery BuildSelectQuery<TInner>(Expression<Func<TInner, bool>> func)
        {
            var context = new QueryContext<TInner>(func);
            var args = ConvertExpression(context.Body!, context);

            // by default return all fields
            var fields = typeof(TInner).GetProperties().Select(x => x.GetCustomAttribute<EdgeDBProperty>()?.Name ?? x.Name);
            var queryText = $"SELECT {typeof(TInner).Name} {{ {string.Join(", ", fields)} }} filter {args.Filter}";

            return new BuiltQuery
            {
                QueryText = queryText,
                Parameters = args.Arguments
            };
        }

        private static (string Filter, Dictionary<string, object?> Arguments) ConvertExpression<TInner>(Expression s, QueryContext<TInner> context)
        {
            if(s is BinaryExpression bin)
            {
                // compute left and right
                var left = ConvertExpression(bin.Left, context);
                var right = ConvertExpression(bin.Right, context);

                // get converter 
                if (_converters.TryGetValue(s.NodeType, out var conv))
                {
                    return (conv.Build(left.Filter, right.Filter), left.Arguments.Concat(right.Arguments).ToDictionary(x => x.Key, x => x.Value));
                }
                else throw new NotSupportedException($"Couldn't find operator for {s.NodeType}");

            }

            if(s is MethodCallExpression mc)
            {
                // check if its a edgeql method
                if (mc.Method.DeclaringType != typeof(EdgeQL))
                    throw new NotSupportedException($"Cannot use method {mc.Method} because there is no conversion to edgeql for it!");

                // get the equivilant operator
                var op = mc.Method.GetCustomAttribute<EquivalentOperator>();

                if (op == null)
                    throw new NotSupportedException($"Couldn't find operator for method {mc.Method}");

                // parse the arguments
                var arguments = mc.Arguments.Select(x => ConvertExpression(x, context));

                return (op.Operator.Build(arguments.Select(x => x.Filter).ToArray()), arguments.SelectMany(x => x.Arguments).ToDictionary(x => x.Key, x => x.Value));
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
                return (ParseArgument(constant.Value), new());
            }

            return ("", new());
        }

        private static string RecurseNameLookup(MemberExpression expression)
        {
            List<string?> tree = new();

            tree.Add(expression.Member.GetCustomAttribute<EdgeDBProperty>()?.Name ?? expression.Member.Name);

            var t = expression.Expression?.GetType();

            if (expression.Expression is MemberExpression innerExp)
                tree.Add(RecurseNameLookup(innerExp));
            if (expression.Expression is ParameterExpression param)
                tree.Add(param.Name);

            tree.Reverse();
            return string.Join('.', tree);
        }

        private static string ParseArgument(object? arg)
        {
            if(arg is string str)
            {
                return $"\"{str}\"";
            }

            // empy set for null
            return arg?.ToString() ?? "{}";
        }
    }
}
