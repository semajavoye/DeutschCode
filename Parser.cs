using System;
using System.Collections.Generic;

namespace DeutschCode
{
    class Parser
    {
        private List<Token> _tokens;
        private int _position = 0;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        private Token CurrentToken => _tokens[_position];

        public void Parse()
        {
            while (CurrentToken.Type != TokenType.EOF)
            {
                ParseStatement();
            }
        }

        private void ParseStatement()
        {
            if (CurrentToken.Type == TokenType.Gib)
            {
                ParsePrintStatement();
            }
            else if (CurrentToken.Type == TokenType.Sei)
            {
                ParseVariableDeclaration();
            }
            else if (CurrentToken.Type == TokenType.Funktion)
            {
                ParseFunctionDeclaration();
            }
            else if (CurrentToken.Type == TokenType.Identifier)
            {
                ParseFunctionCall();
            }
            else
            {
                throw new Exception($"Unexpected token: {CurrentToken}");
            }

            // Hier muss der Punkt (.) zur Trennung von Anweisungen behandelt werden
            if (CurrentToken.Type == TokenType.Dot)
            {
                Expect(TokenType.Dot); // Punkt als Statement-Abschluss erwarten
            }
        }

        private void ParsePrintStatement()
        {
            _position++; // Skip 'gib'
            var expression = ParseExpression();
            Expect(TokenType.Aus); // Expect 'aus'
            Console.WriteLine($"Print: {expression}");
        }

        private void ParseVariableDeclaration()
        {
            _position++; // Skip 'sei'
            string variableName = Expect(TokenType.Identifier).Value;
            Expect(TokenType.Equals); // Expect '='
            var value = ParseExpression();
            Console.WriteLine($"Declare variable: {variableName} = {value}");
        }

		private void ParseFunctionDeclaration()
		{
			_position++; // Skip 'funktion'
			string functionName = Expect(TokenType.Identifier).Value;
			Expect(TokenType.LeftParen); // Expect '('
			var parameters = ParseFunctionParameters();
			Expect(TokenType.RightParen); // Expect ')'

			// Here you need to parse the function body.
			while (CurrentToken.Type != TokenType.Ende && CurrentToken.Type != TokenType.EOF)
			{
				ParseStatement();
			}

			Expect(TokenType.Ende); // Expect 'ende'
			Console.WriteLine($"Declare function: {functionName} with parameters: {string.Join(", ", parameters)}");
		}


        private List<string> ParseFunctionParameters()
        {
            List<string> parameters = new List<string>();
            if (CurrentToken.Type != TokenType.RightParen)
            {
                parameters.Add(Expect(TokenType.Identifier).Value);
                while (CurrentToken.Type == TokenType.Comma)
                {
                    _position++; // Skip ','
                    parameters.Add(Expect(TokenType.Identifier).Value);
                }
            }
            return parameters;
        }

        private void ParseFunctionCall()
        {
            string functionName = Expect(TokenType.Identifier).Value;
            Expect(TokenType.LeftParen); // Expect '('
            var arguments = ParseArguments();
            Expect(TokenType.RightParen); // Expect ')'
            Console.WriteLine($"Call function: {functionName} with args: {string.Join(", ", arguments)}");
        }

        private List<string> ParseArguments()
        {
            List<string> args = new List<string>();
            if (CurrentToken.Type != TokenType.RightParen)
            {
                args.Add(ParseExpression());
                while (CurrentToken.Type == TokenType.Comma)
                {
                    _position++; // Skip ','
                    args.Add(ParseExpression());
                }
            }
            return args;
        }

        private string ParseExpression()
        {
            var left = ParseTerm();

            while (CurrentToken.Type == TokenType.Plus || CurrentToken.Type == TokenType.Minus)
            {
                var operatorToken = CurrentToken;
                _position++; // Skip operator
                var right = ParseTerm();
                left = $"{left} {operatorToken.Value} {right}";
            }

            return left;
        }

        private string ParseTerm()
        {
            var left = ParseFactor();

            while (CurrentToken.Type == TokenType.Asterisk || CurrentToken.Type == TokenType.Slash)
            {
                var operatorToken = CurrentToken;
                _position++; // Skip operator
                var right = ParseFactor();
                left = $"{left} {operatorToken.Value} {right}";
            }

            return left;
        }

        private string ParseFactor()
        {
            if (CurrentToken.Type == TokenType.Number)
            {
                return Expect(TokenType.Number).Value;
            }
            else if (CurrentToken.Type == TokenType.Identifier)
            {
                return Expect(TokenType.Identifier).Value;
            }
            else
            {
                throw new Exception($"Unexpected token in expression: {CurrentToken}");
            }
        }

        private Token Expect(TokenType type)
        {
            if (CurrentToken.Type == type)
            {
                var token = CurrentToken;
                _position++;
                return token;
            }
            else
            {
                throw new Exception($"Expected {type}, but found {CurrentToken.Type}");
            }
        }
    }
}
