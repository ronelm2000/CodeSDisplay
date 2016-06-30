using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQL_Phase2
{
    public class Summary
    {
        public static Summary Results { get; } = new Summary();
        public delegate void SummaryEvent();
        public event SummaryEvent OnUpdate;

        private Dictionary<string, Dictionary<string, List<CodeSmellSummary>>> dictionary = new Dictionary<string, Dictionary<string, List<CodeSmellSummary>>>();

        private Summary() {
        }

        public void Add(string filename, string projectAssemblyName, CodeSmellSummary summary)
        {
            if (dictionary.ContainsKey(projectAssemblyName) && dictionary[projectAssemblyName] != null)
            {
                if (dictionary[projectAssemblyName].ContainsKey(filename) && dictionary[projectAssemblyName][filename] != null)
                {
                    dictionary[projectAssemblyName][filename].Add(summary);
                }
                else
                {
                    dictionary[projectAssemblyName].Add(filename, new List<CodeSmellSummary>() { summary });
                }
            } else
            {
                dictionary.Add(projectAssemblyName, new Dictionary<string, List<CodeSmellSummary>>());
                dictionary[projectAssemblyName].Add(filename, new List<CodeSmellSummary>() { summary });
            }
        }

        public void Clear(string projectAssemblyName)
        {
            dictionary.Remove(projectAssemblyName);
        }

        public IEnumerable<TotalValues> GenerateTotals()
        {
            foreach (var dict in dictionary)
            {
                foreach (var item in dict.Value)
                {
                    if (item.Value != null && item.Value.Count > 0)
                    {
                        yield return new TotalValues(
                            item.Key,
                            item.Value.Count,
                            item.Value.Sum((x) => x.Node.LinesRepresented())
                        );
                    }
                }
            }
        }

        internal void Update()
        {
            OnUpdate();
        }

        public IEnumerable<SpecificValues> GenerateSpecificTable(TotalValues selectedItem)
        {
            var result = dictionary
                .SelectMany((x) => x.Value)
                .Where((x) => x.Key == selectedItem.Filename);

            if (result.Count() > 0)
            {
                var resultItem = result.First().Value;
                if (resultItem != null)
                {
                    var noOfEnumItems = Enum.GetNames(typeof(CodeSmellSummary.CodeSmellType)).Length;
                    var specificSummaries = new SpecificValues[noOfEnumItems];
                    for (int i = 0; i < noOfEnumItems; i++)
                    {
                        var type = ((CodeSmellSummary.CodeSmellType)i).ToString();
                        var specificSmells = resultItem.Where((x) => x.Type == (CodeSmellSummary.CodeSmellType)i);
                        var noOfCS = specificSmells.Count();
                        var lines_rep = specificSmells.Sum((x) => x.Node.LinesRepresented());
                        yield return new SpecificValues(type, noOfCS, lines_rep);
                    }
                }
            }
        }
    }

    public class CodeSmellSummary
    {
        public enum CodeSmellType
        {
            DuplicateCode,
            LongMethod,
            LargeClass,
            DataClump,
            InstanceOf,
            MessageChain,
            SwitchStatement,
            TypeCast
        }

        public CodeSmellType Type { get; private set; }
        public Location Location { get; private set; }
        public SyntaxNode Node { get; private set; }


        public CodeSmellSummary (CodeSmellType type, Location location, SyntaxNode node)
        {
            Location = location;
            Type = type;
            Node = node;
        }

        public override bool Equals(object obj)
        {
            if (obj is CodeSmellSummary)
            {
                var objAsLoc = obj as CodeSmellSummary;
                return objAsLoc.Type == Type && objAsLoc.Location.Equals(Location);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
