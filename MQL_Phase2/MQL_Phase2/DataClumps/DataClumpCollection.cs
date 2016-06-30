using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MQL_Phase2.DataClumps
{
    public class DataClumpCollection
    {
        private IEnumerable<MethodDeclarationSyntax> all_methods_except_this;

        public List<DataClump> List { get; } = new List<DataClump>();
        
        public DataClumpCollection(IEnumerable<MethodDeclarationSyntax> all_methods_except_this)
        {
            foreach (var method in all_methods_except_this)
            {
                TryAddAll(DataClump.GetAllDataClumpsFrom(method));
            }
            /*
            List = List.Distinct().ToList();
            */
        }

        private bool TryAddAll(DataClump[] dataClump)
        {
            var result = true;
            foreach (DataClump d in dataClump)
            {
                if (!TryAdd(d)) result = false;
            }
            return result;
        }

        public bool TryAdd (DataClump dataClump)
        {
            foreach (DataClump dataClumpInList in List)
            {
                if (dataClumpInList == dataClump) return false;
            }

            List.Add(dataClump);
            return true;
        }
        
        internal bool[] ContainsAny(DataClump[] dataClumps)
        {
            bool[] results = new bool[dataClumps.Length];
            List.RemoveAll((x) => x.IsClean);
            for (int i = 0; i < dataClumps.Length; i++)
            {
                results[i] = !dataClumps[i].IsClean && List.Exists((x) => dataClumps[i].Equals(x));
            }
            return results;
        }
    } // == is not .Equals
}
