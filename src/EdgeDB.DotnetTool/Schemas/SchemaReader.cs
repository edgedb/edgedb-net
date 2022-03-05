using EdgeDB.DotnetTool.Lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DotnetTool
{
    internal class SchemaReader
    {
        private readonly SchemaLexer _lexer;

        public SchemaReader(string schema)
        {
            _lexer = new SchemaLexer(schema);
        }

        public List<Module> Read()
        {
            List<Module> modules = new();
            while (_lexer.PeekToken().Type != Lexer.TokenType.EndOfFile)
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
                ret.Types.Add(ReadType());
            }

            ExpectEndOfBody();

            return ret;
        }

        private Type ReadType()
        {
            bool isScalar = false;
            bool isAbstract = false;

            var type = _lexer.ReadToken();

            if (type.Type == TokenType.Scalar)
            {
                isScalar = true;
                type = _lexer.ReadToken();
            }

            if(type.Type == TokenType.Abstract)
            {
                isAbstract = true;
                type = _lexer.ReadToken();
            }

            if (type.Type != TokenType.Type)
                throw new Exception($"Expected type but got {type.Type}");

            var ret = new Type
            {
                Name = type.Value,
                IsScalar = isScalar,
                IsAbstract = isAbstract
            };

            while (_lexer.PeekToken().Type != TokenType.BeginBrace)
            {
                var other = _lexer.ReadToken();

                switch (other.Type)
                {
                    case TokenType.Extending:
                        {
                            ret.IsAbstract = true;
                            ret.Extending = other.Value;
                        }
                        break;
                    default:
                        throw new Exception($"Unexpected token type {other.Type}");
                }
            }

            if(_lexer.PeekToken().Type == TokenType.BeginBrace)
            {
                _lexer.Expect(TokenType.BeginBrace);
                // read inner
                while(_lexer.PeekToken().Type != TokenType.EndBrace)
                {
                    ret.Properties.Add(ReadProperty());
                }

                ExpectEndOfBody();
            }

            return ret;
        }

        private Property ReadProperty()
        {
            var prop = new Property();

            while(_lexer.PeekToken().Type != TokenType.Property && _lexer.PeekToken().Type != TokenType.Link)
            {
                var token = _lexer.ReadToken();

                switch (token.Type)
                {
                    case TokenType.Required:
                        prop.Required = true;
                        break;
                    case TokenType.Single:
                        prop.Cardinality = PropertyCardinality.Single;
                        break;
                    case TokenType.Multi:
                        prop.Cardinality = PropertyCardinality.Multi;
                        break;
                    case TokenType.Abstract:
                        prop.IsAbstract = true;
                        break;
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
            else
                throw new Exception($"Expected type arrow or assignment but got {propertyDeclarerToken.Type}");


            // check for body
            if (_lexer.PeekToken().Type == TokenType.BeginBrace)
            {
                _lexer.Expect(TokenType.BeginBrace);

                while (_lexer.PeekToken().Type != TokenType.EndBrace)
                {
                    var peeked = _lexer.PeekToken();

                    if (peeked.Type == TokenType.Constraint)
                        prop.Constraints.Add(ReadConstraint());
                    else if (peeked.Type == TokenType.Annotation)
                        prop.Annotation = ReadAnnotation();
                    else if (peeked.Type == TokenType.Property && prop.IsLink)
                        prop.LinkProperties.Add(ReadProperty());
                    else
                        throw new Exception($"Unexpected token, expected constraint or annotation but got {peeked.Type}");
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

            if(_lexer.PeekToken().Type == TokenType.On && constraint.Value == "expression")
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

        private Annotation ReadAnnotation()
        {
            return new Annotation() { };
        }

        private void ExpectEndOfBody()
        {
            _lexer.Expect(TokenType.EndBrace);
            if (_lexer.PeekToken().Type == TokenType.Semicolon)
                _lexer.ReadToken();
        }
    }
}
