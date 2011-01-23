using System;
using Xunit;

namespace Machete.Tests
{
    public class IterationStatements : TestBase
    {
        [Fact(DisplayName = "12.6 Iteration Statements -> do Statement while ( Expression );")]
        public void Test126A()
        {
            var script = @"
                var n = 1;
                do {
                    n++;
                } while (n < 100);
                return n;
            ";
            Assert.Equal(100.0, (double)Engine.ExecuteScript(script));
        }

        [Fact(DisplayName = "12.6 Iteration Statements -> while ( Expression ) Statement")]
        public void Test126B()
        {
            var script = @"
                var n = 1;
                while (n < 100) {
                    n++;
                }
                return n;
            ";
            Assert.Equal(100.0, (double)Engine.ExecuteScript(script));
        }

        [Fact(DisplayName = "12.6 Iteration Statements -> for ( ; ; ) Statement")]
        public void Test126C()
        {
            var script = @"
                var n = 1;
                for (;;) {
                    n *= 2;
                    if (n === 256) {
                        break;
                    }
                }
                return n;
            ";
            Assert.Equal(256.0, (double)Engine.ExecuteScript(script));
        }

        [Fact(DisplayName = "12.6 Iteration Statements -> for ( ; Expression ; ) Statement")]
        public void Test126D()
        {
            var script = @"
                var n = 1;
                for (; n !== 256;) {
                    n *= 2;
                }
                return n;
            ";
            Assert.Equal(256.0, (double)Engine.ExecuteScript(script));
        }

        [Fact(DisplayName = "12.6 Iteration Statements -> for ( ; ; Expression ) Statement")]
        public void Test126E()
        {
            var script = @"
                var n = 1;
                for (;; n *= 2) {
                    if (n === 256) {
                        break;
                    }
                }
                return n;
            ";
            Assert.Equal(256.0, (double)Engine.ExecuteScript(script));
        }

        [Fact(DisplayName = "12.6 Iteration Statements -> for ( ; Expression ; Expression ) Statement")]
        public void Test126F()
        {
            var script = @"
                var n = 1;
                for (; n !== 256; n *= 2) {

                }
                return n;
            ";
            Assert.Equal(256.0, (double)Engine.ExecuteScript(script));
        }

        [Fact(DisplayName = "12.6 Iteration Statements -> for ( ExpressionNoIn ; ; ) Statement")]
        public void Test126G()
        {
            var script = @"
                var b = 0;
                for ( b = 1; ; ) {
                    return b;
                }
            ";
            Assert.Equal(1.0, (double)Engine.ExecuteScript(script));
        }

        [Fact(DisplayName = "12.6 Iteration Statements -> for ( ExpressionNoIn ; Expression ; ) Statement")]
        public void Test126H()
        {
            var script = @"
                var b = 0, n = 0;
                for (b = 1; n !== 3;) {
                    n++;
                }
                return n - b;
            ";
            Assert.Equal(2.0, (double)Engine.ExecuteScript(script));
        }

        [Fact(DisplayName = "12.6 Iteration Statements -> for ( ExpressionNoIn ; ; Expression ) Statement")]
        public void Test126I()
        {
            var script = @"
                var b = 0, n = 0;
                for (b = 1; ; n++) {
                    if (n === 3) {
                        break;
                    }
                }
                return n - b;
            ";
            Assert.Equal(2.0, (double)Engine.ExecuteScript(script));
        }

        [Fact(DisplayName = "12.6 Iteration Statements -> for ( ExpressionNoIn ; Expression ; Expression ) Statement")]
        public void Test126J()
        {
            var script = @"
                var n = 1, b = 0;
                for (b = 1; n !== 256; n *= 2) {

                }
                return n - b;
            ";
            Assert.Equal(255.0, (double)Engine.ExecuteScript(script));
        }

        [Fact(DisplayName = "12.6 Iteration Statements -> for ( var VariableDeclarationListNoIn ; ; ) Statement")]
        public void Test126K()
        {
            var script = @"
                var r = 1;
                for (var b = 2;;) {
                    r += b;
                    break;
                }
                return r;
            ";
            Assert.Equal(3.0, (double)Engine.ExecuteScript(script));
        }

        [Fact(DisplayName = "12.6 Iteration Statements -> for ( var VariableDeclarationListNoIn ; Expression ; ) Statement")]
        public void Test126L()
        {
            var script = @"
                for (var n = 1; n !== 256;) {
                    n *= 2
                }
                return n;
            ";
            Assert.Equal(256.0, (double)Engine.ExecuteScript(script));
        }

        [Fact(DisplayName = "12.6 Iteration Statements -> for ( var VariableDeclarationListNoIn ; ; Expression ) Statement")]
        public void Test126M()
        {
            var script = @"
                for (var n = 1; ; n *= 2) {
                    if (n === 256) {
                        break;
                    }
                }
                return n;
            ";
            Assert.Equal(256.0, (double)Engine.ExecuteScript(script));
        }

        [Fact(DisplayName = "12.6 Iteration Statements -> for ( var VariableDeclarationListNoIn ; Expression ; Expression ) Statement")]
        public void Test126N()
        {
            ExpectDouble(@"
                for (var n = 1; n !== 256; n *= 2);
                return n;
            ", 256.0);
            ExpectDouble(@"
                var n = 0; 
                for (var i = 0; i < 1000; ++i)
                    if (i % 3 === 0 || i % 5 === 0)
                        n += i;
                return n;
            ", 233168.0);
        }

        [Fact(DisplayName = "12.6 Iteration Statements -> for ( LeftHandSideExpression in Expression ) Statement")]
        public void Test126O()
        {
            ExpectString(@"
                var r = {}, o = { a:1, b:2, c:3 };
                for (r in o) ;
                return r;
            ", "c");
        }

        [Fact(DisplayName = "12.6 Iteration Statements -> for ( var VariableDeclarationNoIn in Expression ) Statement")]
        public void Test126P()
        {
            var script = @"
                var r = '', o = { a:1, b:2, c:3 };
                for (var n in o) {
                    r += n;
                }
                return r;
            ";
            Assert.Equal("abc", (string)Engine.ExecuteScript(script));
        }
    }
}