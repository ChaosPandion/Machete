using System;
using Xunit;
using Machete.Interfaces;

namespace Machete.Tests
{
    public class NumberObjects : TestBase
    {
        [Fact(DisplayName = "15.7.1.1 Number ( [ value ] )")]
        public void Test15711()
        {
            Assert.Equal(100.0, (double)Engine.ExecuteScript("Number(100)"));
            Assert.Equal(0.0, (double)Engine.ExecuteScript("Number()"));
        }

        [Fact(DisplayName = "15.7.2.1 new Number ( [ value ] )")]
        public void Test15721()
        {
            Assert.Equal("object", (string)Engine.ExecuteScript("typeof (new Number(1))"));
            Assert.Equal(1.0, (double)Engine.ExecuteScript("+(new Number(1))"));
        }

        [Fact(DisplayName = "15.7.3.1 Number.prototype")]
        public void Test15731()
        {
            Assert.Equal("object", (string)Engine.ExecuteScript("typeof Number.prototype"));
        }

        [Fact(DisplayName = "15.7.3.2 Number.MAX_VALUE")]
        public void Test15732()
        {
            Assert.Equal(double.MaxValue, (double)Engine.ExecuteScript("Number.MAX_VALUE"));
        }

        [Fact(DisplayName = "15.7.3.3 Number.MIN_VALUE")]
        public void Test15733()
        {
            Assert.Equal(double.MinValue, (double)Engine.ExecuteScript("Number.MIN_VALUE"));
        }

        [Fact(DisplayName = "15.7.3.4 Number.NaN")]
        public void Test15734()
        {
            Assert.True(double.IsNaN((double)Engine.ExecuteScript("Number.NaN")));
        }

        [Fact(DisplayName = "15.7.3.5 Number.NEGATIVE_INFINITY")]
        public void Test15735()
        {
            Assert.True(double.IsNegativeInfinity((double)Engine.ExecuteScript("Number.NEGATIVE_INFINITY")));
        }

        [Fact(DisplayName = "15.7.3.6 Number.POSITIVE_INFINITY")]
        public void Test15736()
        {
            Assert.True(double.IsPositiveInfinity((double)Engine.ExecuteScript("Number.POSITIVE_INFINITY")));
        }

        [Fact(DisplayName = "15.7.4.1 Number.prototype.constructor")]
        public void Test15741()
        {
            Assert.True((bool)Engine.ExecuteScript("Number.prototype.constructor === Number"));
        }

        [Fact(DisplayName = "15.7.4.2 Number.prototype.toString ( [ radix ] )")]
        public void Test15742()
        {
            Assert.Equal("ff.266666", ((string)Engine.ExecuteScript("255.15.toString(16);")).Substring(0, 9));
        }

        [Fact(DisplayName = "15.7.4.3 Number.prototype.toLocaleString()")]
        public void Test15743()
        {
            Assert.Equal((999.99).ToString(), (string)Engine.ExecuteScript("999.99.toLocaleString();"));
        }

        [Fact(DisplayName = "15.7.4.4 Number.prototype.valueOf ( )")]
        public void Test15744()
        {
            Assert.Equal(999.999, (double)Engine.ExecuteScript("999.999.valueOf();"));
            Assert.Equal(999.999, (double)Engine.ExecuteScript("(new Number(999.999)).valueOf();"));
        }

        [Fact(DisplayName = "15.7.4.5 Number.prototype.toFixed (fractionDigits)")]
        public void Test15745()
        {
            Assert.Equal("999.99", (string)Engine.ExecuteScript("999.999.toFixed(2);"));
            Assert.Equal("999", (string)Engine.ExecuteScript("999.999.toFixed();"));
            Assert.Equal("NaN", (string)Engine.ExecuteScript("NaN.toFixed(2);"));
            Assert.IsAssignableFrom<MacheteRuntimeException>(Engine.ExecuteScript("999.999.toFixed(-1);"));
            Assert.IsAssignableFrom<MacheteRuntimeException>(Engine.ExecuteScript("999.999.toFixed(21);"));
        }

        [Fact(DisplayName = "15.7.4.6 Number.prototype.toExponential (fractionDigits)")]
        public void Test15746()
        {
            Assert.Equal("1.00e+3", (string)Engine.ExecuteScript("999.999.toExponential(2);"));
            Assert.Equal("9.99999e+2", (string)Engine.ExecuteScript("999.999.toExponential();"));
            Assert.Equal("NaN", (string)Engine.ExecuteScript("NaN.toExponential(2);"));
            Assert.Equal("Infinity", (string)Engine.ExecuteScript("Infinity.toExponential(2);"));
            Assert.IsAssignableFrom<MacheteRuntimeException>(Engine.ExecuteScript("999.999.toExponential(-1);"));
            Assert.IsAssignableFrom<MacheteRuntimeException>(Engine.ExecuteScript("999.999.toExponential(21);"));
        }

        [Fact(DisplayName = "15.7.4.7 Number.prototype.toPrecision (precision)")]
        public void Test15747()
        {
            Assert.Equal("1.e+3", (string)Engine.ExecuteScript("999.999.toPrecision(1);"));
            Assert.Equal("1.0e+3", (string)Engine.ExecuteScript("999.999.toPrecision(2);"));
            Assert.Equal("1.00e+3", (string)Engine.ExecuteScript("999.999.toPrecision(3);"));
            Assert.Equal("1000", (string)Engine.ExecuteScript("999.999.toPrecision(4);"));
            Assert.Equal("1000.0", (string)Engine.ExecuteScript("999.999.toPrecision(5);"));
            Assert.IsAssignableFrom<MacheteRuntimeException>(Engine.ExecuteScript("999.999.toPrecision(0);"));
            Assert.IsAssignableFrom<MacheteRuntimeException>(Engine.ExecuteScript("999.999.toPrecision(22);"));
        }
    }
}