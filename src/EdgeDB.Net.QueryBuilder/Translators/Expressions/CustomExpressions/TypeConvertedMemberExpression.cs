using EdgeDB.TypeConverters;
using System;
using System.Linq.Expressions;

namespace EdgeDB.Translators.Expressions.CustomExpressions
{
    public class TypeConvertedMemberExpression : Expression
    {
        public Type SourceType => _converter.Source;
        public Type TargetType => _converter.Target;
        public IEdgeDBTypeConverter Converter => _converter;

        public MemberExpression MemberExpression { get; }

        public override Type Type => _converter.Target;

        private readonly IEdgeDBTypeConverter _converter;

        public TypeConvertedMemberExpression(MemberExpression expression, IEdgeDBTypeConverter typeConverter)
            : base()
        {
            MemberExpression = expression;
            _converter = typeConverter;
        }
    }
}

