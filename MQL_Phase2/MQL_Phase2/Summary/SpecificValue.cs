using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQL_Phase2
{
    public class SpecificValue
    {
        public string Type { get; internal set; }
        public int No_Of_CodeSmells { get; internal set; }
        public int Lines_Represented { get; internal set; }

        private CodeSmellType actualType;
        
        public SpecificValue (CodeSmellType type, int noOfCS, int lines_rep)
        {
            actualType = type;
            No_Of_CodeSmells = noOfCS;
            Lines_Represented = lines_rep;
            Type = Summary.TypeProper[type];
        }

        // designed so that it won't appear in the dataGrid.
        public CodeSmellType GetCodeSmellType()
        {
            return actualType;
        }
    }
}
