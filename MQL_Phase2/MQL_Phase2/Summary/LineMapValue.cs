using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQL_Phase2
{
    public class LineMapValue
    {
        public string Type { get; internal set; }
        public int Line { get; internal set; }

        public LineMapValue(string type, int line)
        {
            Type = type;
            Line = line;
        }
    }
}
