using FormulaParser;
using FormulaParser.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CalcRx
{
    public static class Evaluator
    {
        public static Func<IObservable<TInput>, IObservable<TOutput>> Evaluate<TInput, TOutput>(this IObservable<TInput> source, string expression)
        {
            return CreateExpression<TInput, TOutput>(source, expression, null);
        }

        public static Func<IObservable<TInput>, IObservable<TOutput>> Evaluate<TInput, TOutput>(this IObservable<TInput> source, string expression, IEnumerable<Function> functions)
        {
            return CreateExpression<TInput, TOutput>(source, expression, functions);
        }

        private static Func<IObservable<TInput>, IObservable<TOutput>> CreateExpression<TInput, TOutput>(IObservable<TInput> source, string expression, IEnumerable<Function> functions = null)
        {
            ExpressionParser parser;

            if (functions == null)
            {
                parser = new ExpressionParser();
            }
            else
            {
                parser = new ExpressionParser(functions.ToList());
            }

            var baseExpr = Expression.Parameter(source.GetType());

            var exp = parser.BuildExpression(expression, baseExpr);

            var lambda = Expression.Lambda(exp, baseExpr);

            return (Func<IObservable<TInput>, IObservable<TOutput>>)lambda.Compile();
        }
    
    }
}
