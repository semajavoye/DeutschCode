using System;
using System.Collections.Generic;

namespace DeutschCode
{
    class Lexer
    {
        private string _code;
        private int _position;
        private Dictionary<string, TokenType> _keywords = new Dictionary<string, TokenType>
        {
            {"gib", TokenType.Gib},
            {"aus", TokenType.Aus},
            {"sei", TokenType.Sei},
            {"vom", TokenType.Vom},
            {"Typ", TokenType.Typ},
            {"Zahl", TokenType.Zahl},
            {"ende", TokenType.Ende},
            {"funktion", TokenType.Funktion}  // Hinzugefügt für das Schlüsselwort 'funktion'
        };

        public Lexer(string code)
        {
            _code = code;
        }

        public List<Token> Tokenize()
        {
            List<Token> tokens = new List<Token>();
            while (_position < _code.Length)
            {
                char current = _code[_position];

                if (char.IsWhiteSpace(current))
                {
                    _position++;
                }
                else if (char.IsLetter(current))
                {
                    tokens.Add(ReadIdentifierOrKeyword());
                }
                else if (char.IsDigit(current))
                {
                    tokens.Add(ReadNumber());
                }
                else if (current == '=')
                {
                    tokens.Add(new Token(TokenType.Equals, "="));
                    _position++;
                }
                else if (current == '(')
                {
                    tokens.Add(new Token(TokenType.LeftParen, "("));
                    _position++;
                }
                else if (current == ')')
                {
                    tokens.Add(new Token(TokenType.RightParen, ")"));
                    _position++;
                }
                else if (current == ',')
                {
                    tokens.Add(new Token(TokenType.Comma, ","));
                    _position++;
                }
                else if (current == '.')
                {
                    tokens.Add(new Token(TokenType.Dot, "."));  // Punkt als eigenes Token behandeln
                    _position++;
                }
                else if (current == '+')
                {
                    tokens.Add(new Token(TokenType.Plus, "+"));  // Pluszeichen als Token hinzufügen
                    _position++;
                }
                else if (current == '-')
                {
                    tokens.Add(new Token(TokenType.Minus, "-"));  // Minuszeichen als Token hinzufügen
                    _position++;
                }
                else if (current == '*')
                {
                    tokens.Add(new Token(TokenType.Asterisk, "*"));  // Multiplikationszeichen als Token hinzufügen
                    _position++;
                }
                else if (current == '/')
                {
                    tokens.Add(new Token(TokenType.Slash, "/"));  // Divisionszeichen als Token hinzufügen
                    _position++;
                }
                else if (current == '\'')
                {
                    tokens.Add(ReadString());
                }
                else
                {
                    throw new Exception($"Unexpected character: {current}");
                }
            }
            tokens.Add(new Token(TokenType.EOF, "")); // EOF am Ende der Tokenliste hinzufügen
            return tokens;
        }

        private Token ReadIdentifierOrKeyword()
        {
            int start = _position;
            while (_position < _code.Length && char.IsLetter(_code[_position]))
            {
                _position++;
            }

            string word = _code.Substring(start, _position - start);

            if (_keywords.ContainsKey(word))
            {
                return new Token(_keywords[word], word);
            }
            else
            {
                return new Token(TokenType.Identifier, word);
            }
        }

        private Token ReadNumber()
        {
            int start = _position;
            while (_position < _code.Length && char.IsDigit(_code[_position]))
            {
                _position++;
            }

            string number = _code.Substring(start, _position - start);
            return new Token(TokenType.Number, number);
        }

        private Token ReadString()
        {
            _position++; // Skip opening '
            int start = _position;

            while (_position < _code.Length && _code[_position] != '\'')
            {
                _position++;
            }

            if (_position >= _code.Length)
            {
                throw new Exception("Unterminated string");
            }

            string str = _code.Substring(start, _position - start);
            _position++; // Skip closing '
            return new Token(TokenType.String, str);
        }
    }
}
