namespace DeutschCode
{
    enum TokenType
    {
        Gib,        // Keyword 'gib'
        Aus,        // Keyword 'aus'
        Sei,        // Keyword 'sei'
        Vom,        // Keyword 'vom'
        Typ,        // Keyword 'Typ'
        Zahl,       // Typ 'Zahl'
        Identifier, // Variablen- oder Funktionsname
        Number,     // Zahl
        String,     // Zeichenketten
        Equals,     // '='
        LeftParen,  // '('
        RightParen, // ')'
        Comma,      // ','
        Dot,        // '.'
        Plus,       // '+'
        Minus,      // '-'
        Asterisk,   // '*'
        Slash,      // '/'
        Ende,        // 'ende'
        Funktion,   // Keyword 'funktion'  <- Hinzugefügt
        EOF         // End of File
    }

    class Token
    {
        public TokenType Type { get; }
        public string Value { get; }

        public Token(TokenType type, string value)
        {
            Type = type;
            Value = value;
        }

        public override string ToString()
        {
            return $"{Type}({Value})";
        }
    }
}
