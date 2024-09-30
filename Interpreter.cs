using System;
using System.Collections.Generic;

namespace DeutschCode
{
    class Interpreter
    {
        private Dictionary<string, object> _variables = new Dictionary<string, object>();
        private Dictionary<string, UserDefinedFunction> _functions = new Dictionary<string, UserDefinedFunction>();

        public void Interpret(List<Token> tokens)
        {
            int position = 0;

            while (position < tokens.Count)
            {
                var currentToken = tokens[position];

                if (currentToken.Type == TokenType.Sei)
                {
                    position = HandleVariableDeclaration(tokens, position);
                }
                else if (currentToken.Type == TokenType.Gib)
                {
                    position = HandlePrintStatement(tokens, position);
                }
                else if (currentToken.Type == TokenType.Funktion)
                {
                    position = HandleFunctionDefinition(tokens, position);
                }
                else if (currentToken.Type == TokenType.Identifier && LookAhead(tokens, position + 1).Type == TokenType.LeftParen)
                {
                    position = HandleFunctionCall(tokens, position);
                }
                else
                {
                    throw new Exception($"Unrecognized statement starting with: {currentToken.Value}");
                }
            }
        }

        // Funktion definieren und im Dictionary speichern
        private int HandleFunctionDefinition(List<Token> tokens, int position)
        {
            position++; // Skip 'funktion'
            string functionName = tokens[position].Value;
            position++; // Skip function name

            // Collect function parameters
            Expect(TokenType.LeftParen, tokens[position]);
            position++; // Skip '('
            List<string> parameters = new List<string>();
            while (tokens[position].Type != TokenType.RightParen)
            {
                parameters.Add(tokens[position].Value);
                position++;
                if (tokens[position].Type == TokenType.Comma)
                {
                    position++; // Skip ','
                }
            }
            position++; // Skip ')'

            // Collect function body
            List<Token> functionBody = new List<Token>();
            while (tokens[position].Type != TokenType.Ende)
            {
                functionBody.Add(tokens[position]);
                position++;
            }
            position++; // Skip 'ende'

            // Save the function definition
            _functions[functionName] = new UserDefinedFunction(parameters, functionBody);

            return position;
        }

        private int HandleFunctionCall(List<Token> tokens, int position)
        {
            string functionName = tokens[position].Value;
            position += 2; // Skip function name and '('
            List<object> arguments = new List<object>();
            while (tokens[position].Type != TokenType.RightParen)
            {
                arguments.Add(EvaluateExpression(tokens, ref position));
                if (tokens[position].Type == TokenType.Comma)
                {
                    position++; // Skip ','
                }
            }
            position++; // Skip ')'

            if (_functions.ContainsKey(functionName))
            {
                ExecuteFunction(functionName, arguments);
            }
            else
            {
                throw new Exception($"Function '{functionName}' not defined.");
            }

            return position;
        }

        private void ExecuteFunction(string functionName, List<object> arguments)
        {
            var function = _functions[functionName];
            if (arguments.Count != function.Parameters.Count)
            {
                throw new Exception($"Function '{functionName}' expects {function.Parameters.Count} arguments, but got {arguments.Count}.");
            }

            // Create a local scope for function variables
            Dictionary<string, object> localVariables = new Dictionary<string, object>(_variables);
            for (int i = 0; i < function.Parameters.Count; i++)
            {
                localVariables[function.Parameters[i]] = arguments[i];
            }

            var interpreter = new Interpreter();
            interpreter._variables = localVariables;
            interpreter.Interpret(function.Body);
        }

        private int HandleVariableDeclaration(List<Token> tokens, int position)
        {
            position++; // Skip 'sei'
            string variableName = tokens[position].Value;
            position++; // Move to '=' or 'vom Typ'
            if (tokens[position].Type == TokenType.Equals)
            {
                position++; // Skip '='
                var expressionValue = EvaluateExpression(tokens, ref position);
                _variables[variableName] = expressionValue;
            }
            else if (tokens[position].Type == TokenType.Vom && tokens[position + 1].Type == TokenType.Typ)
            {
                position += 3; // Skip "vom Typ"
                _variables[variableName] = 0; // Default value
            }

            Expect(TokenType.Dot, tokens[position]);
            return position + 1; // Move past the dot
        }

        private int HandlePrintStatement(List<Token> tokens, int position)
        {
            position++; // Skip 'gib'
            var expressionValue = EvaluateExpression(tokens, ref position);
            Console.WriteLine(expressionValue);
            Expect(TokenType.Aus, tokens[position]);
            Expect(TokenType.Dot, tokens[position + 1]);
            return position + 2; // Move past 'aus' and dot
        }

        private object EvaluateExpression(List<Token> tokens, ref int position)
        {
            var left = EvaluateTerm(tokens, ref position);

            while (position < tokens.Count && (tokens[position].Type == TokenType.Plus || tokens[position].Type == TokenType.Minus))
            {
                var operatorToken = tokens[position];
                position++; // Skip the operator
                var right = EvaluateTerm(tokens, ref position);

                if (operatorToken.Type == TokenType.Plus)
                {
                    left = (Convert.ToDouble(left) + Convert.ToDouble(right));
                }
                else if (operatorToken.Type == TokenType.Minus)
                {
                    left = (Convert.ToDouble(left) - Convert.ToDouble(right));
                }
            }

            return left;
        }

        private object EvaluateTerm(List<Token> tokens, ref int position)
        {
            var left = EvaluateFactor(tokens, ref position);

            while (position < tokens.Count && (tokens[position].Type == TokenType.Asterisk || tokens[position].Type == TokenType.Slash))
            {
                var operatorToken = tokens[position];
                position++; // Skip the operator
                var right = EvaluateFactor(tokens, ref position);

                if (operatorToken.Type == TokenType.Asterisk)
                {
                    left = (Convert.ToDouble(left) * Convert.ToDouble(right));
                }
                else if (operatorToken.Type == TokenType.Slash)
                {
                    left = (Convert.ToDouble(left) / Convert.ToDouble(right));
                }
            }

            return left;
        }

        private object EvaluateFactor(List<Token> tokens, ref int position)
        {
            var currentToken = tokens[position];

            if (currentToken.Type == TokenType.Number)
            {
                position++;
                return double.Parse(currentToken.Value);
            }
            else if (currentToken.Type == TokenType.Identifier)
            {
                position++;
                if (_variables.ContainsKey(currentToken.Value))
                {
                    return _variables[currentToken.Value];
                }
                else
                {
                    throw new Exception($"Variable '{currentToken.Value}' not found.");
                }
            }
            else
            {
                throw new Exception($"Unexpected token: {currentToken.Value}");
            }
        }

        private void Expect(TokenType expectedType, Token token)
        {
            if (token.Type != expectedType)
            {
                throw new Exception($"Expected token of type {expectedType}, but found {token.Type} ({token.Value})");
            }
        }

        private Token LookAhead(List<Token> tokens, int position)
        {
            if (position < tokens.Count)
            {
                return tokens[position];
            }
            return new Token(TokenType.EOF, "");
        }
    }

    class UserDefinedFunction
    {
        public List<string> Parameters { get; private set; }
        public List<Token> Body { get; private set; }

        public UserDefinedFunction(List<string> parameters, List<Token> body)
        {
            Parameters = parameters;
            Body = body;
        }
    }
}
