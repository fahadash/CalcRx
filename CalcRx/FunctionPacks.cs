using FormulaParser.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcRx
{
    public class FunctionPacks
    {
        public static IEnumerable<Function> BasicAggregationFunctionPack
        {
            get
            {
                return new[] 
                {
                    new Function("SUM", 
                        new Func<IObservable<double>, IObservable<double>>(
                        o => o.Scan(0.0d, (acc, item) => item + acc))),
                    new Function("MIN", 
                        new Func<IObservable<double>, IObservable<double>>(
                        o => o.Scan(double.MinValue, (acc, item) => MinOf(acc, item)))),
                    new Function("MAX", 
                        new Func<IObservable<double>, IObservable<double>>(
                        o => o.Scan(double.MinValue, (acc, item) => MaxOf(acc, item)))),
                    new Function("COUNT", 
                        new Func<IObservable<double>, IObservable<double>>(
                        o => o.Scan(0.0d, (acc, _) => acc + 1))),
                };
            }            
        }
        public static IEnumerable<Function> HistoryFunctionPack
        {
            get
            {
                return new[] 
                {
                    new Function("REF", 
                        new Func<IObservable<double>, double, IObservable<double>>(
                            //TODO: Change the Grammar to parse int and double separately
                            //  so 2nd parameter can be int
                        (o, count) =>  o.Zip(o.Skip(Convert.ToInt32(count)), (a, b) => a)))
                };
            }            
        }



        public static double MinOf(double a, double b)
        {
            if (a < b)
            {
                return a;
            }

            return b;
        }
        public static double MaxOf(double a, double b)
        {
            if (a > b)
            {
                return a;
            }

            return b;
        }
    }
}
