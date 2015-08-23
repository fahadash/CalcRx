using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormulaParser.Contracts
{
    public class Function
    {
        public Function(string name, object value)
        {
            this.Name = name;
            this.Value = value;
        }
        public string Name { get; set; }

        public object Value { get; set; }
    }
}
