using System;


namespace numerical_expression_parser
{
    class Program
    {
        static void Main(string[] args)
        {
            var expression = args[0];
            var result = NumericalExpressionParser.Evaluate(expression);
            Console.WriteLine($"{expression} = {result}");
        }
    }
}
