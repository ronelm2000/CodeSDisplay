using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MQL_Phase2.DataClumps;
using MQL_Phase2.Resource;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MQL_Phase2
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DataClumpAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DataClump";

        public Summary ResultSet { get; private set; } = Summary.Results;

        // You can change these strings in the DataClumpResources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        internal static readonly LocalizableString Title = new LocalizableResourceString(nameof(DataClumpResources.AnalyzerTitle), DataClumpResources.ResourceManager, typeof(DataClumpResources));
        internal static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(DataClumpResources.AnalyzerMessageFormat), DataClumpResources.ResourceManager, typeof(DataClumpResources));
        internal static readonly LocalizableString Description = new LocalizableResourceString(nameof(DataClumpResources.AnalyzerDescription), DataClumpResources.ResourceManager, typeof(DataClumpResources));
        internal const string Category = "Naming";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            context.RegisterSyntaxNodeAction(DetectDataClumps, SyntaxKind.MethodDeclaration);
            context.RegisterCompilationStartAction((x) =>
            {
                Summary.Results.Clear(x.Compilation.AssemblyName, CodeSmellType.DataClump);
            });
            context.RegisterCompilationAction((x) =>
            {
                Summary.Results.Update();
            });
        }

        private void DetectDataClumps(SyntaxNodeAnalysisContext context)
        {
            // I need to retrieve all methods from all classes...
            SyntaxNode root = null;
            var all_c_sharp_files = context.SemanticModel.Compilation.SyntaxTrees.Where(x => x.TryGetRoot(out root) && x.Length > 0);
            var all_methods = (from n in all_c_sharp_files select n.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>()).SelectMany(x => x);
            var all_methods_except_this = all_methods.Where(x => x != context.Node);
            // foreach block quote, get affected series of variables
            // generate potential data clumps from context node

            DataClumpCollection collection = new DataClumpCollection(all_methods_except_this);

            
            DataClump[] dataClump = DataClump.GetAllDataClumpsFrom(context.Node as MethodDeclarationSyntax);

            bool[] result = collection.ContainsAny(dataClump);
            if (result.Contains(true))
            {
                // add it into the Diagnostic.
                var diag = Diagnostic.Create(Rule, context.Node.DescendantTokens().First((x) => x.IsKind(SyntaxKind.IdentifierToken)).GetLocation());
                context.ReportDiagnostic(diag);
                Summary.Results.Add(
                    context.Node.SyntaxTree.FilePath, 
                    context.Compilation.AssemblyName,
                    new CodeSmellSummary(
                        CodeSmellType.DataClump, 
                        context.Node.DescendantTokens().First((x) => x.IsKind(SyntaxKind.IdentifierToken)).GetLocation(),
                        context.Node,
                        context.Node.GetLocation().GetLines().ToArray()
                        )
                );
            }
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
        }
    }
}
