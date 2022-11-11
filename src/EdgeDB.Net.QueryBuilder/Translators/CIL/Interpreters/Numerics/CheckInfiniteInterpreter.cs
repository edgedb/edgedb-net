using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class CheckInfiniteInterpreter
        : BaseCILInterpreter
    {
        public CheckInfiniteInterpreter()
            : base(OpCodeType.Ckfinite)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            var number = context.Stack.PopExp();

            var method = number switch
            {
                _ when number.Type == typeof(float)
                    => typeof(float).GetMethod("IsInfinity"),
                _ when number.Type == typeof(double)
                    => typeof(double).GetMethod("IsInfinity"),
                _ => throw new NotSupportedException($"Unable to check infinity for type {number.Type}")
            };

            if (method is null)
                throw new NullReferenceException($"Failed to get infinity check method for type {number.Type}");

            return Expression.Call(null, method!, number);
        }
    }
}

