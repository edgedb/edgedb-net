using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class BoxInterpreter : BaseCILInterpreter
    {
        public BoxInterpreter()
            : base(OpCodeType.Box)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            var value = context.Stack.PopExp();

            // TODO: accept non-const?

            if (value is not ConstantExpression cnst)
                throw new ArgumentException($"Cannot box {value.GetType()} expression");

            return Expression.Constant(cnst.Value, typeof(object));
        }
    }
}

