using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class StoreElementInterpreter : BaseCILInterpreter
    {
        public StoreElementInterpreter()
            : base(
                  OpCodeType.Stelem,
                  OpCodeType.Stelem_i,
                  OpCodeType.Stelem_i1,
                  OpCodeType.Stelem_i2,
                  OpCodeType.Stelem_i4,
                  OpCodeType.Stelem_i8,
                  OpCodeType.Stelem_r4,
                  OpCodeType.Stelem_r8,
                  OpCodeType.Stelem_ref)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            var value = context.Stack.PopExp();
            var index = context.Stack.PopExp();
            var array = context.Stack.PopExp();

            Expression expression;

            if(instruction.OpCodeType is OpCodeType.Stelem)
            {
                var targetType = instruction.OprandAsType();

                Refine(ref value, context, targetType);

                expression = Expression.Assign(
                    Expression.ArrayIndex(array, index),
                    Expression.Convert(value, targetType)
                );
            }
            else
            {
                // TODO: is this correct for getting array type?
                var arrayType = array.Type.GetElementType()
                    ?? array.Type.GetInterfaces().First(x => x.Name == "IEnumerable`1").GenericTypeArguments[0];

                Refine(ref value, context, arrayType);

                expression = Expression.Assign(
                    Expression.ArrayAccess(array, index),
                    value
                );
            }

            return expression;
        }
    }
}

