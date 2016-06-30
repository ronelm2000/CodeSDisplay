using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQL_Phase2
{
    public class TotalValues
    {
        public string Filename { get; set; }
        public int Total { get; set; }
        public int Represented_Lines { get; set; }

        public TotalValues(string filename, int number, int represented_lines)
        {
            Filename = filename;
            Total = number;
            Represented_Lines = represented_lines;
        }

    }
}
