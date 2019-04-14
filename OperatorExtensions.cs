using System;


namespace numerical_expression_parser
{
    internal static class OperatorExtensions
    {
        public static bool IsArithmetic(this Operator @operator)
        {
            return IsBinary(@operator) || IsUnary(@operator);
        }

        public static bool IsBinary(this Operator @operator)
        {
            switch (@operator)
            {
                case Operator.Add:
                case Operator.Subtract:
                case Operator.Multiply:
                case Operator.Divide:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsUnary(this Operator @operator)
        {
            switch (@operator)
            {
                case Operator.UnaryPlus:
                case Operator.UnaryMinus:
                    return true;
                default:
                    return false;
            }
        }

        public static Operator ToUnary(this Operator @operator)
        {
            switch (@operator)
            {
                case Operator.Add:
                    return Operator.UnaryPlus;
                case Operator.Subtract:
                    return Operator.UnaryMinus;
                default:
                    throw new InvalidOperationException($"Cannot convert operator {@operator} to unary. Only binary plus and binary minus operators are supported.");
            }
        }
    }
}
