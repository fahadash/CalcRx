using FormulaParser;
using FormulaParser.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using Test.Contracts;
using Xunit;
using Xunit.Abstractions;
using CalcRx;

namespace Test
{
    public class CalcRxTest
    {        
        private readonly ITestOutputHelper output;
        public CalcRxTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        [Trait("Category", "CalcRx Simple")]
        public void AddWorks()
        {
            var observable = Observable.Range(0, 10);

            var result = observable.Evaluate<int, double>("_ + 1");

            var combined = observable.Zip(result, (a, b) => new {a, b});

            combined.Subscribe(o => {
                output.WriteLine("{0} - {1}", o.a, o.b);
                Assert.True(o.a + 1 == o.b);
            });
        }


        [Fact]
        [Trait("Category", "CalcRx Simple")]
        public void SubtractWorks()
        {
            var observable = Observable.Range(0, 10);

            var result = observable.Evaluate<int, double>("3 - _");

            var combined = observable.Zip(result, (a, b) => new { a, b });

            combined.Subscribe(o =>
            {
                output.WriteLine("{0} - {1}", o.a, o.b);
                Assert.True(3 - o.a == o.b);
            });
        }


        [Fact]
        [Trait("Category", "CalcRx Simple")]
        public void MultiplyWorks()
        {
            var observable = Observable.Range(0, 10);

            var result = observable.Evaluate<int, double>("3 * _");

            var combined = observable.Zip(result, (a, b) => new { a, b });

            combined.Subscribe(o =>
            {
                output.WriteLine("{0} - {1}", o.a, o.b);
                Assert.True(3 * o.a == o.b);
            });
        }

        [Fact]
        [Trait("Category", "CalcRx Simple")]
        public void DivideWorks()
        {
            var observable = Observable.Range(1, 10);

            var result = observable.Evaluate<int, double>("3 / _");

            var combined = observable.Zip(result, (a, b) => new { a, b });

            combined.Subscribe(o =>
            {
                output.WriteLine("{0} - {1} == {2}", o.a, o.b, 3/o.a);
                Assert.True(Convert.ToDouble(3) / o.a == o.b);
            });
        }


        [Fact]
        [Trait("Category", "CalcRx Simple")]
        public void PowerWorks()
        {
            var observable = Observable.Range(1, 10);

            var result = observable.Evaluate<int, double>("_ ^ 3");

            var combined = observable.Zip(result, (a, b) => new { a, b });

            combined.Subscribe(o =>
            {
                output.WriteLine("{0}^3, {1} == {2}", o.a, o.b, (Math.Pow(o.a, 3)));
                Assert.True(Math.Pow(o.a, 3) == o.b);
            });
        }


        [Fact]
        [Trait("Category", "CalcRx Simple")]
        public void ComplicatedWorks()
        {
            var observable = Observable.Range(1, 10);

            var result = observable.Evaluate<int, double>("(_-32)*5/9");

            var combined = observable.Zip(result, (a, b) => new { a, b });

            combined.Subscribe(o =>
            {
                output.WriteLine("{0} - {1}", o.a, o.b);
                Assert.True((Convert.ToDouble(o.a) - 32) * 5 / 9 == o.b);
            });            
        }


        [Fact]
        [Trait("Category", "CalcRx Simple")]
        public void PropertyAccessWorks()
        {
            var observable = Observable.Range(0, 10)
                .Select(t => new Tick() { Time = DateTime.Now, Price = t, Symbol = "AAPL" });

            var result = observable.Evaluate<Tick, double>("Price * 2");

            var combined = observable.Zip(result, (a, b) => new { a, b });

            combined.Subscribe(o =>
            {
                output.WriteLine("{0} - {1}", o.a, o.b);
                Assert.True(o.a.Price * 2 == o.b);
            });
        }

        [Fact]
        [Trait("Category", "CalcRx Simple")]
        public void FunctionCallWorks()
        {
            var list = new List<Function>();
            var sum = new Func<IObservable<int>, IObservable<int>>(o => o.Scan(0, (p, n) => n + p));
            list.Add(new Function("SUM", sum));
            var observable = Observable.Range(0, 10);

            var result = observable.Evaluate<int, int>("SUM(_)", list);

            var test = sum(observable);

            var combined = result.Zip(test, (a, b) => new { a, b });
            combined.Subscribe(o =>
            {
                output.WriteLine("{0} - {1}", o.a, o.b);
                Assert.True(o.a == o.b);
            });           
        }
  
    }
}
