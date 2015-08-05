using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Contracts
{
    internal class Tick
    {
        public string Symbol { get; set; }
        public double Price { get; set; }
        public DateTime Time { get; set; }
    }
}
