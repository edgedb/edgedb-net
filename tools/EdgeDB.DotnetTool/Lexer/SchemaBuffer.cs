using System.Globalization;

namespace EdgeDB.DotnetTool.Lexer;

internal class SchemaBuffer
{
    private readonly string _buffer;
    private int _bufferPos;

    public SchemaBuffer(string schema)
    {
        _buffer = schema;
        _bufferPos = 0;
        Column = 1;
        Line = 1;
    }

    public int Column { get; private set; }
    public int Line { get; private set; }

    public string Peek(int count)
    {
        var ret = "";
        while (count > 0)
        {
            ret += GetNextTextElement(ret.Length);
            count--;
        }

        return ret;
    }

    public string Read(int count) => MovePosition(Peek(count));

    public string PeekUntil(string str)
    {
        var ret = "";
        while (!ret.Contains(str))
        {
            var temp = GetNextTextElement(ret.Length);
            if (temp.Length == 0)
            {
                break;
            }

            ret += temp;
        }

        return ret;
    }

    public string ReadUntil(string str) => MovePosition(PeekUntil(str));

    public string ReadWhile(Predicate<string> pred)
    {
        var ret = "";
        while (true)
        {
            var temp = GetNextTextElement(ret.Length);
            if (temp.Length == 0 || !pred(temp))
            {
                break;
            }

            ret += temp;
        }

        return MovePosition(ret);
    }

    private string MovePosition(string portion)
    {
        _bufferPos += portion.Length;
        var index = portion.LastIndexOf('\n');
        if (index >= 0)
        {
            Column = new StringInfo(portion[(index + 1)..]).LengthInTextElements + 1;
            Line += portion.Count(ch => ch == '\n');
        }
        else
        {
            Column += new StringInfo(portion).LengthInTextElements;
        }

        return portion;
    }

    private string GetNextTextElement(int offset)
    {
        while (true)
        {
            var elem = StringInfo.GetNextTextElement(_buffer, _bufferPos + offset);
            if (_bufferPos + offset + elem.Length < _buffer.Length)
            {
                return elem;
            }

            return elem;
        }
    }
}
