using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.CIL
{
    internal readonly struct Instruction
    {
        public readonly OpCode OpCode;
        public readonly object? Oprand;

        public OpCodeType OpCodeType
            => (OpCodeType)OpCode.Value;

        private readonly MethodBase _rootMethod;

        public Instruction(OpCode code, object? oprand, MethodBase method)
        {
            OpCode = code;
            Oprand = oprand;
            _rootMethod = method;
        }

        public MethodBase OprandAsMethod()
        {
            if (OpCode.OperandType != OperandType.InlineMethod && OpCode.OperandType != OperandType.InlineTok)
                throw new InvalidOperationException("The current instruction doesn't reference a method");

            var asm = Assembly.GetExecutingAssembly();
            return _rootMethod.Module.ResolveMethod((int)Oprand!, _rootMethod.DeclaringType!.GenericTypeArguments, _rootMethod.GetGenericArguments())!;
        }

        public Type OprandAsType()
        {
            if (OpCode.OperandType != OperandType.InlineType && OpCode.OperandType != OperandType.InlineTok)
                throw new InvalidOperationException("The current instruction doesn't reference a type");

            return _rootMethod.Module.ResolveType((int)Oprand!, _rootMethod.DeclaringType!.GenericTypeArguments, _rootMethod.GetGenericArguments())!;
        }

        public FieldInfo OprandAsField()
        {
            if (OpCode.OperandType != OperandType.InlineField && OpCode.OperandType != OperandType.InlineTok)
                throw new InvalidOperationException("The current instruction doesn't reference a field");

            return _rootMethod.Module.ResolveField((int)Oprand!, _rootMethod.DeclaringType!.GenericTypeArguments, _rootMethod.GetGenericArguments())!;
        }

        public string OprandAsString()
        {
            if (OpCode.OperandType != OperandType.InlineString)
                throw new InvalidOperationException("The current instruction doesn't reference a string");

            return _rootMethod.Module.ResolveString((int)Oprand!);
        }
        
        public MemberInfo OprandAsMember()
        {
            if (OpCode.OperandType != OperandType.InlineTok)
                throw new InvalidOperationException("Instruction does not reference a member.");

            return _rootMethod.Module.ResolveMember((int)Oprand!)!;
        }

        public byte[] OprandAsSignature()
        {
            if (OpCode.OperandType != OperandType.InlineSig)
                throw new InvalidOperationException("Instruction does not reference a signature.");
            return _rootMethod.Module.ResolveSignature((int)Oprand!);
        }

        public object? ParseOprand()
        {
            return OpCode.OperandType switch
            {
                OperandType.InlineBrTarget => (Label)Oprand!,
                OperandType.InlineField => OprandAsField(),
                OperandType.InlineI => (int)Oprand!,
                OperandType.InlineI8 => (long)Oprand!,
                OperandType.InlineMethod => OprandAsMethod(),
                OperandType.InlineNone => null,
                OperandType.InlineR => (double)Oprand!,
                OperandType.InlineSig => OprandAsSignature(),
                OperandType.InlineString => OprandAsString(),
                OperandType.InlineSwitch => (int)Oprand!,
                OperandType.InlineTok => OprandAsMember(),
                OperandType.InlineType => OprandAsType(),
                OperandType.InlineVar => (short)Oprand!,
                OperandType.ShortInlineBrTarget => (Label)Oprand!,
                OperandType.ShortInlineI => (byte)Oprand!,
                OperandType.ShortInlineR => (float)Oprand!,
                OperandType.ShortInlineVar => (byte)Oprand!,

                _ => Oprand
            };
        }

        public bool TryGetOperandAs<T>([MaybeNullWhen(false)] out T oprand)
        {
            oprand = default!;

            if (ParseOprand() is T op)
            {
                oprand = op;
                return true;
            }
            return false;
        }
    }
}
