using System;
using Xunit;

namespace Machete.Tests
{
    public class ArrayObjects : TestBase
    {
        [Fact(DisplayName = "15.4.1.1 Array ( [ item1 [ , item2 [ , … ] ] ] )")]
        public void Test15411()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.4.2.1 new Array ( [ item0 [ , item1 [ , … ] ] ] )")]
        public void Test15421()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.4.2.2 new Array (len)")]
        public void Test15422()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.4.3.1 Array.prototype")]
        public void Test15431()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.4.3.2 Array.isArray ( arg )")]
        public void Test15432()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.4.4.1 Array.prototype.constructor")]
        public void Test15441()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.4.4.2 Array.prototype.toString ( )")]
        public void Test15442()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.4.4.3 Array.prototype.toLocaleString ( )")]
        public void Test15443()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.4.4.4 Array.prototype.concat ( [ item1 [ , item2 [ , … ] ] ] )")]
        public void Test15444()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.4.4.5 Array.prototype.join (separator)")]
        public void Test15445()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.4.4.6 Array.prototype.pop ( )")]
        public void Test15446()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.4.4.7 Array.prototype.push ( [ item1 [ , item2 [ , … ] ] ] )")]
        public void Test15447()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.4.4.8 Array.prototype.reverse ( )")]
        public void Test15448()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.4.4.9 Array.prototype.shift ( )")]
        public void Test15449()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.4.4.10 Array.prototype.slice (start, end)")]
        public void Test154410()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.4.4.11 Array.prototype.sort (comparefn)")]
        public void Test154411()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.4.4.12 Array.prototype.splice (start, deleteCount [ , item1 [ , item2 [ , … ] ] ] )")]
        public void Test154412()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.4.4.13 Array.prototype.unshift ( [ item1 [ , item2 [ , … ] ] ] )")]
        public void Test154413()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.4.4.14 Array.prototype.indexOf ( searchElement [ , fromIndex ] )")]
        public void Test154414()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.4.4.15 Array.prototype.lastIndexOf ( searchElement [ , fromIndex ] )")]
        public void Test154415()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.4.4.16 Array.prototype.every ( callbackfn [ , thisArg ] )")]
        public void Test154416()
        {
            var script = @"
                function every(value, index, obj) {
                    return value >= 10;
                }
                ([10, 20, 30, 40, 50, 60]).every(every);
            ";
            Assert.True((bool)Engine.ExecuteScript(script));
        }

        [Fact(DisplayName = "15.4.4.17 Array.prototype.some ( callbackfn [ , thisArg ] )")]
        public void Test154417()
        {
            var script = @"
                function some(value, index, obj) {
                    return value >= 10;
                }
                ([1, 2, 3, 4, 5, 10]).some(some);
            ";
            Assert.True((bool)Engine.ExecuteScript(script));
        }

        [Fact(DisplayName = "15.4.4.18 Array.prototype.forEach ( callbackfn [ , thisArg ] )")]
        public void Test154418()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.4.4.19 Array.prototype.map ( callbackfn [ , thisArg ] )")]
        public void Test154419()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.4.4.20 Array.prototype.filter ( callbackfn [ , thisArg ] )")]
        public void Test154420()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.4.4.21 Array.prototype.reduce ( callbackfn [ , initialValue ] )")]
        public void Test154421()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.4.4.22 Array.prototype.reduceRight ( callbackfn [ , initialValue ] )")]
        public void Test154422()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.4.5.1 [[DefineOwnProperty]] ( P, Desc, Throw )")]
        public void Test15451()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.4.5.2 length")]
        public void Test15452()
        {
            Assert.True(false);
        }
    }
}