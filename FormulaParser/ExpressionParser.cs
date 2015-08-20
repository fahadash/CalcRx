using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormulaParser
{
    public class ExpressionParser : IDisposable
    {

        /// Create a separate Function class that takes name, function object, and parameter expressions
        /// key-value pairs are not going to work. good luck
        List<Function> functions = new List<Function>();

        public ExpressionParser()
        {

        }
        public ExpressionParser(IList<KeyValuePair<string, object>> functions)
        {
            if (functions != null)
            {
                var function = functions.First();
                var type = function.Value.GetType();

                if (type.IsFunction())
                {
                    var func = new Function();

                    var numParameters = type.GetNumberOfGenericArguments()  - 1;
                    var functionExpression = Expression.Constant(function.Value);
                    func.FunctionName = string.Format("{0}({1})",
                        function.Key,
                        numParameters.ToString());

                    func.Parameters = type.GetGenericArguments()
                                            .Take(numParameters)
                                            .Select(a => Expression.Parameter(a))
                                            .OfType<Expression>()
                                            .ToList();
                    func.FunctionExpression = functionExpression;

                    this.functions.Add(func);
                }

                ExpressionsHelper.functions = this.functions;
            }
        }

        public Expression BuildExpression(string input, Expression baseExpression)
        {
            var scanner = new Scanner(new MemoryStream(Encoding.UTF8.GetBytes(input)));
            var parser = new Parser(scanner);

            parser.BaseExpression = baseExpression;
            parser.Parse();
            return parser.Output;
        }

        public void Dispose()
        {
        }
    }
}
