using EdgeDB.Translators.Expressions.CustomExpressions;
using EdgeDB.TypeConverters;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;

namespace EdgeDB.Translators.Expressions
{
    public class PreTranslationExpressionVisitor : ExpressionVisitor
    {
        [return: NotNullIfNotNull("node")]
        public override Expression? Visit(Expression? node)
        {
            var newNode = base.Visit(node);
            return PostProcessNode(newNode);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            // get the property map for the member, to check for type converters
            var disassembled = ExpressionUtils.DisassembleExpression(node).ToArray();
            Expression parent = disassembled[1];

            

            // if its a valid obj type
            if (!TypeBuilder.IsValidObjectType(parent.Type))
                return base.VisitMember(node);

            if (node.Member is not PropertyInfo prop)
                return base.VisitMember(node);

            var info = new EdgeDBPropertyInfo(prop);

            if (info.CustomConverter is not null)
                return new TypeConvertedMemberExpression(node, info.CustomConverter);


            return base.VisitMember(node);
        }

        private static Expression? PostProcessNode(Expression? node)
        {
            switch (node)
            {
                case BinaryExpression binary:
                    {
                        // does one of the sides need to be converted?
                        if (binary.Left is not TypeConvertedMemberExpression && binary.Right is not TypeConvertedMemberExpression)
                            break;

                        var converter = binary.Left is TypeConvertedMemberExpression ltc
                            ? ltc.Converter
                            : ((TypeConvertedMemberExpression)binary.Right).Converter;

                        if (binary.Right is not TypeConvertedMemberExpression)
                        {
                            // convert the right to the target type
                            return binary.Update(binary.Left, binary.Conversion, ConvertWithConverter(converter, binary.Right));
                        }
                        else
                        {
                            return binary.Update(ConvertWithConverter(converter, binary.Left), binary.Conversion, binary.Right);
                        }
                    }
            }

            return node;
        }

        private static Expression ConvertWithConverter(IEdgeDBTypeConverter converter, Expression target)
        {
            switch (target)
            {
                case ConstantExpression constant when constant.Type.IsAssignableTo(converter.Source):
                    return Expression.Constant(converter.ConvertTo(constant.Value), converter.Target);
                case MemberExpression member:
                    {
                        var deconstructed = ExpressionUtils.DisassembleExpression(member).ToArray();

                        var baseExpression = deconstructed.LastOrDefault();

                        // if the resolute expression is a constant expression, assume
                        // were in a set-like context and add it as a variable.
                        if (baseExpression is ConstantExpression constant)
                        {
                            // walk thru the reference tree, you can imagine this as a variac pointer resolution.
                            object? refHolder = constant.Value;

                            for (int i = deconstructed.Length - 2; i >= 0; i--)
                            {
                                // if the deconstructed node is not a member expression, we have something fishy...
                                if (deconstructed[i] is not MemberExpression memberExp)
                                    throw new InvalidOperationException("Member tree does not contain all members. this is a bug, please file a github issue with the query that caused this exception.");

                                // gain the new reference holder to the value we're after
                                refHolder = memberExp.Member.GetMemberValue(refHolder);
                            }

                            // at this point, 'refHolder' is now a direct reference to the property the expression resolves to,
                            // we can add this as our variable.

                            return new EdgeQLVariableExpression(converter.ConvertTo(refHolder), converter.Target);
                        }

                        // TODO: what should be done here?
                        return member;
                    }
                default:
                    throw new NotSupportedException($"Cannot find way to implictly convert {target} to the type {converter.Target}");
            }
        }
    }
}

