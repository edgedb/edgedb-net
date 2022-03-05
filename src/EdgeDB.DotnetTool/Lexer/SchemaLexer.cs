using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DotnetTool.Lexer
{
    internal class SchemaLexer
    {
        private readonly Dictionary<string, TokenType> _tokens = new Dictionary<string, TokenType>
        {
            {"module", TokenType.Module },
            {"{", TokenType.BeginBrace },
            {"}", TokenType.EndBrace },
            {"required", TokenType.Required },
            {"property", TokenType.Property },
            {"constraint", TokenType.Constraint },
            {"->", TokenType.TypeArrow },
            {"multi", TokenType.Multi },
            {"single", TokenType.Single },
            {"link", TokenType.Link },
            {"abstract", TokenType.Abstract },
            {";", TokenType.Semicolon },
            {":=", TokenType.Assignment },
            {"on", TokenType.On },
            {"extending", TokenType.Extending },
            {",", TokenType.Comma },
            {"(", TokenType.BeginParenthesis },
            {")", TokenType.EndParenthesis },
            {"annotation", TokenType.Annotation },
            {"type", TokenType.Type },
            {"index", TokenType.Index },
            {"scalar", TokenType.Scalar }
        };

        private readonly SchemaBuffer _reader;
        private readonly Stack<Token> _stack;

        public SchemaLexer(string schema)
        {
            _reader = new SchemaBuffer(schema);
            _stack = new();
        }

        public Token PeekToken()
        {
            if (_stack.Count == 0)
                _stack.Push(ReadTokenInternal());

            return _stack.Peek();
        }

        public Token ReadToken()
        {
            if (_stack.Count > 0)
                return _stack.Pop();
            return ReadTokenInternal();
        }

        private Token ReadTokenInternal()
        {
            ReadWhitespace();

            var elArr = _tokens.Select(x => x.Key.Length).Distinct();

            foreach(var item in elArr)
            {
                if(_tokens.TryGetValue(_reader.Peek(item), out var token))
                {
                    _reader.Read(item);

                    switch (token)
                    {
                        case TokenType.Assignment
                            or TokenType.TypeArrow
                            or TokenType.Extending
                            or TokenType.Constraint:
                            {
                                ReadWhitespace();
                                var value = ReadValue(token == TokenType.Assignment, ';', '{');
                                return NewToken(token, value);
                            }

                        case TokenType.Module
                            or TokenType.Type
                            or TokenType.Property
                            or TokenType.Link:
                            {
                                ReadWhitespace();
                                var value = ReadValue();
                                return NewToken(token, value);
                            }
                        default:
                            return NewToken(token, null);

                    }

                }
            }

            var st = _reader.Peek(1);

            if(st == null || st.Length == 0)
            {
                // end of file
                return NewToken(TokenType.EndOfFile);
            }

            var c = _reader.Peek(1)[0];

            // identifier
            if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
            {
                var val = _reader.ReadWhile(x => !char.IsWhiteSpace(x, 0));
                return NewToken(TokenType.Identifier, val);
            }

            throw new Exception("No token");
        }

        private string ReadValue(bool ignoreSpace = false, params char[] delimiters)
        {
            // read untill we get to a quote, then read everything in the quote until whitespace
            bool isEscaped = false;
            return _reader.ReadWhile(x =>
            {
                if (x == "'" || x == "\"" || x == "[" || x == "]")
                    isEscaped = !isEscaped;

                return isEscaped || ((ignoreSpace || !char.IsWhiteSpace(x, 0)) && (delimiters.Length > 0 ? !delimiters.Contains(x[0]) : true));
            });
        }

        public Token Expect(TokenType type)
        {
            var t = PeekToken();

            if(type == TokenType.EndOfFile)
            {
                throw new Exception("Unexpected end of file");
            }

            if (t.Type != type)
                throw new Exception($"Unexpected token! Expected {type} but got {t.Type}");

            return ReadToken();
        }

        private Token NewToken(TokenType t, string? value = null)
        {
            return new Token
            {
                Type = t,
                Value = value,
                StartPos = _reader.Column,
                StartLine = _reader.Line
            };
        }

        private void ReadWhitespace()
            => _reader.ReadWhile(x => char.IsWhiteSpace(x, 0));
    }
}
