using FormulaParser;
using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using Test.Contracts;
using Xunit;
using Xunit.Abstractions;

namespace Test
{
    public class ExpressionParserTests : IClassFixture<ExpressionParser>
    {
        ExpressionParser parser;
        private readonly ITestOutputHelper output;
        public ExpressionParserTests(ExpressionParser parser, ITestOutputHelper output)
        {
            this.parser = parser;
            this.output = output;
        }

        [Fact]
        public void TestAdd()
        {
            var baseExpr = Expression.Parameter(typeof(IObservable<int>));

            var observable = Observable.Range(0, 10);
            var expression = parser.BuildExpression("3 + _", baseExpr);

            var lambda = Expression.Lambda(expression, baseExpr);

            var func = (Func<IObservable<int>, IObservable<double>>)lambda.Compile();

            var result = func(observable);

            result.Subscribe(t => output.WriteLine(string.Format("TestAdd() - {0}", t)));
        }


        [Fact]
        public void TestSubtract()
        {
            var baseExpr = Expression.Parameter(typeof(IObservable<int>));

            var observable = Observable.Range(0, 10);
            var expression = parser.BuildExpression("3 - _", baseExpr);

            var lambda = Expression.Lambda(expression, baseExpr);

            var func = (Func<IObservable<int>, IObservable<double>>)lambda.Compile();

            var result = func(observable);

            result.Subscribe(t => output.WriteLine(string.Format("TestSubtract() - {0}", t)));
        }


        [Fact]
        public void TestMultiply()
        {
            var baseExpr = Expression.Parameter(typeof(IObservable<int>));

            var observable = Observable.Range(0, 10);
            var expression = parser.BuildExpression("3 * _", baseExpr);

            var lambda = Expression.Lambda(expression, baseExpr);

            var func = (Func<IObservable<int>, IObservable<double>>)lambda.Compile();

            var result = func(observable);

            result.Subscribe(t => output.WriteLine(string.Format("TestMultiply() - {0}", t)));
        }


        [Fact]
        public void TestDivide()
        {
            var baseExpr = Expression.Parameter(typeof(IObservable<int>));

            var observable = Observable.Range(0, 10);
            var expression = parser.BuildExpression("3 / _", baseExpr);

            var lambda = Expression.Lambda(expression, baseExpr);

            var func = (Func<IObservable<int>, IObservable<double>>)lambda.Compile();

            var result = func(observable);

            result.Subscribe(t => output.WriteLine(string.Format("TestDivide() - {0}", t)));
        }


        [Fact]
        public void TestPower()
        {
            var baseExpr = Expression.Parameter(typeof(IObservable<int>));

            var observable = Observable.Range(0, 10);
            var expression = parser.BuildExpression("_ ^ 2", baseExpr);

            var lambda = Expression.Lambda(expression, baseExpr);

            var func = (Func<IObservable<int>, IObservable<double>>)lambda.Compile();

            var result = func(observable);

            result.Subscribe(t => output.WriteLine(string.Format("TestPower() - {0}", t)));
        }


        [Fact]
        public void TestComplicated()
        {
            var baseExpr = Expression.Parameter(typeof(IObservable<int>));

            var observable = Observable.Range(70, 10);
            var expression = parser.BuildExpression("(_-32)*5/9", baseExpr);

            var lambda = Expression.Lambda(expression, baseExpr);

            var func = (Func<IObservable<int>, IObservable<double>>)lambda.Compile();

            var result = func(observable);

            result.Subscribe(t => output.WriteLine(string.Format("TestComplicated() - {0}", t)));
        }

        
        [Fact]
        public void TestPropertyAccess()
        {
            var baseExpr = Expression.Parameter(typeof(IObservable<Tick>));

            var observable = Observable.Range(0, 10)
                .Select(t => new Tick() { Time = DateTime.Now, Price = t, Symbol = "AAPL" });

            var expression = parser.BuildExpression("Price * 2", baseExpr);

            var lambda = Expression.Lambda(expression, baseExpr);

            var func = (Func<IObservable<Tick>, IObservable<double>>)lambda.Compile();

            var result = func(observable);

            result.Subscribe(t => output.WriteLine(string.Format("{0}", t)));
        }
    }
}
