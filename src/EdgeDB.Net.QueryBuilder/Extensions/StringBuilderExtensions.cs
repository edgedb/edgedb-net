using System.Text;

namespace EdgeDB;

internal static class StringBuilderExtensions
{
    public static StringBuilder QueryArgument(this StringBuilder builder, object type, object name)
        => builder.TypeCast(type).Append('$').Append(name);

    public static StringBuilder TypeCast(this StringBuilder builder, object type)
        => builder.Append('<').Append(type).Append('>');

    public static StringBuilder Function(this StringBuilder builder, object function, params object[] args)
    {
        builder.Append(function);

        builder.Append('(');

        for (var i = 0; i < args.Length; i++)
        {
            builder.Append(args[i]);
            if (i + 1 != args.Length)
                builder.Append(", ");
        }

        return builder.Append(')');
    }

    public readonly struct FunctionArg
    {
        private readonly string? _value;
        private readonly Action<StringBuilder>? _mapper;

        public FunctionArg(string value)
        {
            _value = value;
            _mapper = null;
        }

        public FunctionArg(Action<StringBuilder> mapper)
        {
            _mapper = mapper;
            _value = null;
        }

        public void Append(StringBuilder builder)
        {
            if (_value is not null)
                builder.Append(_value);
            else
            {
                _mapper?.Invoke(builder);
            }
        }

        public static implicit operator FunctionArg(string s) => new(s);
        public static implicit operator FunctionArg(Action<StringBuilder> s) => new(s);
    }

    public static StringBuilder Function(this StringBuilder builder, object function, params FunctionArg[] args)
    {
        builder.Append(function);

        builder.Append('(');

        for (var i = 0; i < args.Length; i++)
        {
            args[i].Append(builder);
            if (i + 1 != args.Length)
                builder.Append(", ");
        }

        return builder.Append(')');
    }
}
