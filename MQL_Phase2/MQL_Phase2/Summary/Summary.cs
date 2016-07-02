using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace MQL_Phase2
{
    public class Summary
    {
        public static Summary Results { get; } = new Summary();


        public delegate void SummaryEvent();
        public delegate void SummaryStepEvent(string filename, CodeSmellSummary summary);
        public event SummaryEvent OnUpdate;
        public event SummaryStepEvent OnStepUpdate;
        private object DictionaryWriteLock = new object();

        public static readonly Dictionary<CodeSmellType, string> TypeProper = new Dictionary<CodeSmellType, string>()
        {
            {CodeSmellType.DataClump, "Data Clump" },
            {CodeSmellType.DuplicateCode, "Duplicate Code" },
            {CodeSmellType.InstanceOf, "Instance Of / is" },
            {CodeSmellType.LargeClass, "Large Class" },
            {CodeSmellType.LongMethod, "Long Method" },
            {CodeSmellType.MessageChain, "Message Chain" },
            {CodeSmellType.SwitchStatement, "Switch Statement" },
            {CodeSmellType.TypeCast, "Type Cast" }
        };

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
            OnStepUpdate(filename, summary);
        }

        public void Clear(string projectAssemblyName, CodeSmellType type)
        {
            lock (DictionaryWriteLock)
            {
                if (dictionary.ContainsKey(projectAssemblyName))
                {
                    var result = dictionary[projectAssemblyName];
                    if (result != null)
                    {
                        foreach (var i in result.Values)
                        {
                            i.RemoveAll((x) => x.Type == type);
                        }
                    }
                }
            }
        }

        public IEnumerable<TotalTableValue> GenerateTotals()
        {
            lock (DictionaryWriteLock)
            {
                foreach (var dict in dictionary)
                {
                    foreach (var item in dict.Value)
                    {
                        if (item.Value != null && item.Value.Count > 0)
                        {
                            var lineMap = item
                                .Value
                                .Select((x) => x.Node.GetLocation().GetLines())
                                .SelectMany((x) => x)
                                .Distinct();
                            yield return new TotalTableValue(
                                item.Key,
                                item.Value.Count,
                                lineMap.Count()
                            );
                        }
                    }
                }
            }
        }

        internal void Update()
        {
            OnUpdate();
        }

        public IEnumerable<SpecificValue> GenerateSpecificTable(TotalTableValue selectedItem)
        {
            lock (DictionaryWriteLock)
            {
                var result = dictionary
                    .SelectMany((x) => x.Value)
                    .Where((x) => x.Key == selectedItem.Filename);

                if (result.Count() > 0)
                {
                    var resultItem = result.First().Value;
                    if (resultItem != null)
                    {
                        var noOfEnumItems = Enum.GetNames(typeof(CodeSmellType)).Length;
                        var specificSummaries = new SpecificValue[noOfEnumItems];
                        for (int i = 0; i < noOfEnumItems; i++)
                        {
                            var type = ((CodeSmellType)i);
                            var specificSmells = resultItem.Where((x) => x.Type == (CodeSmellType)i);
                            var noOfCS = specificSmells.Count();
                            var lineMap = specificSmells
                                .Select((x) => x.LineMap)
                                .SelectMany((x) => x)
                                .Distinct();
                            var lines_rep = (type == CodeSmellType.LargeClass || type == CodeSmellType.LongMethod || type == CodeSmellType.DataClump) ?
                                noOfCS : lineMap.Count();
                            yield return new SpecificValue(type, noOfCS, lines_rep);
                        }
                    }
                }
            }
        }

        public IEnumerable<LineMapValue> GenerateLineMapping(TotalTableValue summary, SpecificValue specificValue)
        {
            var linq = dictionary
                .SelectMany((x) => x.Value)
                .Where((x) => x.Key == summary.Filename);
            if (linq.Count() > 0)
            {
                var resultTable = linq.First().Value;
                var resultsLinq2 = resultTable
                    .Where((x) => x.Type == specificValue.GetCodeSmellType());
                var values = new List<LineMapValue>();
                foreach (var item in resultsLinq2)
                {
                    var type = item.Type;
                    var lineMap = item.LineMap;
                    foreach (var line in lineMap)
                        if (values.Find((x) => x.Line == line + 1 && x.Type == TypeProper[type]) == null)
                            values.Add(new LineMapValue(TypeProper[type], line + 1));    
                }
                foreach (var item in values) yield return item;
            }
        }
    }

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

    public class CodeSmellSummary
    {
        public CodeSmellType Type { get; private set; }
        public Location Location { get; private set; }
        public SyntaxNode Node { get; private set; }
        public int[] LineMap { get; private set; }


        public CodeSmellSummary (CodeSmellType type, Location location, SyntaxNode node, int[] lines)
        {
            Location = location;
            Type = type;
            Node = node;
            LineMap = lines;
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
