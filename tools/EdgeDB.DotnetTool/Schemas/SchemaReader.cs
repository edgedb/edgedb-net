using EdgeDB.DotnetTool.Lexer;

namespace EdgeDB.DotnetTool
{
    internal class SchemaReader
    {
        private readonly SchemaLexer _lexer;

        public SchemaReader(string schema)
        {
            _lexer = new(schema);
        }

        public List<Module> Read()
        {
            List<Module> modules = new();
            while (_lexer.PeekToken().Type != TokenType.EndOfFile)
            {
                modules.Add(ReadModule());
            }
            return modules;
        }

        private Module ReadModule()
        {
            var module = _lexer.Expect(TokenType.Module);
            _lexer.Expect(TokenType.BeginBrace);

            var ret = new Module
            {
                Name = module.Value
            };

            while (_lexer.PeekToken().Type != TokenType.EndBrace)
            {
                var type = ReadModuleType(ret);
                if (type != null)
                    ret.Types.Add(type);
            }

            ExpectEndOfBody();

            return ret;
        }

        private Type? ReadModuleType(Module module)
        {
            bool isScalar = false;
            bool isAbstract = false;

            var type = _lexer.ReadToken();

            if (type.Type == TokenType.Scalar)
            {
                isScalar = true;
                type = _lexer.ReadToken();
            }

            if (type.Type == TokenType.Abstract)
            {
                isAbstract = true;
                type = _lexer.ReadToken();
            }

            switch (type.Type)
            {
                case TokenType.Type:
                    return ReadType(module, type, isScalar, isAbstract);
                case TokenType.Annotation when isAbstract:
                    {
                        // skip, useless 
                        // Colin 03-04-2022
                        _ = _lexer.ReadToken(); // read value
                        _ = _lexer.ReadToken(); // skip semicolon
                        return null;
                    }
                case TokenType.Constraint when isAbstract:
                    {
                        // store somewhere
                        return null;
                    }
                case TokenType.Function:
                    {
                        // store somewhere
                        return null;
                    }
                case TokenType.Link:
                    {
                        return ReadType(module, type, isScalar, isAbstract, true);
                    }
                case TokenType.EndOfFile:
                    {
                        // shouldn't happen?
                        return null;
                    }
                default:
                    {
                        // skip?

                        return null;
                    }
            }
        }

        private Type? ReadType(Module module, Token type, bool isScalar, bool isAbstract, bool isLink = false)
        {
            var ret = new Type
            {
                Parent = module,
                Name = type.Value,
                IsScalar = isScalar,
                IsAbstract = isAbstract,
                IsLink = isLink,
            };

            if (_lexer.PeekToken().Type == TokenType.Semicolon)
            {
                // empty shape?
                _lexer.Expect(TokenType.Semicolon);
                return null;
            }

            while (_lexer.PeekToken().Type != TokenType.BeginBrace)
            {
                var other = _lexer.ReadToken();

                switch (other.Type)
                {
                    case TokenType.Extending:
                        {
                            if (other.Value!.StartsWith("enum<"))
                            {
                                var t = new List<Token>();
                                while (_lexer.PeekToken().Type == TokenType.Identifier)
                                {
                                    t.Add(_lexer.ReadToken());
                                }

                                ret.Extending = $"{other.Value} {string.Join(" ", t.Select(x => x.Value))}";
                                // read semi colon
                                if (_lexer.PeekToken().Type == TokenType.Semicolon)
                                    _lexer.ReadToken();


                                goto endLabel;
                            }
                            else
                            {
                                ret.IsAbstract = true;
                                ret.Extending = other.Value;

                                // read semi colon
                                if (_lexer.PeekToken().Type == TokenType.Semicolon)
                                    _lexer.ReadToken();

                                if (_lexer.PeekToken().Type == TokenType.Comma)
                                {
                                    while (_lexer.PeekToken().Type != TokenType.BeginBrace)
                                    {
                                        _lexer.ReadToken(); // comma
                                        var a = _lexer.ReadToken();
                                        ret.Extending += $", {a.Value}";
                                    }
                                }

                                goto endLabel;
                            }
                        }
                    default:
                        throw new FormatException($"Unexpected token type {other.Type} at {other.StartLine}:{other.StartPos}");
                }
            }

        endLabel:

            if (_lexer.PeekToken().Type == TokenType.BeginBrace)
            {
                _lexer.Expect(TokenType.BeginBrace);
                // read inner
                while (_lexer.PeekToken().Type != TokenType.EndBrace)
                {
                    ret.Properties.Add(ReadProperty(ret));
                }

                ExpectEndOfBody();
            }

            return ret;
        }

