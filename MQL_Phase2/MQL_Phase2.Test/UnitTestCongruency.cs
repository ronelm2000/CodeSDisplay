using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQL_Phase2.Test.IsCongurentTo
{
    [TestClass]
    public class UnitTestIsCongruentTo
    {
        [TestMethod]
        public void IsCongruentToIfStatement()
        {
            var testSubject1 = SyntaxFactory.ParseSyntaxTree(@"
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
                int[] stuff = new int[] {0, 1, 2, 3, 4, 5};
                if (stuff[0] == 0) {
                    a++;
                    a++;
                    a++;
                }
            }
        }
");
            var testSubject2 = SyntaxFactory.ParseSyntaxTree(@"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName2
        {   
            public void B () {
                int a = 0;
                int[] array = new int[] {0, 1, 2, 3, 4, 5};
                if (stuff[0] == 0) {
                    a++;
                    a++;
                    a++;
                }
            }
        }
");
            var if1 = testSubject1.GetRoot().DescendantNodes().OfType<IfStatementSyntax>().First();
            var if2 = testSubject2.GetRoot().DescendantNodes().OfType<IfStatementSyntax>().First();
            Assert.IsTrue(if1.IsCongruentTo(if2));
            Assert.IsTrue(if1.LinesRepresented() == 4);
        }
        [TestMethod]
        public void IsCongruentToIfStatementNegative()
        {
            var testSubject1 = SyntaxFactory.ParseSyntaxTree(@"
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
                int[] stuff = new int[] {0, 1, 2, 3, 4, 5};
                if (stuff[0] == 0) {
                    a++;
                    a++;
                    a++;
                }
            }
        }
");
            var testSubject2 = SyntaxFactory.ParseSyntaxTree(@"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName2
        {   
            public void B () {
                int a = 0;
                int[] array = new int[] {0, 1, 2, 3, 4, 5};
                if (stuff[0] == 0) {
                    a++;
                    a++;
                    a++;
                    array[0]++;
                }
            }
        }
");
            var if1 = testSubject1.GetRoot().DescendantNodes().OfType<IfStatementSyntax>().First();
            var if2 = testSubject2.GetRoot().DescendantNodes().OfType<IfStatementSyntax>().First();
            Assert.IsFalse(if1.IsCongruentTo(if2));
        }
        [TestMethod]
        public void IsCongruentToWhileStatement()
        {
            var testSubject1 = SyntaxFactory.ParseSyntaxTree(@"
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
                int[] stuff = new int[] {0, 1, 2, 3, 4, 5};
                while (true) {
                    a++;
                    a++;
                    a++;
                    if (a > 8) break;
                }
            }
        }
");
            var testSubject2 = SyntaxFactory.ParseSyntaxTree(@"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName2
        {   
            public void B () {
                int a = 0;
                int[] array = new int[] {0, 1, 2, 3, 4, 5};
                while (true) {
                    a++;
                    a++;
                    a++;
                    if (a > 8) break;
                }
            }
        }
");
            var if1 = testSubject1.GetRoot().DescendantNodes().OfType<WhileStatementSyntax>().First();
            var if2 = testSubject2.GetRoot().DescendantNodes().OfType<WhileStatementSyntax>().First();
            Assert.IsTrue(if1.IsCongruentTo(if2));
        }
    }
}
