using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class ComparisonInterpreter : BaseCILInterpreter
    {
        public ComparisonInterpreter()
            : base(
                  OpCodeType.Clt,
                  OpCodeType.Clt_un,
                  OpCodeType.Cgt,
                  OpCodeType.Cgt_un)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            var right = context.Stack.PopExp();
            var left = context.Stack.PopExp();

            return instruction.OpCodeType switch
            {
                OpCodeType.Clt or OpCodeType.Clt_un
                    => Expression.LessThan(left, right),
                OpCodeType.Cgt or OpCodeType.Cgt_un
                    => Expression.GreaterThan(left, right),
                _ => throw new NotSupportedException($"Cannot find expression for {instruction.OpCodeType}")
            };
        }
    }
}

