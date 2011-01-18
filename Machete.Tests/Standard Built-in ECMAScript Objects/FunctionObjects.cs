using System;
using Xunit;

namespace Machete.Tests
{
    public class FunctionObjects : TestBase
    {
        [Fact(DisplayName = "15.3.1.1 Function (p1, p2, … , pn, body)")]
        public void Test15311()
        {
            var script = @"
                (function() {
                    var add = Function('x', 'y', 'return x + y');
                    return add(1, 2);
                })();
            ";
            ExpectDouble(script, 3.0);
        }

        [Fact(DisplayName = "15.3.2.1 new Function (p1, p2, … , pn, body)")]
        public void Test15321()
        {
            var script = @"
                (function() {
                    var add = new Function('x', 'y', 'return x + y');
                    return add(1, 2);
                })();
            ";
            ExpectDouble(script, 3.0);
        }

        [Fact(DisplayName = "15.3.3.1 Function.prototype")]
        public void Test15331()
        {
            Assert.True((bool)Engine.ExecuteScript("Function.prototype.isPrototypeOf(Object)"));
        }

        [Fact(DisplayName = "15.3.3.2 Function.length")]
        public void Test15332()
        {
            Assert.True((bool)Engine.ExecuteScript("Function.length === 1"));
        }

        [Fact(DisplayName = "15.3.4.1 Function.prototype.constructor")]
        public void Test15341()
        {
            Assert.True((bool)Engine.ExecuteScript("Function.prototype.constructor === Function"));
        }

        [Fact(DisplayName = "15.3.4.2 Function.prototype.toString ( )")]
        public void Test15342()
        {
            Assert.True((bool)Engine.ExecuteScript("parseInt.toString() === '[object, Function]'"));
        }

        [Fact(DisplayName = "15.3.4.3 Function.prototype.apply (thisArg, argArray)")]
        public void Test15343()
        {
            var script = @"
                (function() {
                    var ags = [1, 2];
                    var obj = {
                        value: 100
                    };
                    function add(x, y) {
                        return this.value + x + y;
                    }
                    return add.apply(obj, ags);
                })();
            ";
            ExpectDouble(script, 103.0);
        }

        [Fact(DisplayName = "15.3.4.4 Function.prototype.call (thisArg [ , arg1 [ , arg2, … ] ] )")]
        public void Test15344()
        {
            var script = @"
                (function() {
                    var obj = {
                        value: 999
                    };
                    function add(x, y) {
                        return x + y;
                    }
                    function callAdd(y) {
                        return add(this.value, y);
                    }
                    return callAdd.call(obj, 1);
                })();
            ";
            ExpectDouble(script, 1000.0);
        }

        [Fact(DisplayName = "15.3.4.5 Function.prototype.bind (thisArg [, arg1 [, arg2, …]])")]
        public void Test15345()
        {
            var script = @"
                (function() {
                    function add(x, y) {
                        return x + y;
                    }
                    var add1 = add.bind(this, 1);
                    return add1(2);
                })();
            ";
            ExpectDouble(script, 3.0);
        }
    }
}