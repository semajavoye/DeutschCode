using System;
using System.Collections.Generic;
using System.Data;

namespace DeutschCode
{
    class Program
    {
        // Dictionary to store variables (their names and values)
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
            foreach (string line in lines)
            {
                // Trim the line to remove unnecessary spaces
                string trimmedLine = line.Trim();

                // Skip empty lines
                if (string.IsNullOrWhiteSpace(trimmedLine))
                    continue;

                // Interpret "gib ... aus." statements (for printing values)
                if (trimmedLine.StartsWith("gib") && trimmedLine.EndsWith("aus."))
                {
                    HandleGib(trimmedLine);
                }
                // Interpret "sei ... vom Typ Zahl." or "sei ... = ..." (for declaring and assigning variables)
                else if (trimmedLine.StartsWith("sei"))
                {
                    HandleSei(trimmedLine);
                }
                else
                {
                    Console.WriteLine($"Unrecognized command: {trimmedLine}");
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
            // Example: "sei summe vom Typ Zahl."
            // Example: "sei summe = 5."

            string[] parts = line.Split(' ');

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
            // Use DataTable to compute the mathematical expression
            var table = new DataTable();
            var result = table.Compute(expression, string.Empty);
            return result;
        }
    }
}
