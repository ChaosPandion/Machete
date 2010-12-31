using System;
using Xunit;

namespace Machete.Tests
{
    public class NumberObjects : TestBase
    {
        [Fact(DisplayName = "15.7.1.1 Number ( [ value ] )")]
        public void Test15711()
        {
            Assert.Equal("number", (string)Engine.ExecuteScript("typeof Number(true)"));
            Assert.Equal(0.0, (double)Engine.ExecuteScript("Number()"));
        }

        [Fact(DisplayName = "15.7.2.1 new Number ( [ value ] )")]
        public void Test15721()
        {
            Assert.IsNotType<Exception>(Engine.ExecuteScript("var o = new Number(1);"));
            Assert.Equal("object", (string)Engine.ExecuteScript("typeof o"));
            Assert.Equal(1.0, (double)Engine.ExecuteScript("+o"));
            Assert.True((bool)Engine.ExecuteScript("delete o"));
        }

        [Fact(DisplayName = "15.7.3.1 Number.prototype")]
        public void Test15731()
        {
            Assert.Equal("object", (string)Engine.ExecuteScript("typeof Number.prototype"));
        }

        [Fact(DisplayName = "15.7.3.2 Number.MAX_VALUE")]
        public void Test15732()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.7.3.3 Number.MIN_VALUE")]
        public void Test15733()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.7.3.4 Number.NaN")]
        public void Test15734()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.7.3.5 Number.NEGATIVE_INFINITY")]
        public void Test15735()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.7.3.6 Number.POSITIVE_INFINITY")]
        public void Test15736()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.7.4.1 Number.prototype.constructor")]
        public void Test15741()
        {
            Assert.True((bool)Engine.ExecuteScript("Number.prototype.constructor === Number"));
        }

        [Fact(DisplayName = "15.7.4.2 Number.prototype.toString ( [ radix ] )")]
        public void Test15742()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.7.4.3 Number.prototype.toLocaleString()")]
        public void Test15743()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.7.4.4 Number.prototype.valueOf ( )")]
        public void Test15744()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.7.4.5 Number.prototype.toFixed (fractionDigits)")]
        public void Test15745()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.7.4.6 Number.prototype.toExponential (fractionDigits)")]
        public void Test15746()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.7.4.7 Number.prototype.toPrecision (precision)")]
        public void Test15747()
        {
            Assert.True(false);
        }
    }
}