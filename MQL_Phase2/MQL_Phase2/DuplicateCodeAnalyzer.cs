using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using MQL_Phase2.Resource;
using System.Diagnostics;
using System.IO;

namespace MQL_Phase2
{
    //[DiagnosticAnalyzer]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DuplicateCodeAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DupCode";

        // You can change these strings in the DuplicateCodeResources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        internal static readonly LocalizableString Title = new LocalizableResourceString(nameof(DuplicateCodeResources.AnalyzerTitle), DuplicateCodeResources.ResourceManager, typeof(DuplicateCodeResources));
        internal static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(DuplicateCodeResources.AnalyzerMessageFormat), DuplicateCodeResources.ResourceManager, typeof(DuplicateCodeResources));
        internal static readonly LocalizableString Description = new LocalizableResourceString(nameof(DuplicateCodeResources.AnalyzerDescription), DuplicateCodeResources.ResourceManager, typeof(DuplicateCodeResources));
        internal const string Category = "Naming";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.Block);
            context.RegisterCompilationStartAction((x) =>
            {
                Summary.Results.Clear(x.Compilation.AssemblyName, CodeSmellType.DuplicateCode);
            });
            context.RegisterCompilationAction((x) =>
            {
                Summary.Results.Update();
            });
        }

        // each node represents a method.
        // this represents the "iterate over all methods" instruction
        public void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            // get all the statements of the node (which is the method)
            SyntaxList<SyntaxNode> statements = (context.Node as BlockSyntax).Statements;
            // I need to retrieve all methods from all classes...
            SyntaxNode root = null;
            var all_c_sharp_files = context.SemanticModel.Compilation.SyntaxTrees.Where(x => x.TryGetRoot(out root) && x.Length > 0);
            var all_blocks = (from n in all_c_sharp_files select n.GetRoot().DescendantNodes().OfType<BlockSyntax>()).SelectMany(x => x);
            var all_prev_diag = new List<Diagnostic>();
            // check all lists in the SyntaxNode
            // first thing 
            // if there is nothing, something must be added
            // if there is anothing that is structurally equivalent, why bother as well?
            // Parallel.ForEach(all_methods.Select((x) => x.Body.Statements), (SyntaxList<StatementSyntax> x) =>
            foreach (var thisBlock in all_blocks.Select((x) => x.Statements))
            {
                for (int i = 0; i < statements.Count; i++)
                {
                    int equivalent_lines = 0;
                    int equivalent_nodes = 0;
                    for (int j = 0; j < thisBlock.Count; j++)
                    {
                        // if an equal statement is read, add into the equivalent lines
                        // the statement must be "structurally equal" even if the arguments are different
                        if (i + equivalent_nodes < statements.Count &&
                            !statements.ElementAt(i + equivalent_nodes)
                                        .GetLocation()
                                        .Equals(thisBlock.ElementAt(j).GetLocation()) &&
                            thisBlock.ElementAt(j).IsCongruentTo(statements.ElementAt(i + equivalent_nodes))
                            // && thisNotNullMethod.ElementAt(j) != statements.ElementAt(i + equivalent_lines) 
                            // statements.ElementAt(i + equivalent_lines).IsEquivalentTo(thisNotNullMethod.ElementAt(j))
                            )
                        {   // this is working but does not work for different arguments.
                            // However if the equivalent lines were 3 or more, duplicate code found.
                            equivalent_lines += thisBlock[j].LinesRepresented();
                            equivalent_nodes++;
                        }
                        else
                        {
                            // the actual "equivalence" is the line on j BEFORE IT!

                            if (equivalent_lines > 2)
                                foreach (Diagnostic d in CheckEquivalentLines(equivalent_lines, equivalent_nodes, statements, i, thisBlock, j - 1))
                                    TryToAddDiagnostic(context, ref all_prev_diag, d);

                            equivalent_lines = 0;
                            equivalent_nodes = 0;

                        }
                    }

                    if (equivalent_lines > 2)
                        foreach (Diagnostic d in CheckEquivalentLines(equivalent_lines, equivalent_nodes, statements, i, thisBlock, thisBlock.Count - 1))
                            TryToAddDiagnostic(context, ref all_prev_diag, d);


                }
            }
            //});
        }

        private void TryToAddDiagnostic(SyntaxNodeAnalysisContext context, ref List<Diagnostic> all_prev_diag, Diagnostic d)
        {
            if (all_prev_diag.Where((x) => x.GetMessage() == d.GetMessage() && x.Location == d.Location).Count() == 0)
            {
                all_prev_diag.Add(d);
                context.ReportDiagnostic(d);
                try
                {
                    Summary.Results.Add(
                        context.Node.SyntaxTree.FilePath,
                        context.Compilation.AssemblyName,
                        new CodeSmellSummary(
                            CodeSmellType.DuplicateCode,
                            context.Node.HighlightOneLine(),
                            context.Node,
                            context.Node.GetLocation().GetLines().ToArray()
                            )
                    );
                } catch (Exception e)
                {
                    Debug.WriteLine(e.Message + "\n" + e.StackTrace);
                }
            }
        }

        private IEnumerable<Diagnostic> CheckEquivalentLines(
            int equivalent_lines, 
            int equivalent_nodes, 
            SyntaxList<SyntaxNode> statements, 
            int statement_index, 
            SyntaxList<SyntaxNode> target, 
            int target_index)
        {
            // duplicated code found.
            // let's try to reduce number of lines to put in; we don't want a full text on IDE.
            // j though is the index of the line AFTER the actual code.
            Location[] otherLocations = new Location[equivalent_nodes - 1];
            for (int i = 0; i < equivalent_nodes - 1; i++)
            {
                otherLocations[i] = statements.ElementAt(statement_index + 1 + i).GetLocation();
            }
            var d1 = Diagnostic.Create(
                Rule, 
                statements.ElementAt(statement_index).HighlightOneLine(),
                target.ElementAt(target_index - equivalent_nodes + 1).GetLocation().Format()
                );

            if (statements
                .ElementAt(statement_index)
                .GetDiagnostics()
                .Where((x) => x.GetMessage() == d1.GetMessage() && x.Location.Equals(d1.Location))
                .Count() == 0)
                yield return d1;
            for (int i = 0; i < equivalent_nodes - 1; i++)
            {
                var d = Diagnostic.Create(
                    Rule,
                    statements.ElementAt(statement_index + 1 + i).HighlightOneLine(),
                    target.ElementAt(target_index - equivalent_nodes + 2 + i).GetLocation().Format()
                    );
                if (statements
                    .ElementAt(statement_index + 1 + i)
                    .GetDiagnostics()
                    .Where((x) => x.GetMessage() == d.GetMessage() && x.Location.Equals(d.Location))
                    .Count() == 0)
                    yield return d;
            }
        }
    }
}
