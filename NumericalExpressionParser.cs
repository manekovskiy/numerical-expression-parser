using System;
using System.Collections.Generic;
using System.IO;


namespace numerical_expression_parser
{
    public class NumericalExpressionParser
    {
        public static double Evaluate(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentNullException(nameof(input));
            }

            var queue = ConvertToReversePolishNotation(input);
            var intermediateResults = new Stack<double>();

            while (queue.Count > 0)
            {
                var operatorOrOperand = queue.Dequeue();

                switch (operatorOrOperand)
                {
                    case double operand:
                        intermediateResults.Push(operand);
                        break;
                    case Operator @operator:
                        switch (@operator)
                        {
                            case Operator binaryOperator when @operator.IsBinary():
                                if (intermediateResults.Count < 2)
                                {
                                    throw new InvalidOperationException($"Expected to have at least two operands to perform {@operator} operation but got {intermediateResults.Count}.");
                                }

                                var right = intermediateResults.Pop();
                                var left = intermediateResults.Pop();
                                switch (@operator)
                                {
                                    case Operator.Add:
                                        intermediateResults.Push(left + right);
                                        break;
                                    case Operator.Subtract:
                                        intermediateResults.Push(left - right);
                                        break;
                                    case Operator.Multiply:
                                        intermediateResults.Push(left * right);
                                        break;
                                    case Operator.Divide:
                                        intermediateResults.Push(left / right);
                                        break;
                                    default:
                                        throw new InvalidOperationException($"Binary operator \"{binaryOperator}\" is not supported.");
                                }
                                break;
                            case Operator unaryOperator when @operator.IsUnary():
                                var intermediateResult = intermediateResults.Pop();
                                switch (unaryOperator)
                                {
                                    case Operator.UnaryPlus:
                                        intermediateResults.Push(intermediateResult);
                                        break;
                                    case Operator.UnaryMinus:
                                        intermediateResults.Push(-1 * intermediateResult);
                                        break;
                                    default:
                                        throw new InvalidOperationException($"Unary operator \"{unaryOperator}\" is not supported.");
                                }
                                break;
                        }
                        break;
                    default:
                        throw new InvalidOperationException($"Element of type \"{operatorOrOperand.GetType()}\" is not supported.");
                }
            }

            // After all characters are scanned, we will have only one element in the stack. Return topStack.
            if (intermediateResults.Count > 1)
                throw new Exception("More than one result on the stack.");
            else if (intermediateResults.Count < 1)
                throw new Exception("No results on the stack.");

            return intermediateResults.Pop();
        }

        internal static Queue<object> ConvertToReversePolishNotation(string input)
        {
            var output = new Queue<object>();
            var operators = new Stack<Operator>();

            var isFirstToken = true;
            Token previousToken = default(Token);

            using (var tokenizer = new Tokenizer(input))
            {
                while (tokenizer.MoveNext())
                {
                    var token = tokenizer.Current;
                    switch (token.Type)
                    {
                        // if the token is a number, then: push it to the output queue.
                        case TokenType.Constant:
                            output.Enqueue(token.Value);
                            break;
                        // if the token is a left paren(i.e. "("), then: push it onto the operator stack.
                        case TokenType.OpeningParenthesis:
                            operators.Push(token.Type.ToOperator());
                            break;
                        // if the token is a right paren(i.e. ")"), then:
                        case TokenType.ClosingParenthesis:
                            // while the operator at the top of the operator stack is not a left paren:
                            //   pop the operator from the operator stack onto the output queue.
                            var lastOperator = operators.Peek();
                            while (lastOperator != Operator.OpeningParenthesis)
                            {
                                output.Enqueue(operators.Pop());
                                lastOperator = operators.Peek();
                            }

                            // if the stack runs out without finding a left paren, then there are mismatched parentheses.
                            if (operators.Count == 0)
                                throw new InvalidDataException("Mismatched parentheses found.");

                            // if there is a left paren at the top of the operator stack, then:
                            //   pop the operator from the operator stack and discard it
                            operators.Pop();
                            break;
                        // if the token is an operator, then:
                        case TokenType operatorToken when token.Type.IsArithmeticOperator():
                            var currentOperator = operatorToken.ToOperator();

                            // Decide whether we need to convert current operator from binary to unary
                            if (currentOperator == Operator.Add || currentOperator == Operator.Subtract)
                            {
                                // 1. This is a first token we are processing => convert to unary
                                if (isFirstToken)
                                {
                                    currentOperator = currentOperator.ToUnary();
                                }
                                else if (operators.Count > 0)
                                {
                                    lastOperator = operators.Peek();

                                    // 2. We need to flip operator type to unary if all of the following conditions are met:
                                    //   a) Last token type was not the closing parenthesis AND was not the constant
                                    //   b) Operator on top of operators stack is opening parenthesis OR binary arithmetic operator
                                    if (previousToken.Type != TokenType.ClosingParenthesis && previousToken.Type != TokenType.Constant
                                        && (lastOperator.IsBinary() || lastOperator == Operator.OpeningParenthesis))
                                    {
                                        currentOperator = currentOperator.ToUnary();
                                    }
                                }
                            }

                            // 3. This is not the first operator which means that we need to figure where current operator belongs
                            if (operators.Count > 0)
                            {
                                lastOperator = operators.Peek();

                                // while there is an operator at the top of the operator stack with greater precedence
                                //   pop operators from the operator stack onto the output queue.
                                while (lastOperator.IsArithmetic() && GetOperatorPrecendence(currentOperator) <= GetOperatorPrecendence(lastOperator))
                                {
                                    output.Enqueue(operators.Pop());

                                    if (operators.Count == 0)
                                    {
                                        break; 
                                    }
                                    lastOperator = operators.Peek();
                                }
                            }

                            // push current operator onto the operator stack.
                            operators.Push(currentOperator);
                            break;
                        default:
                            throw new InvalidDataException($"Unexpected token: Type={token.Type}; Value={token.Value}. Provided input is in an ivalid format.");
                    }

                    previousToken = token;
                    isFirstToken = false;
                }

                // while there are still operator tokens on the stack:
                while (operators.Count > 0)
                {
                    // pop the operator from the operator stack
                    var lastOperator = operators.Pop();

                    // if the operator token on the top of the stack is a paren, then there are mismatched parentheses.
                    if (lastOperator == Operator.OpeningParenthesis || lastOperator == Operator.ClosingParenthesis)
                    {
                        throw new InvalidDataException("Mismatched parentheses found.");
                    }
                    else
                    {
                        // push operator onto the output queue
                        output.Enqueue(lastOperator);
                    }
                }
            }

            return output;
        }

        private static int GetOperatorPrecendence(Operator @operator)
        {
            switch (@operator)
            {
                case Operator.Add:
                case Operator.Subtract:
                    return 1;
                case Operator.Multiply:
                case Operator.Divide:
                    return 2;
                case Operator.UnaryMinus:
                case Operator.UnaryPlus:
                    return 3;
            }

            throw new InvalidOperationException($"\"{@operator}\" is not an arithmetic operation. Only arithmetic operations can have precendence value defined.");
        }
    }
}
