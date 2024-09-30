using System;
using System.Collections.Generic;
using System.IO;

namespace DeutschCode
{
    class Program
    {
        static void Main(string[] args)
        {
            // Check if the user passed the correct arguments.
            if (args.Length < 2 || args[0] != "-code" || !args[1].EndsWith(".dc"))
            {
                Console.WriteLine("Usage: DeutschCode.exe -code code.dc [-d]");
                return;
            }

            // Read the code from the file.
            string code = File.ReadAllText(args[1]);

            // Tokenize the code
            Lexer lexer = new Lexer(code);
            List<Token> tokens = lexer.Tokenize();

            // Parse the tokens
            Parser parser = new Parser(tokens);
            parser.Parse();

            // Interpretation logic is embedded in the parser as it processes statements.
        }
    }
}