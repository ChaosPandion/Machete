using Machete.Core;
using Xunit;

namespace Machete.Tests.Expressions
{
    public sealed class GeneratorExpression : TestBase
    {
        [Fact(DisplayName = "GeneratorExpression: YieldStatement")]
        public void TestA()
        {
            var r = Engine.ExecuteScriptToDynamic(@"
                (function() {
                    var g = generator {
                        yield 1;
                        yield 1;
                        yield 1;
                        yield 1;
                        yield 1;
                    };
                    var r = 0;
                    foreach (var n in g) {
                        r += n;
                    }       
                    return r;
                })();
            ");
            Assert.IsAssignableFrom<INumber>(r);
            Assert.Equal(5.0, ((INumber)r).BaseValue);
        }

        [Fact(DisplayName = "GeneratorExpression: YieldStatement With YieldBreakStatement")]
        public void TestB()
        {
            var r = Engine.ExecuteScriptToDynamic(@"
                (function() {
                    var breakGen = true,
                    g = generator {
                        yield 1;
                        yield 1;
                        yield 1;
                        yield 1;
                        yield 1;
                        if (breakGen) {
                            yield break;
                        }
                        yield 1;
                    };
                    var r = 0;
                    foreach (var n in g) {
                        r += n;
                    }       
                    return r;
                })();
            ");
            Assert.IsAssignableFrom<INumber>(r);
            Assert.Equal(5.0, ((INumber)r).BaseValue);
        }

        [Fact(DisplayName = "GeneratorExpression: YieldStatement With YieldContinueStatement")]
        public void TestC()
        {
            var r = Engine.ExecuteScriptToDynamic(@"
                (function() {
                    var g1 = generator {
                            yield 1;
                            yield 1;
                            yield continue g2;
                        },
                        g2 = generator {
                            yield 1;
                            yield 1;
                            yield 1;
                        };
                    var r = 0;
                    foreach (var n in g) {
                        r += n;
                    }       
                    return r;
                })();
            ");
            Assert.IsAssignableFrom<INumber>(r);
            Assert.Equal(5.0, ((INumber)r).BaseValue);
        }
    }
}