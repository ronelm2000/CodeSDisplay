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
    public class MessageChainAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MessageChain";

        // You can change these strings in the MessageChainResources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        internal static readonly LocalizableString Title = new LocalizableResourceString(nameof(MessageChainResources.AnalyzerTitle), MessageChainResources.ResourceManager, typeof(MessageChainResources));
        internal static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(MessageChainResources.AnalyzerMessageFormat), MessageChainResources.ResourceManager, typeof(MessageChainResources));
        internal static readonly LocalizableString Description = new LocalizableResourceString(nameof(MessageChainResources.AnalyzerDescription), MessageChainResources.ResourceManager, typeof(MessageChainResources));
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
            var all_invocations = context.Node.DescendantNodes((x) => !(x.Parent is InvocationExpressionSyntax)).OfType<InvocationExpressionSyntax>();

            // it wasn't clear if invocation includes properties in C# since Java has no properties.
            double rating = 0;
            List<Location> listLocs = new List<Location>();
            List<double> chainSizes = new List<double>();
            foreach (InvocationExpressionSyntax invocation in all_invocations)
            {
                double chainSize = invocation.ComputeChain();
                if (chainSize > 1)
                {
                    listLocs.Add(invocation.GetLocation());
                    chainSizes.Add(chainSize);
                    rating += chainSize;
                }
            }

            // current Stench Blossom will only output method declarations with message chains equal to a certain rating.
            // this can be improved by instead detecting the actual invocation expressions!
            rating = Math.Log(rating, 2) / 8;

            if (rating > 0.1)
            {
                // found message chains
                for (int i = 0; i < listLocs.Count; i++)
                {
                    var diag = Diagnostic.Create(Rule, listLocs[i], rating * 100, chainSizes[i]);
                    context.ReportDiagnostic(diag);
                }
                Summary.Results.Add(
                    context.Node.SyntaxTree.FilePath,
                    context.Compilation.AssemblyName,
                    new CodeSmellSummary(
                        CodeSmellSummary.CodeSmellType.MessageChain,
                        context.Node.GetLocation(),
                        context.Node
                        )
                );
            }
            //});

        }
    }
}




