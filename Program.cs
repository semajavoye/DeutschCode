using System;
using System.Collections.Generic;
using System.Data;

namespace DeutschCode
{
	class Program
	{
		// Dictionary to store functions
		static Dictionary<string, Action<List<object>>> functions = new Dictionary<string, Action<List<object>>>();

		// Dictionary to store variables
		static Dictionary<string, object> variables = new Dictionary<string, object>();

		// Debug flag
		static bool debugMode = false;

		static void Main(string[] args)
		{
			// Check if the user passed the correct arguments.
			if (args.Length < 2 || args[0] != "-code" || !args[1].EndsWith(".dc"))
			{
				Console.WriteLine("Usage: DeutschCode.exe -code code.dc [-d]");
				return;
			}

			// Check if debug flag (-d) is set
			if (args.Length == 3 && args[2] == "-d")
			{
				debugMode = true;
			}

			// Read the code from the file.
			string code = System.IO.File.ReadAllText(args[1]);

			// Interpret the code.
			Interpret(code);
		}

		// Method to interpret DeutschCode
		static void Interpret(string code)
		{
			// Split the code into lines.
			string[] lines = code.Split('\n');

			// Process each line of the code
			for (int i = 0; i < lines.Length; i++)
			{
				string line = lines[i].Trim();

				// Skip empty lines or lines with just "ende."
				if (string.IsNullOrWhiteSpace(line) || line == "ende.")
					continue;

				// Interpret "gib ... aus." statements (for printing values)
				if (line.StartsWith("gib") && line.EndsWith("aus."))
				{
					HandleGib(line);
				}
				// Interpret "sei ... vom Typ Zahl." or "sei ... = ..." (for declaring and assigning variables)
				else if (line.StartsWith("sei"))
				{
					HandleSei(line);
				}
				// Handle function definitions
				else if (line.StartsWith("funktion"))
				{
					// Function definition handling starts from here
					i = HandleFunctionDefinition(line, lines, i);
				}
				// Interpret function calls
				else if (IsFunctionCall(line))
				{
					if (debugMode) Console.WriteLine("Function call detected.");
					HandleFunctionCall(line);
				}
				else
				{
					Console.WriteLine($"Unrecognized command: {line}");
				}
			}
		}

		// Handle "gib ... aus." statement
		static void HandleGib(string line)
		{
			// Remove "gib" at the start and "aus." at the end
			string expression = line.Substring(3, line.Length - 7).Trim();

			// Check if the expression is a string (starts and ends with single quotes)
			if (expression.StartsWith("'") && expression.EndsWith("'"))
			{
				// Remove the quotes and print the string
				string output = expression.Substring(1, expression.Length - 2);
				Console.WriteLine(output);
			}
			else
			{
				// Replace variable names in the expression with their values
				expression = ReplaceVariables(expression);

				// Evaluate the expression (supports basic math operations)
				try
				{
					var result = EvaluateExpression(expression);
					Console.WriteLine(result);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error evaluating expression: {expression}. {ex.Message}");
				}
			}
		}

		// Handle "sei ... vom Typ Zahl." or "sei ... = ..." statement
		static void HandleSei(string line)
		{
			string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length >= 4 && parts[2] == "vom" && parts[3] == "Typ")
			{
				// Handle "sei summe vom Typ Zahl." (declare variable)
				string variableName = parts[1];
				string variableType = parts[4].Replace(".", ""); // e.g., "Zahl"

				if (variableType == "Zahl")
				{
					variables[variableName] = 0;  // Default value for numbers

					// Print debug info if debugMode is enabled
					if (debugMode)
					{
						Console.WriteLine($"Declared variable: {variableName} as {variableType}");
					}
				}
			}
			else if (line.Contains("="))
			{
				// Handle "sei summe = 5." (declare and assign variable)
				string[] assignmentParts = line.Split(new string[] { " = " }, StringSplitOptions.None);

				string variableName = assignmentParts[0].Split(' ')[1];  // e.g., "summe"
				string expression = assignmentParts[1].Replace(".", "").Trim(); // e.g., "5"

				var result = EvaluateExpression(ReplaceVariables(expression));
				variables[variableName] = result;

				// Print debug info if debugMode is enabled
				if (debugMode)
				{
					Console.WriteLine($"Assigned variable: {variableName} = {result}");
				}
			}
		}

