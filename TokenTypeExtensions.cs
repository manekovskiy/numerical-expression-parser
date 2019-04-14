using System;


namespace numerical_expression_parser
{
    internal static class TokenTypeExtensions
    {
        public static bool IsArithmeticOperator(this TokenType tokenType)
        {
            switch (tokenType)
            {
                case TokenType.Add:
                case TokenType.Subtract:
                case TokenType.Multiply:
                case TokenType.Divide:
                    return true;
            }

            return false;
        }

        public static Operator ToOperator(this TokenType tokenType)
        {
            switch (tokenType)
            {
                case TokenType.Constant:
                    return Operator.Constant;
                case TokenType.Add:
                    return Operator.Add;
                case TokenType.Subtract:
                    return Operator.Subtract;
                case TokenType.Multiply:
                    return Operator.Multiply;
                case TokenType.Divide:
                    return Operator.Divide;
                case TokenType.OpeningParenthesis:
                    return Operator.OpeningParenthesis;
                case TokenType.ClosingParenthesis:
                    return Operator.ClosingParenthesis;
            }

            throw new InvalidOperationException($"Cannot convert TokenType \"{tokenType.ToString()}\" to ExpressionType.");
        }
    }
}
