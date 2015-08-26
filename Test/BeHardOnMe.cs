﻿using FormulaParser;
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
    public class BeHardOnMe
    {        
        private readonly ITestOutputHelper output;
        public BeHardOnMe(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        [Trait("Category", "CalcRx Hard")]
        public void AddWorks()
        {
            var observable = Observable.Range(0, 10);

            var result = observable.Evaluate<int, double>("_/(1-(_/10)^2)");

            var combined = observable.Zip(result, (a, b) => new { a, b });

            combined.Subscribe(o =>
            {
                output.WriteLine("{0} - {1} - {2}", o.a, o.b, Convert.ToDouble(o.a) / (1 - Math.Pow((Convert.ToDouble(o.a) / 10), 2)));
                //Assert.True(Convert.ToDouble(o.a)/(1-Math.Pow(Convert.ToDouble(o.a)/10.0d, 2.0d)) == o.b);
            });
        }
    }
}
