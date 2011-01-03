using System;
using Xunit;

namespace Machete.Tests
{
    public class GlobalObject : TestBase
    {
        [Fact(DisplayName = "15.1.1.1 NaN")]
        public void Test15111()
        {
            Assert.Equal("number", (string)Engine.ExecuteScript("typeof NaN"));
            Assert.True(double.IsNaN((double)Engine.ExecuteScript("NaN")));
        }

        [Fact(DisplayName = "15.1.1.2 Infinity")]
        public void Test15112()
        {
            Assert.Equal("number", (string)Engine.ExecuteScript("typeof Infinity"));
            Assert.True(double.IsInfinity((double)Engine.ExecuteScript("Infinity")));
        }

        [Fact(DisplayName = "15.1.1.3 undefined")]
        public void Test15113()
        {
            Assert.Equal("undefined", (string)Engine.ExecuteScript("typeof undefined"));
        }

        [Fact(DisplayName = "15.1.2.1 eval (x)")]
        public void Test15121()
        {
            Assert.Equal(999e2, (double)Engine.ExecuteScript("eval('999e2')"));
        }

        [Fact(DisplayName = "15.1.2.2 parseInt (string , radix)")]
        public void Test15122()
        {
            Assert.Equal(100.0, (double)Engine.ExecuteScript("parseInt('100', 10)"));
            Assert.Equal(100.0, (double)Engine.ExecuteScript("parseInt('    100', 10)"));
            Assert.Equal(100.0, (double)Engine.ExecuteScript("parseInt('    100_', 10)"));
            Assert.Equal(255.0, (double)Engine.ExecuteScript("parseInt('0xFF', 16)"));
            Assert.Equal(255.0, (double)Engine.ExecuteScript("parseInt('    0xFF', 16)"));
            Assert.Equal(255.0, (double)Engine.ExecuteScript("parseInt('    0xFF_', 16)"));
            Assert.Equal(35.0, (double)Engine.ExecuteScript("parseInt('z', 36)"));
            Assert.Equal(35.0, (double)Engine.ExecuteScript("parseInt('    z', 36)"));
            Assert.Equal(35.0, (double)Engine.ExecuteScript("parseInt('    z_', 36)"));
            Assert.Equal(100.0, (double)Engine.ExecuteScript("parseInt('100')"));
            Assert.Equal(100.0, (double)Engine.ExecuteScript("parseInt('    100')"));
            Assert.Equal(100.0, (double)Engine.ExecuteScript("parseInt('    100_')"));
            Assert.Equal(255.0, (double)Engine.ExecuteScript("parseInt('0xFF')"));
            Assert.Equal(255.0, (double)Engine.ExecuteScript("parseInt('    0xFF')"));
            Assert.Equal(255.0, (double)Engine.ExecuteScript("parseInt('    0xFF_')"));
            Assert.True(double.IsNaN((double)Engine.ExecuteScript("parseInt('test', 10)")));
        }

        [Fact(DisplayName = "15.1.2.3 parseFloat (string)")]
        public void Test15123()
        {
            Assert.Equal(100.0, (double)Engine.ExecuteScript("parseFloat('100')"));
            Assert.Equal(100.0, (double)Engine.ExecuteScript("parseFloat(' 100')"));
            Assert.Equal(100.0, (double)Engine.ExecuteScript("parseFloat(' 100x')"));
            Assert.True(double.IsNaN((double)Engine.ExecuteScript("parseFloat('test')")));
        }

        [Fact(DisplayName = "15.1.2.4 isNaN (number)")]
        public void Test15124()
        {
            Assert.True((bool)Engine.ExecuteScript("isNaN(NaN)"));
            Assert.False((bool)Engine.ExecuteScript("isNaN(100)"));
        }

        [Fact(DisplayName = "15.1.2.5 isFinite (number)")]
        public void Test15125()
        {
            Assert.True((bool)Engine.ExecuteScript("isFinite(100)"));
            Assert.False((bool)Engine.ExecuteScript("isFinite(NaN)"));
            Assert.False((bool)Engine.ExecuteScript("isFinite(Number.POSITIVE_INFINITY)"));
            Assert.False((bool)Engine.ExecuteScript("isFinite(Number.NEGATIVE_INFINITY)"));
        }

