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
        public void AddWorks()
        {
            var observable = Observable.Range(0, 10);

            var add = observable.Evaluate<int, double>("_ + 1");

            var combined = observable.Zip(add, (a, b) => new {a, b});

            combined.Subscribe(o => output.WriteLine("{0} - {1}", o.a, o.b));
        }
    }
}
