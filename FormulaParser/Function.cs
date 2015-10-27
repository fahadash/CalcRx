using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FormulaParser
{
    internal class Function
    {
        public string FunctionName { get; set; }

        public Expression FunctionExpression { get; set; }

        public List<Expression> Parameters { get; set; }

        public Type ReturnType { get; set; }
    }
}
