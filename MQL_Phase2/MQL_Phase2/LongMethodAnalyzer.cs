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
    public class LongMethodAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "LongMethod";

        // You can change these strings in the LargeMethodResources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        internal static readonly LocalizableString Title = new LocalizableResourceString(nameof(LargeMethodResources.AnalyzerTitle), LargeMethodResources.ResourceManager, typeof(LargeMethodResources));
        internal static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(LargeMethodResources.AnalyzerMessageFormat), LargeMethodResources.ResourceManager, typeof(LargeMethodResources));
        internal static readonly LocalizableString Description = new LocalizableResourceString(nameof(LargeMethodResources.AnalyzerDescription), LargeMethodResources.ResourceManager, typeof(LargeMethodResources));
        internal const string Category = "Naming";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration);
            Debug.WriteLine("Registered Action!");
        }

        // each node represents a method.
        // this represents the "iterate over all methods" instruction
        public void AnalyzeNode (SyntaxNodeAnalysisContext context)
        {
            // get all the statements of the node (which is the method)
            var this_method = (context.Node as MethodDeclarationSyntax);

            // Find Large Method
            // Two Methods: First determines characters, and the other determines by the number of lines.
            // Suggested Comment in Stench Blossom wants lines, but I'll use the characters one.
            // float rating = all_other_methods.Max((x) => x.Span.Length - 100) / 3000;
            float rating = Math.Max(this_method.FullSpan.Length - this_method.DescendantTrivia().Select(x => x.FullSpan.Length).Sum() - 100, 0) / 3000f;
            // string str_rating2 = this_method.ToString();
            
            if (rating > 0.1)
            {
                // found a long method or something
                var diag = Diagnostic.Create(Rule, context.Node.DescendantTokens().First((x) => x.IsKind(SyntaxKind.IdentifierToken)).GetLocation(), rating * 100);
                context.ReportDiagnostic(diag);
                Summary.Results.Add(
                    context.Node.SyntaxTree.FilePath,
                    context.Compilation.AssemblyName,
                    new CodeSmellSummary(
                        CodeSmellSummary.CodeSmellType.LongMethod,
                        context.Node.DescendantTokens().First((x) => x.IsKind(SyntaxKind.IdentifierToken)).GetLocation(),
                        context.Node
                        )
                );
            }
        }
    }
}
