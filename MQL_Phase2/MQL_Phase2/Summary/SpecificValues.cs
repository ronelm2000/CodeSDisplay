using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQL_Phase2
{
    public class SpecificValues
    {
        public string Type { get; internal set; }
        public int No_Of_CodeSmells { get; internal set; }
        public int Lines_Represented { get; internal set; }
        
        public SpecificValues (string type, int noOfCS, int lines_rep)
        {
            Type = type;
            No_Of_CodeSmells = noOfCS;
            Lines_Represented = lines_rep;
        } 
    }
}
