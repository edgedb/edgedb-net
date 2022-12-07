using System;
using System.Linq.Expressions;

namespace EdgeDB.Translators.Expressions.CustomExpressions
{
    public class EdgeQLVariableExpression : Expression
    {
        public object? Value => _value;
        public override Type Type => _type;

        private readonly object? _value;
        private readonly Type _type;

        public EdgeQLVariableExpression(object? value)
            : this(value, value?.GetType() ?? typeof(void))
        {

        }

        public EdgeQLVariableExpression(object? value, Type type)
        {
            _value = value;
            _type = type;
        }
    }
}

