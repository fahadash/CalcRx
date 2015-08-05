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