        [Fact(DisplayName = "15.1.3.1 decodeURI (encodedURI)")]
        public void Test15131()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.1.3.2 decodeURIComponent (encodedURIComponent)")]
        public void Test15132()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.1.3.3 encodeURI (uri)")]
        public void Test15133()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.1.3.4 encodeURIComponent (uriComponent)")]
        public void Test15134()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.1.4.1 Object ( . . . )")]
        public void Test15141()
        {
            Assert.Equal("function", (string)Engine.ExecuteScript("typeof Object"));
        }

        [Fact(DisplayName = "15.1.4.2 Function ( . . . )")]
        public void Test15142()
        {
            Assert.Equal("function", (string)Engine.ExecuteScript("typeof Function"));
        }

        [Fact(DisplayName = "15.1.4.3 Array ( . . . )")]
        public void Test15143()
        {
            Assert.Equal("function", (string)Engine.ExecuteScript("typeof Array"));
        }

        [Fact(DisplayName = "15.1.4.4 String ( . . . )")]
        public void Test15144()
        {
            Assert.Equal("function", (string)Engine.ExecuteScript("typeof String"));
        }

        [Fact(DisplayName = "15.1.4.5 Boolean ( . . . )")]
        public void Test15145()
        {
            Assert.Equal("function", (string)Engine.ExecuteScript("typeof Boolean"));
        }

        [Fact(DisplayName = "15.1.4.6 Number ( . . . )")]
        public void Test15146()
        {
            Assert.Equal("function", (string)Engine.ExecuteScript("typeof Number"));
        }

        [Fact(DisplayName = "15.1.4.7 Date ( . . . )")]
        public void Test15147()
        {
            Assert.Equal("function", (string)Engine.ExecuteScript("typeof Date"));
        }

        [Fact(DisplayName = "15.1.4.8 RegExp ( . . . )")]
        public void Test15148()
        {
            Assert.Equal("function", (string)Engine.ExecuteScript("typeof RegExp"));
        }

        [Fact(DisplayName = "15.1.4.9 Error ( . . . )")]
        public void Test15149()
        {
            Assert.Equal("function", (string)Engine.ExecuteScript("typeof Error"));
        }

        [Fact(DisplayName = "15.1.4.10 EvalError ( . . . )")]
        public void Test151410()
        {
            Assert.Equal("function", (string)Engine.ExecuteScript("typeof EvalError"));
        }

        [Fact(DisplayName = "15.1.4.11 RangeError ( . . . )")]
        public void Test151411()
        {
            Assert.Equal("function", (string)Engine.ExecuteScript("typeof RangeError"));
        }

        [Fact(DisplayName = "15.1.4.12 ReferenceError ( . . . )")]
        public void Test151412()
        {
            Assert.Equal("function", (string)Engine.ExecuteScript("typeof ReferenceError"));
        }

        [Fact(DisplayName = "15.1.4.13 SyntaxError ( . . . )")]
        public void Test151413()
        {
            Assert.Equal("function", (string)Engine.ExecuteScript("typeof SyntaxError"));
        }

        [Fact(DisplayName = "15.1.4.14 TypeError ( . . . )")]
        public void Test151414()
        {
            Assert.Equal("function", (string)Engine.ExecuteScript("typeof TypeError"));
        }

        [Fact(DisplayName = "15.1.4.15 URIError ( . . . )")]
        public void Test151415()
        {
            Assert.Equal("function", (string)Engine.ExecuteScript("typeof URIError"));
        }

        [Fact(DisplayName = "15.1.5.1 Math")]
        public void Test15151()
        {
            Assert.Equal("object", (string)Engine.ExecuteScript("typeof Math"));
        }

        [Fact(DisplayName = "15.1.5.2 JSON")]
        public void Test15152()
        {
            Assert.Equal("object", (string)Engine.ExecuteScript("typeof JSON"));
        }
    }
}