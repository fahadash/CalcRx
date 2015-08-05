using FormulaParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            var baseExpr = Expression.Parameter(typeof(IObservable<int>));
            var expression = new ExpressionParser().BuildExpression("-(_-32)*5/9", baseExpr);

            var lambda = Expression.Lambda(expression, baseExpr);

            Func<IObservable<int>, IObservable<double>> func = (Func<IObservable<int>, IObservable<double>>) lambda.Compile();

            var ob = func(Observable.Range(70, 100));

            ob.Subscribe(Console.WriteLine);

            Console.ReadKey();
        }
    }
}
