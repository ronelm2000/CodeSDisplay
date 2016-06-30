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
    public class TypeCastAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TypecastStatement";

        // You can change these strings in the TypeCastResources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        internal static readonly LocalizableString Title = new LocalizableResourceString(nameof(TypeCastResources.AnalyzerTitle), TypeCastResources.ResourceManager, typeof(TypeCastResources));
        internal static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(TypeCastResources.AnalyzerMessageFormat), TypeCastResources.ResourceManager, typeof(TypeCastResources));
        internal static readonly LocalizableString Description = new LocalizableResourceString(nameof(TypeCastResources.AnalyzerDescription), TypeCastResources.ResourceManager, typeof(TypeCastResources));
        internal const string Category = "Naming";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.CastExpression);
            //context.RegisterSyntaxNodeAction(RatingCompute, SyntaxKind.MethodDeclaration);
            Debug.WriteLine("Registered Action!");
        }

        // each node represents a method.
        // this represents the "iterate over all methods" instruction
        public void AnalyzeNode (SyntaxNodeAnalysisContext context)
        {
            var diag = (Diagnostic)Diagnostic.Create(Rule, context.Node.GetLocation(), 0);
            // var rating = method.GetAllTypeCasts.GetAllClasses.Unique.Count / 8f;
            context.ReportDiagnostic(diag);
            Summary.Results.Add(
                context.Node.SyntaxTree.FilePath,
                context.Compilation.AssemblyName,
                new CodeSmellSummary(
                    CodeSmellSummary.CodeSmellType.TypeCast,
                    context.Node.GetLocation(),
                    context.Node
                    )
            );
        }

        public void RatingCompute (SyntaxNodeAnalysisContext context)
        {
            var all_casts = context.Node.DescendantNodes().OfType<CastExpressionSyntax>();
            if (all_casts.Count() > 0)
            {
                var rating = Math.Log(all_casts.Sum((x) => x.Span.Length)) / 8d;
                var diag = Diagnostic.Create(Rule, (context.Node as MethodDeclarationSyntax).Identifier.GetLocation(), rating);
                // context.ReportDiagnostic(diag);
            }
        }
    }
}
