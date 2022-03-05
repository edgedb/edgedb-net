using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DotnetTool.Lexer
{
    internal class SchemaBuffer
    {
        string _buffer;
        int _bufferPos;

        public int Column { get; private set; }
        public int Line { get; private set; }

        public SchemaBuffer(string schema)
        {
            _buffer = schema;
            _bufferPos = 0;
            Column = 1;
            Line = 1;

        }

        public string Peek(int count)
        {
            string ret = "";
            while (count > 0)
            {
                ret += GetNextTextElement(ret.Length);
                count--;
            }
            return ret;
        }

        public string Read(int count)
        {
            return MovePosition(Peek(count));
        }

        public string PeekUntil(string str)
        {
            string ret = "";
            while (!ret.Contains(str))
            {
                string temp = GetNextTextElement(ret.Length);
                if (temp.Length == 0)
                {
                    break;
                }
                ret += temp;
            }
            return ret;
        }

        public string ReadUntil(string str)
        {
            return MovePosition(PeekUntil(str));
        }

        public string ReadWhile(Predicate<string> pred)
        {
            string ret = "";
            while (true)
            {
                string temp = GetNextTextElement(ret.Length);
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
            int index = portion.LastIndexOf('\n');
            if (index >= 0)
            {
                Column = new StringInfo(portion.Substring(index + 1)).LengthInTextElements + 1;
                Line += portion.Count((ch) => ch == '\n');
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
                string elem = StringInfo.GetNextTextElement(_buffer, _bufferPos + offset);
                if (_bufferPos + offset + elem.Length < _buffer.Length)
                {
                    return elem;
                }
                return elem;
            }
        }

    }
}
