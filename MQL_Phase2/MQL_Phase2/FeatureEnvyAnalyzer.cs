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

namespace MQL_Phase2
{
    //[DiagnosticAnalyzer]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FeatureEnvyAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FeatureEnvy";

        // You can change these strings in the FeatureEnvyResources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        internal static readonly LocalizableString Title = new LocalizableResourceString(nameof(FeatureEnvyResources.AnalyzerTitle), FeatureEnvyResources.ResourceManager, typeof(FeatureEnvyResources));
        internal static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(FeatureEnvyResources.AnalyzerMessageFormat), FeatureEnvyResources.ResourceManager, typeof(FeatureEnvyResources));
        internal static readonly LocalizableString Description = new LocalizableResourceString(nameof(FeatureEnvyResources.AnalyzerDescription), FeatureEnvyResources.ResourceManager, typeof(FeatureEnvyResources));
        internal const string Category = "Naming";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
            Debug.WriteLine("Registered Action: Feature Envy!");
        }

        // each node represents a method.
        // this represents the "iterate over all methods" instruction
        public void AnalyzeNode (SyntaxNodeAnalysisContext context)
        { 
            // get all the statements of the node (which is the method)
            var thisClass = (ClassDeclarationSyntax)context.Node;
            
            // I need to retrieve all methods from all classes...
            SyntaxNode root = null;
            var all_c_sharp_files = context.SemanticModel.Compilation.SyntaxTrees.Where(x => x.TryGetRoot(out root) && x.Length > 0);
            var all_methods = (from n in all_c_sharp_files select n.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>()).SelectMany(x => x);

            // somehow I need to get the list of all classes referenced by this class
            var classIdentifiers = context.Node.DescendantNodes().OfType<IdentifierNameSyntax>();
            var classMemberReferences = context.Node.DescendantNodes().OfType<MemberAccessExpressionSyntax>();

            List<SyntaxTree> classesReferenced = new List<SyntaxTree>();

            int totalSmells = 0;
            foreach (var method in thisClass.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                var result = context.SemanticModel.AnalyzeDataFlow(method.Body.Statements.ElementAt(0), method.Body.Statements.ElementAt(method.Body.Statements.Count - 1));
                var result2 = result.WrittenOutside;
                foreach (var res in result2)
                {
                    if (res.Locations.ElementAt(0).SourceTree != null)
                    {
                        var results3 = res.Locations.ElementAt(0).SourceTree.GetDiagnostics();
                        totalSmells += Math.Max(0, results3.Count() - classesReferenced.Count);
                    }
                }
            }

            double rating = Math.Min(1.0, Math.Log(totalSmells / 4));

            if (rating > 0.1)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), rating));
            }
        }
    }
}