		// Handle function calls
		static int HandleFunctionDefinition(string line, string[] lines, int currentIndex)
		{
			string functionName = line.Split(' ')[1].Split('(')[0].Trim();
			string parameterList = line.Substring(line.IndexOf('(') + 1);
			parameterList = parameterList.Substring(0, parameterList.IndexOf(')')).Trim();
			string[] parameters = parameterList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

			List<string> parameterNames = new List<string>();
			foreach (var param in parameters)
			{
				parameterNames.Add(param.Trim());
			}

			List<string> functionBody = new List<string>();
			currentIndex++;

			while (currentIndex < lines.Length && lines[currentIndex].Trim() != "ende.")
			{
				functionBody.Add(lines[currentIndex].Trim());
				currentIndex++;
			}

			functions[functionName] = (args) =>
			{
				Dictionary<string, object> localVariables = new Dictionary<string, object>();

				for (int i = 0; i < parameterNames.Count; i++)
				{
					localVariables[parameterNames[i]] = args[i];
				}

				foreach (var bodyLine in functionBody)
				{
					string replacedLine = bodyLine;
					foreach (var localVar in localVariables)
					{
						replacedLine = replacedLine.Replace(localVar.Key, localVar.Value.ToString());
					}
					Interpret(replacedLine);
				}
			};

			if (debugMode)
			{
				Console.WriteLine($"Function {functionName} defined with parameters: {string.Join(", ", parameterNames)}");
			}

			return currentIndex;
		}

		static void HandleFunctionCall(string line)
		{
			string functionName = line.Split('(')[0].Trim();
			if (functions.ContainsKey(functionName))
			{
				string argumentPart = line.Substring(line.IndexOf('(') + 1);
				argumentPart = argumentPart.TrimEnd(')'); // Remove the closing parenthesis
				List<object> args = new List<object>();

				if (!string.IsNullOrWhiteSpace(argumentPart))
				{
					// Parse the arguments if there are any
					string[] arguments = argumentPart.Split(',');
					foreach (var arg in arguments)
					{
						args.Add(EvaluateExpression(ReplaceVariables(arg.Trim()))); // Evaluate the argument
					}
				}

				functions[functionName](args); // Call the function with parsed arguments
				if (debugMode)
				{
					Console.WriteLine($"Called function: {functionName}");
				}
			}
			else
			{
				Console.WriteLine($"Function {functionName} not found.");
			}
		}

		// Helper to check if the line is a function call
		static bool IsFunctionCall(string line)
		{
			// Check if the line contains '(' and ends with ')'
			if (line.Contains("(") && line.EndsWith(")"))
			{
				string functionName = line.Split('(')[0].Trim();
				if (debugMode) Console.WriteLine($"Checking for function: {functionName}");
				return functions.ContainsKey(functionName); // Ensure that the function has been defined
			}
			return false;
		}

		// Replace variable names in an expression with their values
		static string ReplaceVariables(string expression)
		{
			foreach (var variable in variables)
			{
				expression = expression.Replace(variable.Key, variable.Value.ToString());
			}
			return expression;
		}

		// Utility method to evaluate mathematical expressions
		static object EvaluateExpression(string expression)
		{
			// Replace variable names in the expression with their values
			expression = ReplaceVariables(expression);

			// Log the expression being evaluated for debugging
			if (debugMode)
			{
				Console.WriteLine($"Evaluating expression: {expression}");
			}

			// Use DataTable to compute the mathematical expression
			var table = new DataTable();

			try
			{
				var result = table.Compute(expression, string.Empty);
				return result;
			}
			catch (Exception ex)
			{
				// Log the error for debugging
				Console.WriteLine($"Error evaluating expression '{expression}': {ex.Message}");
				throw; // Rethrow the exception after logging
			}
		}
	}
}