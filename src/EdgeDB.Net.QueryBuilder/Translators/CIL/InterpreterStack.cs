using System;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace EdgeDB.CIL
{
    internal enum ElementType
    {
        Expression,
        Member
    }

    internal class InterpreterStack
    {
        private readonly Stack<Expression> _expressionStack;
        private readonly Stack<MemberInfo> _memberStack;
        private readonly Stack<ElementType> _elementStack;

        public InterpreterStack()
        {
            _expressionStack = new();
            _memberStack = new();
            _elementStack = new();
        }

        public void Push(MemberInfo member)
        {
            _memberStack.Push(member);
            _elementStack.Push(ElementType.Member);
        }

        public void Push(Expression expression)
        {
            _expressionStack.Push(expression);
            _elementStack.Push(ElementType.Expression);
        }

        public void Push(object value)
        {
            if (value is Expression exp)
                Push(exp);
            else if (value is MemberInfo member)
                Push(member);
            else
                throw new NotSupportedException($"Cannot add type of {value.GetType()} to the stack");
        }

        public Expression PopExp()
        {
            if (_elementStack.Peek() is not ElementType.Expression)
                throw new InvalidOperationException();

            _elementStack.Pop();
            return _expressionStack.Pop();
        }

        public MemberInfo PopMember()
        {
            if (_elementStack.Peek() is not ElementType.Member)
                throw new InvalidOperationException();

            _elementStack.Pop();
            return _memberStack.Pop();
        }

        public object Pop()
        {
            return _elementStack.Pop() switch
            {
                ElementType.Member => _memberStack.Pop(),
                ElementType.Expression => _expressionStack.Pop(),
                _ => throw new NotSupportedException()
            };
        }

        public T Pop<T>()
            where T : class
        {
            return (T)Pop()!;
        }

        public ElementType PeekType()
            => _elementStack.Peek();

        public object Peek()
        {
            return _elementStack.Peek() switch
            {
                ElementType.Member => _memberStack.Peek(),
                ElementType.Expression => _expressionStack.Peek(),
                _ => throw new NotSupportedException()
            };
        }

        public IReadOnlyCollection<Expression> GetTree()
            => _expressionStack.ToImmutableArray();
    }
}

