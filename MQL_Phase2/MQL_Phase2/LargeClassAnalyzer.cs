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
    public class LargeClassAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "LargeClass";

        // You can change these strings in the LargeClassResources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        internal static readonly LocalizableString Title = new LocalizableResourceString(nameof(LargeClassResources.AnalyzerTitle), LargeClassResources.ResourceManager, typeof(LargeClassResources));
        internal static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(LargeClassResources.AnalyzerMessageFormat), LargeClassResources.ResourceManager, typeof(LargeClassResources));
        internal static readonly LocalizableString Description = new LocalizableResourceString(nameof(LargeClassResources.AnalyzerDescription), LargeClassResources.ResourceManager, typeof(LargeClassResources));
        internal const string Category = "Naming";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
            Debug.WriteLine("Registered Action!");
        }

        // each node represents a method.
        // this represents the "iterate over all methods" instruction
        public void AnalyzeNode (SyntaxNodeAnalysisContext context)
        {
            // get all the statements of the node (which is the method)
            var this_class = (context.Node as ClassDeclarationSyntax);

            // I need to retrieve all methods from all classes...
            SyntaxNode root = null;
            var all_c_sharp_files = context.SemanticModel.Compilation.SyntaxTrees.Where(x => x.TryGetRoot(out root) && x.Length > 0);
            var all_classes = (from n in all_c_sharp_files select n.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()).SelectMany(x => x);

            // Find Large Class
            float rating = Math.Max(this_class.SyntaxTree.Length - 500, 0);
            rating -= 500;
            rating /= 20000;
            /*
            // Class > AllMethods > GetAllTypes > GetLength
            var all_methods_on_this_class = this_class.DescendantNodes().OfType<MethodDeclarationSyntax>();
            double rating = 0;
            foreach (var method in all_methods_on_this_class)
            {
                var method_type = method.ReturnType;
                bool ratingAdded = false;
                List<string> classesFound = new List<string>();
                foreach (var dem_class in all_classes)
                {
                    if (!classesFound.Contains(method_type.ToString()) &&
                        dem_class.Identifier.Text.Equals(method_type.ToString()))
                    {
                        var length_of_type = dem_class.SyntaxTree.Length;
                        rating += Math.Max(length_of_type - 500, 0) - 500;
                        ratingAdded = true;
                        classesFound.Add(method_type.ToString());
                    }
                }
            }
            */
            if (rating > 0.1)
            {
                // found a large class or something
                var diag = Diagnostic.Create(Rule, context.Node.DescendantTokens().First((x) => x.IsKind(SyntaxKind.IdentifierToken)).GetLocation(), rating * 100);
                context.ReportDiagnostic(diag);
                Summary.Results.Add(
                    context.Node.SyntaxTree.FilePath,
                    context.Compilation.AssemblyName,
                    new CodeSmellSummary(
                        CodeSmellSummary.CodeSmellType.LargeClass,
                        context.Node.DescendantTokens().First((x) => x.IsKind(SyntaxKind.IdentifierToken)).GetLocation(),
                        context.Node
                        )
                );
            }
        }
    }
}
