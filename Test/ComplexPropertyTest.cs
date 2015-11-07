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
    public class ComplexPropertyTest
    {
        internal class myclass
        {
            public int X { get; set; }
            public int Y { get; set; }
        }
        private readonly ITestOutputHelper output;
        public ComplexPropertyTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        [Trait("Category", "Complex Properties")]
        public void TestPropertyAdd()
        {
            
            var observable = Observable.Range(0, 10)
                .Select(x => new myclass() { X = x, Y = 2*x });

            var result = observable.Evaluate<myclass, int>("X+Y");

            var combined = observable.Zip(result, (a, b) => new { a, b });

            combined.Subscribe(o =>
            {
                output.WriteLine("Sequence [X={0}, Y={1}] Result[{2}]", o.a.X, o.a.Y, o.b);
                Assert.True(o.a.X + o.a.Y == o.b);           
            });
        }

        [Fact]
        [Trait("Category", "Complex Properties")]
        public void TestPropertyMultiply()
        {

            var observable = Observable.Range(0, 10)
                .Select(x => new myclass() { X = x, Y = 2 * x });

            var result = observable.Evaluate<myclass, int>("X*Y");

            var combined = observable.Zip(result, (a, b) => new { a, b });

            combined.Subscribe(o =>
            {
                output.WriteLine("Sequence [X={0}, Y={1}] Result[{2}]", o.a.X, o.a.Y, o.b);
                Assert.True(o.a.X * o.a.Y == o.b);
            });
        }

        [Fact]
        [Trait("Category", "Complex Properties")]
        public void TestPropertyDivide()
        {

            var observable = Observable.Range(1, 10)
                .Select(x => new myclass() { X = x, Y = 2 * x });

            var result = observable.Evaluate<myclass, int>("X/Y");

            var combined = observable.Zip(result, (a, b) => new { a, b });

            combined.Subscribe(o =>
            {
                output.WriteLine("Sequence [X={0}, Y={1}] Result[{2}]", o.a.X, o.a.Y, o.b);
                Assert.True(o.a.X / o.a.Y == o.b);
            });
        }
    }
}
