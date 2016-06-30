using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using MQL_Phase2;
using DiagnosticAnalyzerAndCodeFix;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MQL_Phase2.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {

        /*
        //No diagnostics expected to show up
        [TestMethod]
        public void TestMethod1()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void TestMethod2()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = TypeCastAnalyzer.DiagnosticId,
                Message = String.Format("Type name '{0}' contains lowercase letters", "TypeName"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 11, 15)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TYPENAME
        {   
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }
        */

        [TestMethod]
        public void LinesRepresentedTestFor()
        {
            var testSubject = SyntaxFactory.ParseSyntaxTree(@"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            public void B () {
                int a = 0;
                for (int i = 0; i < 3; i++) {
                    a++;
                    a++;
                    a++;
                }
            }
        }
");
            var forStatement = testSubject.GetRoot().DescendantNodes().OfType<ForStatementSyntax>().First();
            int represented = forStatement.LinesRepresented();
            Assert.IsTrue(represented == 4);   
        }
        [TestMethod]
        public void LinesRepresentedTestForEeach()
        {
            var testSubject = SyntaxFactory.ParseSyntaxTree(@"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            public void B () {
                int a = 0;
                int[] array = new int[] {0, 1, 2, 3, 4, 5};
                foreach (int x in array) {
                    a++;
                    a++;
                    a++;
                }
            }
        }
");
            var forStatement = testSubject.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().First();
            int represented = forStatement.LinesRepresented();
            Assert.IsTrue(represented == 4);
        }
        [TestMethod]
        public void LinesRepresentedIf()
        {
            var testSubject = SyntaxFactory.ParseSyntaxTree(@"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            public void B () {
                int a = 0;
                int[] array = new int[] {0, 1, 2, 3, 4, 5};
                if (a == 0) {
                    a++;
                    a++;
                } else {
                    a++;
                }
            }
        }
");
            var forStatement = testSubject.GetRoot().DescendantNodes().OfType<IfStatementSyntax>().First();
            int represented = forStatement.LinesRepresented();
            Assert.IsTrue(represented == 5);
        }
        [TestMethod]
        public void LinesRepresentedSwitch()
        {
            var testSubject = SyntaxFactory.ParseSyntaxTree(@"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            public void B () {
                int a = 0;
                int[] array = new int[] {0, 1, 2, 3, 4, 5};
                switch (a) {
                    case 0:
                        a++;
                        break;
                    case 1:
                        a++;
                        a++;
                        a++;
                        break;
                    case 2:
                    {
                        a++;
                        break;
                    }
                    default:
                        break;
                }
            }
        }
");
            var forStatement = testSubject.GetRoot().DescendantNodes().OfType<SwitchStatementSyntax>().First();
            int represented = forStatement.LinesRepresented();
            Assert.IsTrue(represented == 14);
        }
        [TestMethod]
        public void LinesRepresentedDo()
        {
            var testSubject = SyntaxFactory.ParseSyntaxTree(@"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            public void B () {
                int a = 0;
                int[] array = new int[] {0, 1, 2, 3, 4, 5};
                do B(); while (true);
            }
        }
");
            var forStatement = testSubject.GetRoot().DescendantNodes().OfType<DoStatementSyntax>().First();
            int represented = forStatement.LinesRepresented();
            Assert.IsTrue(represented == 2);
        }
        

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new MQL_Phase2CodeFixProvider();
        }
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new TypeCastAnalyzer();
        }
    }
}