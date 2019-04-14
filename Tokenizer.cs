using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;


namespace numerical_expression_parser
{
    internal class Tokenizer : IEnumerator<Token>
    {
        private bool disposed;
        private TextReader inputReader;

        object IEnumerator.Current => Current;
        public Token Current { get; private set; }

        public Tokenizer(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentNullException(nameof(input));
            }

            inputReader = new StringReader(input);
        }

        public bool MoveNext()
        {
            int characterCode;
            char character;
            do
            {
                characterCode = inputReader.Read();
                if (characterCode < 0)
                {
                    return false;
                }
                character = (char)characterCode;
            } while (char.IsWhiteSpace(character));

            Token token;
            if (TryReadConstant(character, out token))
            {
                Current = token;
            }
            else if (TryReadOperator(character, out token))
            {
                Current = token;
            }
            else
            {
                throw new InvalidDataException("Input string is in invalid format");
            }

            return characterCode != -1;
        }

        private bool TryReadConstant(char character, out Token token)
        {
            if (!char.IsDigit(character) && character != '.')
            {
                token = new Token(TokenType.Unknown, double.NaN);
                return false;
            }

            var numberBuilder = new StringBuilder();
            var decimalSeparatorAlreadyAdded = false;

            while (true)
            {
                numberBuilder.Append(character);
                decimalSeparatorAlreadyAdded = character == '.';

                var characterCode = inputReader.Peek();
                if (characterCode < 0)
                {
                    break;
                }
                character = (char)characterCode;

                if (char.IsDigit(character) || (character == '.' && !decimalSeparatorAlreadyAdded))
                {
                    inputReader.Read();
                }
                else
                {
                    break;
                }
            }

            var value = double.Parse(numberBuilder.ToString(), CultureInfo.InvariantCulture);
            token = new Token(TokenType.Constant, value);

            return true;
        }

        private bool TryReadOperator(char character, out Token token)
        {
            var tokenType = TokenType.Unknown;
            switch (character)
            {
                case '+':
                    tokenType = TokenType.Add;
                    break;
                case '-':
                    tokenType = TokenType.Subtract;
                    break;
                case '*':
                    tokenType = TokenType.Multiply;
                    break;
                case '/':
                    tokenType = TokenType.Divide;
                    break;
                case '(':
                    tokenType = TokenType.OpeningParenthesis;
                    break;
                case ')':
                    tokenType = TokenType.ClosingParenthesis;
                    break;
            }

            token = new Token(tokenType, double.NaN);
            return tokenType != TokenType.Unknown;
        }

        public void Reset()
        {
            throw new InvalidOperationException("Reset is not supported");
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            inputReader.Dispose();
            disposed = true;

            GC.SuppressFinalize(this);
        }
    }
}
