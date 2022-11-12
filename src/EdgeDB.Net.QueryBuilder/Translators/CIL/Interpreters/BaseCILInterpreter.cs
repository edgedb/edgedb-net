using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EdgeDB.CIL.Interpreters
{
    internal abstract class BaseCILInterpreter
    {
        public OpCodeType[] SupportedCodes { get; }

        public BaseCILInterpreter(params OpCodeType[] codes)
        {
            SupportedCodes = codes;
        }

        public abstract Expression Interpret(Instruction instruction, CILInterpreterContext context);

        protected Expression InterpretTo(Label label, CILInterpreterContext context)
            => InterpretInstructions(context.Reader.ReadTo(label), context);

        protected Expression InterpretInstructions(IEnumerable<Instruction> instructions, CILInterpreterContext context)
        {
            var stack = context.Stack.TransitionStack();
            var blockContext = context.Enter(x =>
            {
                x.Stack = stack;
            });

            foreach (var instruction in instructions)
            {
                var expression = CILInterpreter.Interpret(instruction, blockContext);
                if (expression is DefaultExpression d && d.Type == typeof(void))
                    continue;
                stack.Push(expression);
            }

            var tree = stack.GetTree();

            return tree.Count == 1
                ? tree.First()
                : Expression.Block(tree.Reverse());
        }

        protected unsafe bool EnsureValidTypes(ref Expression expression, Type? target = null)
        {
            var copy = expression;

            switch (expression)
            {
                case ConstantExpression constant when (target is not null):
                    {
                        // for example unsigned numbers are loaded as signed in CIL
                        // and implicitly converted at time-of-use
                        // the logic is always downcasting int to target, never upcasting
                        if (constant.Type.IsSignedNumber() && target.IsUnsignedNumber())
                        {
                            // TODO: size change needs a check here?
                            var converted = constant.Type.ConvertToTargetNumber(constant.Value!, target);
                            expression = Expression.Constant(converted, target);
                        }
                        // since there is no bool type in CIL we must convert an int to bool
                        else if(IsBooleanConversion(constant.Type, target))
                        {
                            expression = Expression.Constant((int)constant.Value! > 0, target);
                        }
                    }
                    break;
                case BinaryExpression binary:
                    {
                        // ensure left is same type as right, if possible
                        var right = binary.Right;

                        EnsureValidTypes(ref right, binary.Left.Type);

                        expression = binary.Update(binary.Left, binary.Conversion, right);
                    }
                    break;
                case UnaryExpression convert when convert.NodeType == ExpressionType.Convert:
                    {
                        // simply the convert if possible by directly changing the type
                        var convertBody = convert.Operand;

                        // if its an upscale of numbers, use convert.changetype
                        if(convertBody.Type.IsNumericType() && convert.Type.IsNumericType() && convertBody is ConstantExpression cnst)
                        {
                            convertBody = Expression.Constant(Convert.ChangeType(cnst.Value, convert.Type));
                        }

                        EnsureValidTypes(ref convertBody, target);

                        // TODO: only set expression if fully converted?
                        expression = convertBody;
                    }
                    break;
            }

            return !Object.ReferenceEquals(copy, expression);
        }

        private static bool IsBooleanConversion(Type source, Type target)
            => source == typeof(int) && target == typeof(bool);
    }
}

