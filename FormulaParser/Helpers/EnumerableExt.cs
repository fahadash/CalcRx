using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormulaParser.Helpers
{
    internal static class EnumerableExt
    {
        public static IEnumerable<TElement> EmptyIfNull<TElement>(this IEnumerable<TElement> source)
        {
            return source == null? Enumerable.Empty<TElement>() : source;
        }
    }
}
