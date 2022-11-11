using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class BinaryNumericInterpreter : BaseCILInterpreter
    {
        public BinaryNumericInterpreter()
            : base(
                OpCodeType.Add,
                OpCodeType.Add_ovf,
                OpCodeType.Add_ovf_un,
                OpCodeType.Sub,
                OpCodeType.Sub_ovf,
                OpCodeType.Sub_ovf_un,
                OpCodeType.Mul,
                OpCodeType.Mul_ovf,
                OpCodeType.Mul_ovf_un,
                OpCodeType.Div,
                OpCodeType.Div_un,
                OpCodeType.Rem,
                OpCodeType.Rem_un)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            var right = context.Stack.PopExp();
            var left = context.Stack.PopExp();

            return instruction.OpCodeType switch
            {
                OpCodeType.Add or OpCodeType.Add_ovf or OpCodeType.Add_ovf_un
                    => Expression.Add(left, right),
                OpCodeType.Sub or OpCodeType.Sub_ovf or OpCodeType.Sub_ovf_un
                    => Expression.Subtract(left, right),
                OpCodeType.Mul or OpCodeType.Mul_ovf or OpCodeType.Mul_ovf_un
                    => Expression.Multiply(left, right),
                OpCodeType.Div or OpCodeType.Div_un
                    => Expression.Divide(left, right),
                OpCodeType.Rem or OpCodeType.Rem_un
                    => Expression.Modulo(left, right),
                _ => throw new Exception($"Unkown binary numeric operation {instruction.OpCodeType}")
            };
        }
    }
}

