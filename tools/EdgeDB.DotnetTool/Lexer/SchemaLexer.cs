namespace EdgeDB.DotnetTool.Lexer
{
    internal class SchemaLexer
    {
        private readonly Dictionary<string, TokenType> _tokens = new()
        {
            { "module", TokenType.Module },
            { "{", TokenType.BeginBrace },
            { "}", TokenType.EndBrace },
            { "required", TokenType.Required },
            { "property", TokenType.Property },
            { "constraint", TokenType.Constraint },
            { "->", TokenType.TypeArrow },
            { "multi", TokenType.Multi },
            { "single", TokenType.Single },
            { "link", TokenType.Link },
            { "abstract", TokenType.Abstract },
            { ";", TokenType.Semicolon },
            { ":=", TokenType.Assignment },
            { "on", TokenType.On },
            { "extending", TokenType.Extending },
            { ",", TokenType.Comma },
            { "(", TokenType.BeginParenthesis },
            { ")", TokenType.EndParenthesis },
            { "annotation", TokenType.Annotation },
            { "type", TokenType.Type },
            { "index", TokenType.Index },
            { "scalar", TokenType.Scalar },
            { "function", TokenType.Function },
        };

        private readonly SchemaBuffer _reader;
        private readonly Stack<Token> _stack;
        private Token? _previousToken;

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

            foreach (var item in elArr)
            {
                if (_tokens.TryGetValue(_reader.Peek(item), out var token))
                {
                    _reader.Read(item);

                    switch (token)
                    {
                        case TokenType.Assignment
                            or TokenType.TypeArrow
                            or TokenType.Extending
                            or TokenType.Constraint when _previousToken?.Type != TokenType.Abstract:
                            {
                                ReadWhitespace();
                                List<char> delimiters = new(new char[] { ';', '{' });
                                if (token == TokenType.Extending && _reader.Peek(4) != "enum")
                                    delimiters.Add(',');

                                var value = ReadValue(token == TokenType.Assignment, delimiters.ToArray());
                                return NewToken(token, value);
                            }

                        case TokenType.Module
                            or TokenType.Type
                            or TokenType.Property
                            or TokenType.Link:
                            {
                                ReadWhitespace();
                                var value = ReadValue(false, ';');
                                return NewToken(token, value);
                            }

                        case TokenType.Constraint when _previousToken?.Type == TokenType.Abstract:
                            {
                                // read value until the end of line or until ;
                                var value = ReadValue(true, ';');
                                // read the semi colon
                                if (_reader.Peek(1) == ";")
                                    value += _reader.Read(1);
                                return NewToken(TokenType.Constraint, value);
                            }
                        case TokenType.Function:
                            {
                                // read the value until the closing block
                                //var value = ReadValue(true, '{');

                                // read the name
                                var func = _reader.ReadUntil("(");
                                var funcParamsDepth = 1;
                                func += _reader.ReadWhile(x =>
                                {
                                    if (x == "(")
                                        funcParamsDepth++;

                                    if (x == ")")
                                        funcParamsDepth--;

                                    return funcParamsDepth > 0;
                                });
                                func += _reader.Read(1); // the closing prarms brace

                                // type arrow
                                func += ReadWhitespace();
                                func += _reader.Read(2);
                                func += ReadWhitespace();
                                // return type
                                func += ReadValue();
                                func += ReadWhitespace();
                                // check if its a bracket function
                                if (_reader.Peek(1) == "{")
                                {
                                    var depth = 1;
                                    _reader.Read(1);
                                    func += _reader.ReadWhile(x =>
                                    {
                                        if (x == "{")
                                            depth++;
                                        if (x == "}")
                                            depth--;

                                        return depth > 0;
                                    });

                                    func += _reader.Read(1);

                                    // add semi colon
                                    if (_reader.Peek(1) == ";")
                                        func += _reader.Read(1);

                                    return NewToken(TokenType.Function, func);
                                }
                                else if (_reader.Peek(5) == "using")
                                {
                                    // assume its a parentheses block?
                                    func += _reader.Read(5); // read using
                                    func += ReadWhitespace(); // read whitespace
                                    func += _reader.Read(1); // read starting parantheses
                                    var depth = 1;
                                    func += _reader.ReadWhile(x =>
                                    {
                                        if (x == "(")
                                            depth++;
                                        if (x == ")")
                                            depth--;

                                        return depth > 0;
                                    });

                                    func += _reader.Read(1); // read closing parantheses

                                    // add semi colon
                                    if (_reader.Peek(1) == ";")
                                        func += _reader.Read(1);

                                    return NewToken(TokenType.Function, func);
                                }

                                var a = _reader.Read(5);

                                return NewToken(TokenType.Function, "");
                            }
                        default:
                            return NewToken(token, null);

                    }

                }
            }

            var st = _reader.Peek(1);

            if (st == null || st.Length == 0)
            {
                // end of file
                return NewToken(TokenType.EndOfFile);
            }

            var c = _reader.Peek(1)[0];

            // identifier
            var val = ReadValue(false, ';');

            return NewToken(TokenType.Identifier, val);
        }

        private string ReadValue(bool ignoreSpace = false, params char[] delimiters)
        {
            // read untill we get to a quote, then read everything in the quote until whitespace
            bool isEscaped = false;
            return _reader.ReadWhile(x =>
            {
                if (x is "'" or "\"" or "[" or "]" or "`" or "<" or ">")
                    isEscaped = !isEscaped;

                return isEscaped || ((ignoreSpace || !char.IsWhiteSpace(x, 0)) && (delimiters.Length == 0 || !delimiters.Contains(x[0])));
            });
        }

        public Token Expect(TokenType type)
        {
            var t = PeekToken();

            if (type == TokenType.EndOfFile)
            {
                throw new EndOfStreamException("Unexpected end of file");
            }

            if (t.Type != type)
                throw new ArgumentException($"Unexpected token! Expected {type} but got {t.Type} at {t.StartLine}:{t.StartPos}", nameof(type));

            return ReadToken();
        }

        private Token NewToken(TokenType t, string? value = null)
        {
            var token = new Token
            {
                Type = t,
                Value = value,
                StartPos = _reader.Column,
                StartLine = _reader.Line
            };
            _previousToken = token;
            return token;
        }

        private string ReadWhitespace()
            => _reader.ReadWhile(x => char.IsWhiteSpace(x, 0));
    }
}
