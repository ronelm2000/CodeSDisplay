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
using MQL_Phase2;
using MQL_Phase2.Resource;
using System.Diagnostics;

namespace DiagnosticAnalyzerAndCodeFix
{
    //[DiagnosticAnalyzer]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SwitchStatementAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "SwitchStatement";

        // You can change these strings in the SwitchStatementResources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        internal static readonly LocalizableString Title = new LocalizableResourceString(nameof(SwitchStatementResources.AnalyzerTitle), SwitchStatementResources.ResourceManager, typeof(SwitchStatementResources));
        internal static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(SwitchStatementResources.AnalyzerMessageFormat), SwitchStatementResources.ResourceManager, typeof(SwitchStatementResources));
        internal static readonly LocalizableString Description = new LocalizableResourceString(nameof(SwitchStatementResources.AnalyzerDescription), SwitchStatementResources.ResourceManager, typeof(SwitchStatementResources));
        internal const string Category = "Naming";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.SwitchStatement);
            context.RegisterSyntaxNodeAction(RatingCompute, SyntaxKind.MethodDeclaration);
            Debug.WriteLine("Registered Action!");
        }

        // each node represents a method.
        // this represents the "iterate over all methods" instruction
        public void AnalyzeNode (SyntaxNodeAnalysisContext context)
        {
            var diag = Diagnostic.Create(Rule, context.Node.HighlightOneLine(), 0);
            // var rating = switch.GetAllCases.Count / 3f;
            context.ReportDiagnostic(diag);
            Summary.Results.Add(
                context.Node.SyntaxTree.FilePath,
                context.Compilation.AssemblyName,
                new CodeSmellSummary(
                    CodeSmellSummary.CodeSmellType.SwitchStatement,
                    context.Node.HighlightOneLine(),
                    context.Node
                    )
            );
        }

        public void RatingCompute (SyntaxNodeAnalysisContext context)
        {
            var all_switches = context.Node.DescendantNodes().OfType<SwitchStatementSyntax>();
            if (all_switches.Count() > 0)
            {
                var rating = Math.Log(all_switches.Sum((x) => x.Span.Length)) / 8d;
                var diag = Diagnostic.Create(Rule, context.Node.HighlightOneLine(), rating);
                // context.ReportDiagnostic(diag);
            }
        }
    }
}
