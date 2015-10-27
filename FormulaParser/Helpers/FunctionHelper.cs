using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FormulaParser.Helpers
{
    internal class FunctionHelper
    {
        internal static Function SeekFunction(string name, Expression[] arguments, bool seekInsideObservable = false)
        {
            var qualifiedName = string.Format("{0}({1})", name, arguments.Count());

            // TODO: Handle missing function events
            var function = ExpressionsHelper.functions.Where(f => f.FunctionName.Equals(qualifiedName)
                        && arguments.Select(a =>
                        {
                            var type = a.Type.GetFirstObservableGenericType();

                            if (type == null || seekInsideObservable == false)
                            {
                                return a.Type;
                            }

                            return type;

                        })
                                    .SequenceEqual(f.Parameters
                                                        .Select(p => p.Type))).FirstOrDefault();

            return function;
        }
    }
}
