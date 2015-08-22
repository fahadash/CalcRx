using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcRx
{
    public static class Evaluator
    {
        public static Func<IObservable<TInput>, IObservable<TOutput>> Evaluate<TInput, TOutput>(this IObservable<TInput> source, string expression)
        {
//##TODO: Finish working on this extension
            return null;
        }
    }
}
