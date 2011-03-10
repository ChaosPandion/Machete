using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Machete.Core;

namespace Machete.Tests.Statements
{
    public sealed class ForeachStatement : TestBase
    {
        [Fact(DisplayName = "Foreach - Iterate Over Array")]
        public void IterateOverArray()
        {
            var script = @"
                (function() {
                    var r = 0;
                    foreach (var n in [10, 10, 10, 10, 10]) {
                        r += n;
                    }
                    return r;
                })();
            ";
            var r = Engine.ExecuteScriptToDynamic(script);
            Assert.IsAssignableFrom<INumber>(r);
            Assert.Equal(50.0, ((INumber)r).BaseValue);
        }

        [Fact(DisplayName = "Foreach - Iterate Over String")]
        public void IterateOverString()
        {
            var script = @"
                (function() {
                    var r = '';
                    foreach (var c in 'ABC') {
                        r += c;
                    }
                    return r;
                })();
            ";
            var r = Engine.ExecuteScriptToDynamic(script);
            Assert.IsAssignableFrom<IString>(r);
            Assert.Equal("ABC", ((IString)r).BaseValue);
        }

        [Fact(DisplayName = "Foreach - Iterate Over Generator")]
        public void IterateOverGenerator()
        {
            var script = @"
                (function() {
                    var g = generator {
                        yield 10;
                        yield 10;
                        yield 10;
                        yield 10;
                        yield 10;
                    };
                    var r = 0;
                    foreach (var n in g) {
                        r += n;
                    }
                    return r;
                })();
            ";
            var r = Engine.ExecuteScriptToDynamic(script);
            Assert.IsAssignableFrom<INumber>(r);
            Assert.Equal(50.0, ((INumber)r).BaseValue);
        }

        [Fact(DisplayName = "Foreach - Iterate Over Custom Iterator")]
        public void IterateOverCustomIterator()
        {
            var script = @"
                (function() {
                    function Iterator() {
                        var i = 0, self = this;
 
                        this.current = null;

                        this.next = function() {
                            if (i < 5) {
                                self.current = ++i;
                                return true;
                            }
                            return false;    
                        };

                        this.createIterator = function() {
                            return this;
                        };
                    }
                    var r = 0, x = new Iterator();
                    foreach (var n in x) {
                        r += n;
                    }
                    return r;
                })();
            ";
            var r = Engine.ExecuteScriptToDynamic(script);
            Assert.IsAssignableFrom<INumber>(r);
            Assert.Equal(15.0, ((INumber)r).BaseValue);
        }

        [Fact(DisplayName = "Foreach - IfStatement")]
        public void LoopWithIfStatement()
        {
            var script = @"
                (function() {
                    var r = 0;
                    foreach (var n in [1, 2, 3, 4, 5, 6, 7, 8, 9, 10])
                        if (n % 2 === 0)
                            r += n;
                    return r;
                })();
            ";
            var r = Engine.ExecuteScriptToDynamic(script);
            Assert.IsAssignableFrom<INumber>(r);
            Assert.Equal(30.0, ((INumber)r).BaseValue);
        }
    }
}