        private Property ReadProperty(Type type)
        {
            var prop = new Property() { Parent = type };

            while (_lexer.PeekToken().Type is not TokenType.Property and not TokenType.Link)
            {
                var token = _lexer.ReadToken();

                switch (token.Type)
                {
                    case TokenType.Required:
                        prop.Required = true;
                        break;
                    case TokenType.Single:
                        prop.Cardinality = PropertyCardinality.One;
                        break;
                    case TokenType.Multi:
                        prop.Cardinality = PropertyCardinality.Multi;
                        break;
                    case TokenType.Abstract:
                        prop.IsAbstract = true;
                        break;
                    case TokenType.Constraint:
                        prop.IsStrictlyConstraint = true;
                        prop.Constraints.Add(new Constraint
                        {
                            Value = token.Value,
                            IsExpression = false,
                        });

                        if (_lexer.PeekToken().Type == TokenType.Semicolon)
                            _lexer.ReadToken();

                        return prop;
                }
            }

            var propDeclaration = _lexer.ReadToken();
            prop.Name = propDeclaration.Value;
            prop.IsLink = propDeclaration.Type == TokenType.Link;

            // read type
            var propertyDeclarerToken = _lexer.ReadToken();
            if (propertyDeclarerToken.Type == TokenType.TypeArrow)
            {
                prop.Type = propertyDeclarerToken.Value;
            }
            else if (propertyDeclarerToken.Type == TokenType.Assignment)
            {
                prop.IsComputed = true;
                prop.ComputedValue = propertyDeclarerToken.Value;
            }
            else if (propertyDeclarerToken.Type == TokenType.Extending)
            {
                // read the extending value until type assignment
                var typeAssignment = _lexer.Expect(TokenType.TypeArrow);
                prop.Extending = propertyDeclarerToken.Value;
                prop.Type = typeAssignment.Value;
            }
            else
                throw new FormatException($"Expected type arrow or assignment but got {propertyDeclarerToken.Type} at {propertyDeclarerToken.StartLine}:{propertyDeclarerToken.StartPos}");


            // check for body
            if (_lexer.PeekToken().Type == TokenType.BeginBrace)
            {
                _lexer.Expect(TokenType.BeginBrace);

                while (_lexer.PeekToken().Type != TokenType.EndBrace)
                {
                    var peeked = _lexer.PeekToken();

                    switch (peeked.Type)
                    {
                        case TokenType.Constraint:
                            prop.Constraints.Add(ReadConstraint());
                            break;
                        case TokenType.Annotation:
                            prop.Annotation = ReadAnnotation();
                            break;
                        case TokenType.Property when prop.IsLink:
                            prop.LinkProperties.Add(ReadProperty(type));
                            break;
                        case TokenType.Identifier when peeked.Value == "default":
                            {
                                _lexer.Expect(TokenType.Identifier);
                                var assignment = _lexer.Expect(TokenType.Assignment);

                                if (_lexer.PeekToken().Type == TokenType.Semicolon)
                                    _lexer.ReadToken();

                                prop.DefaultValue = assignment.Value;
                            }
                            break;

                        default:
                            throw new FormatException($"Unexpected token, expected constraint or annotation but got {peeked.Type} at {peeked.StartLine}:{peeked.StartPos}");
                    }
                }

                ExpectEndOfBody();
            }
            else
                _lexer.Expect(TokenType.Semicolon);

            return prop;
        }

        private Constraint ReadConstraint()
        {
            var constraint = _lexer.Expect(TokenType.Constraint);

            if (_lexer.PeekToken().Type == TokenType.On && constraint.Value == "expression")
            {
                //var val = "";
                _lexer.Expect(TokenType.BeginParenthesis);

                return new Constraint
                {
                    IsExpression = true,
                };
            }

            if (_lexer.PeekToken().Type == TokenType.Semicolon)
                _lexer.ReadToken();

            return new Constraint
            {
                IsExpression = false,
                Value = constraint.Value
            };
        }

        private static Annotation ReadAnnotation() => new() { };

        private void ExpectEndOfBody()
        {
            _lexer.Expect(TokenType.EndBrace);
            if (_lexer.PeekToken().Type == TokenType.Semicolon)
                _lexer.ReadToken();
        }
    }
}
