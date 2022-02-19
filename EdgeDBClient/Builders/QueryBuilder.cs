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


        public static string BuildSelectQuery<TInner>(Expression<Func<TInner, bool>> func)
        {
            var context = new QueryContext<TInner>(func);
            var args = ConvertExpression(context.Body!, context);

            // by default return all fields
            var fields = typeof(TInner).GetProperties().Select(x => x.GetCustomAttribute<EdgeDBProperty>()?.Name ?? x.Name);
            return $"SELECT {typeof(TInner).Name} {{ {string.Join(", ", fields)} }} filter {args}";
        }

        private static string ConvertExpression<TInner>(Expression s, QueryContext<TInner> context)
        {
            if(s is BinaryExpression bin)
            {
                // compute left and right
                var left = ConvertExpression(bin.Left, context);
                var right = ConvertExpression(bin.Right, context);

                // get converter 
                if (_converters.TryGetValue(s.NodeType, out var conv))
                {
                    return conv.Build(left, right);
                }
                else throw new NotSupportedException($"Couldn't find operator for {s.NodeType}");

            }

            if (s is MemberExpression mbs && s.NodeType == ExpressionType.MemberAccess)
            {
                var t = mbs.Expression?.GetType();

                object? value = null;

                if (mbs.Expression is ConstantExpression innerConstant)
                {
                    switch (mbs.Member.MemberType)
                    {
                        case MemberTypes.Field:
                            value = ((FieldInfo)mbs.Member).GetValue(innerConstant.Value);
                            break;
                        case MemberTypes.Property:
                            value = ((PropertyInfo)mbs.Member).GetValue(innerConstant.Value);
                            break;
                    }
                }
                else 
                {
                    // tostring it and check the starter accesser for our parameter
                    var ts = RecurseNameLookup(mbs);
                    if (ts.StartsWith($"{context.ParameterName}."))
                    {
                        return ts.Substring(context.ParameterName!.Length, ts.Length - context.ParameterName.Length);
                    }

                    throw new NotSupportedException($"Unknown handler for member access: {mbs}");
                }

                return ParseArgument(value);

            }

            if (s is ConstantExpression constant && s.NodeType == ExpressionType.Constant)
            {
                return ParseArgument(constant.Value);
            }

            return "";
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
