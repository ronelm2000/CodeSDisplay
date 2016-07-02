using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace MQL_Phase2
{
    public static class SyntaxNodeExtensions
    {

        // the original only called to only check method name (or line type) but 
        // this was the one we could work on clearly.
        public static bool IsCongruentTo(this SyntaxNode one, SyntaxNode two)
        {
            // first check to deal with arguments and conditions. Those should be ignored.
            if (one is ArgumentListSyntax) return two is ArgumentListSyntax;
            if (one is ArgumentSyntax) return two is ArgumentSyntax;
            // if (one is BinaryExpressionSyntax) return two is BinaryExpressionSyntax;
            // if (one is EqualsValueClauseSyntax) return two is EqualsValueClauseSyntax;

            // Invocation aka assignments are also "ignored"
            // if (one is InvocationExpressionSyntax) return two is InvocationExpressionSyntax;

            // second check to avoid some unusual count errors
            var one_des = one.ChildNodes().ToImmutableArray();
            var two_des = two.ChildNodes().ToImmutableArray();
            if (one_des.Length != two_des.Length) return false;


            if (one_des.Length == 0)
            {
                return SyntaxFactory.AreEquivalent(one, two, true);
            }

            for (int i = 0; i < one_des.Length; i++)
            {
                // loop to keep on checking
                if (!one_des[i].IsCongruentTo(two_des[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static int LinesRepresented(this SyntaxNode one)
        {
            if (one is BlockSyntax)
            {
                int total = 0;
                foreach (StatementSyntax s in ((BlockSyntax)one).Statements)
                {
                    total += s.LinesRepresented();
                }
                return total;

                /*
                return ((BlockSyntax)one).Statements.Sum(x => 
                    x.LinesRepresented()
                );
                */
            }


            if (one is ForStatementSyntax ||
                one is ForEachStatementSyntax ||
                one is WhileStatementSyntax ||
                one is DoStatementSyntax ||
                one is UsingStatementSyntax ||
                one is LockStatementSyntax
                )
            {
                if (one.ChildNodes().OfType<BlockSyntax>().Count() > 0)
                {
                    return 1 + one.ChildNodes().OfType<BlockSyntax>().First().LinesRepresented();
                }
                else
                {
                    return 2;
                }
            }
            else if (one is IfStatementSyntax)
            {
                int lines = 1;
                IfStatementSyntax disOne = one as IfStatementSyntax;
                lines += disOne.Statement.LinesRepresented();
                if (one.ChildNodes().OfType<ElseClauseSyntax>().Count() > 0)
                {
                    lines += 1 + disOne.Else.Statement.LinesRepresented();
                }

                return lines;
            }
            else if (one is TryStatementSyntax)
            {
                return 1 + ((TryStatementSyntax)one).Block.LinesRepresented() + ((TryStatementSyntax)one).Catches.Sum(x => x.Block.LinesRepresented() + 1);
            }
            else if (one is SwitchStatementSyntax)
            {
                int lines = 1;
                SwitchStatementSyntax disOne = one as SwitchStatementSyntax;
                foreach (SwitchSectionSyntax section in disOne.Sections)
                {
                    lines += 1 + section.Statements.Sum(x => x.LinesRepresented());
                }
                return lines;
            }

            return 1;
        }

        public static double ComputeChain(this InvocationExpressionSyntax invocation)
        {
            // this checks only for A().B()
            // in the future we can check for A.B.C
            // in the next we can check for A.B().C.D().E().F.G();
            InvocationExpressionSyntax next = (invocation?.Expression as MemberAccessExpressionSyntax)?.Expression as InvocationExpressionSyntax;
            if (next == null)
                return 1;
            return 1 + ComputeChain(next);
        }

        public static string Format(this Location location)
        {
            var pos = location.GetLineSpan();
            var path = pos.Path.Substring(pos.Path.LastIndexOf('\\') + 1);
            var line = pos.StartLinePosition.Line + 1;
            return path + ": Line " + line;
        }

        public static Location HighlightOneLine (this SyntaxNode node)
        {
            if (node is IfStatementSyntax) return Location.Create(
                node.SyntaxTree,
                new TextSpan(
                    (node as IfStatementSyntax).IfKeyword.GetLocation().SourceSpan.Start,
                    (node as IfStatementSyntax).CloseParenToken.GetLocation().SourceSpan.End -
                    (node as IfStatementSyntax).IfKeyword.GetLocation().SourceSpan.Start
                    )
                    );

            if (node is ForStatementSyntax) return Location.Create(
                node.SyntaxTree,
                new TextSpan(
                    (node as ForStatementSyntax).ForKeyword.GetLocation().SourceSpan.Start,
                    (node as ForStatementSyntax).CloseParenToken.GetLocation().SourceSpan.End -
                    (node as ForStatementSyntax).ForKeyword.GetLocation().SourceSpan.Start
                    )
                    );
            
            if (node is ForEachStatementSyntax) return Location.Create(
                node.SyntaxTree,
                new TextSpan(
                    (node as ForEachStatementSyntax).ForEachKeyword.GetLocation().SourceSpan.Start,
                    (node as ForEachStatementSyntax).CloseParenToken.GetLocation().SourceSpan.End -
                    (node as ForEachStatementSyntax).ForEachKeyword.GetLocation().SourceSpan.Start
                )
                );

            if (node is DoStatementSyntax) return (node as DoStatementSyntax).DoKeyword.GetLocation();
            if (node is TryStatementSyntax) return (node as TryStatementSyntax).TryKeyword.GetLocation();

            if (node is WhileStatementSyntax) return Location.Create(
                node.SyntaxTree,
                new TextSpan(
                    (node as WhileStatementSyntax).WhileKeyword.GetLocation().SourceSpan.Start,
                    (node as WhileStatementSyntax).CloseParenToken.GetLocation().SourceSpan.End -
                    (node as WhileStatementSyntax).WhileKeyword.GetLocation().SourceSpan.Start
                )
                );

            if (node is UsingStatementSyntax) return Location.Create(
                node.SyntaxTree,
                new TextSpan(
                    (node as UsingStatementSyntax).UsingKeyword.GetLocation().SourceSpan.Start,
                    (node as UsingStatementSyntax).CloseParenToken.GetLocation().SourceSpan.End -
                    (node as UsingStatementSyntax).UsingKeyword.GetLocation().SourceSpan.Start
                )
                );

            if (node is SwitchStatementSyntax) return Location.Create(
                node.SyntaxTree,
                new TextSpan(
                    (node as SwitchStatementSyntax).SwitchKeyword.GetLocation().SourceSpan.Start,
                    (node as SwitchStatementSyntax).CloseParenToken.GetLocation().SourceSpan.End -
                    (node as SwitchStatementSyntax).SwitchKeyword.GetLocation().SourceSpan.Start
                )
                );

            if (node is LockStatementSyntax) return Location.Create(
                node.SyntaxTree,
                new TextSpan(
                    (node as LockStatementSyntax).LockKeyword.GetLocation().SourceSpan.Start,
                    (node as LockStatementSyntax).CloseParenToken.GetLocation().SourceSpan.End -
                    (node as LockStatementSyntax).LockKeyword.GetLocation().SourceSpan.Start
                )
                );

            return node.GetLocation();
            // Would cover ridiculously everything and anything C# were to add in the future. ~.~
            /*
            if (node is IBlockKeywordStatementSyntax) return Location.Create(
                node.SyntaxTree,
                new TextSpan(
                    (node as IBlockKeywordStatementSyntax).Keyword.GetLocation().SourceSpan.Start,
                    (node as IBlockKeywordStatementSyntax).InsideStatement.GetLocation().SourceSpan.Start -
                    (node as IBlockKeywordStatementSyntax).Keyword.GetLocation().SourceSpan.Start
                    )
                );
                */
        }

        public static string ShortenAbolutePath(this string str)
        {
            return str.Substring(str.LastIndexOf('\\') + 1);
        }

        public static IEnumerable<int> GetLines(this Location loc)
        {
            var start_line = loc.GetLineSpan().StartLinePosition.Line;
            var end_line = loc.GetLineSpan().EndLinePosition.Line;
            for (int i = 0; i <= end_line - start_line; i++) yield return start_line + i;
            /*
            return loc.SourceTree.GetRoot()
                .DescendantNodes()
                .Where((x) => x.Span.IntersectsWith(loc.SourceSpan))
                .Select((x) => x.GetLocation().GetMappedLineSpan().StartLinePosition.Line);
                */
            
        }

    }

}
