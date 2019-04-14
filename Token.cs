using System.Diagnostics;


namespace numerical_expression_parser
{
    [DebuggerDisplay("{DebuggerDisplay}")]
    internal struct Token
    {
        public TokenType Type { get; private set; }
        public double Value { get; private set; }

        public Token(TokenType tokenType, double value)
        {
            Type = tokenType;
            Value = value;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay
        {
            get
            {
                return $"TokenType = {Type.ToString()}; Value = {Value}";
            }
        }
    }
}
